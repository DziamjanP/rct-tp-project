using Microsoft.EntityFrameworkCore;
using CourseProject.Data;
using CourseProject.Models;
using CourseProject.Dtos;
using CourseProject.Services;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
var conn = builder.Configuration.GetConnectionString("DefaultConnection") 
           ?? throw new InvalidOperationException("Missing DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(conn));

builder.Services.AddSingleton<JwtService>();

var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Missing Jwt:Key");
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // in dev true for prod
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = !string.IsNullOrEmpty(jwtIssuer),
        ValidIssuer = jwtIssuer,
        ValidateAudience = !string.IsNullOrEmpty(jwtAudience),
        ValidAudience = jwtAudience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1)
    };
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Admin", policy =>
        policy.RequireAssertion(context =>
        {
            var claim = context.User.FindFirst("access_level")?.Value;
            if (string.IsNullOrEmpty(claim)) return false;
            return int.TryParse(claim, out var level) && level > 1;
        })
    )
    .AddPolicy("Support", policy =>
        policy.RequireAssertion(context =>
        {
            var claim = context.User.FindFirst("access_level")?.Value;
            if (string.IsNullOrEmpty(claim)) return false;
            return int.TryParse(claim, out var level) && level > 0;
        })
    ).AddPolicy("User", policy =>
        policy.RequireAssertion(context =>
        {
            var claim = context.User.FindFirst("access_level")?.Value;
            if (string.IsNullOrEmpty(claim)) return false;
            return int.TryParse(claim, out var level) && level >= 0;
        })
    );

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddHostedService<ExpiredLockCleanup>();
builder.Services.AddScoped<IStationDistanceCalculator, StationDistanceCalculator>();
builder.Services.AddScoped<ITicketPricingService, TicketPricingService>();

string[] devOrigins =
{
    "http://localhost:5173",
    "http://localhost:3000"
};

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy
            .WithOrigins(devOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.Configure<RouteOptions>(options =>
{
    options.AppendTrailingSlash = false;
    options.LowercaseUrls = true;
});

builder.Services
  .AddControllers()
  .AddJsonOptions(opt =>
  {
      opt.JsonSerializerOptions.ReferenceHandler = 
          System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
  });

var app = builder.Build();

app.UseCors("DevCors");

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


// automigrate in dev
using (var scope = app.Services.CreateScope())
{
    var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (env.IsDevelopment())
    {
        db.Database.EnsureCreated();
        db.Database.Migrate();
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUi(options =>
    {
        options.DocumentPath = "/openapi/v1.json";
    });
}

app.MapGet("/health", () => Results.Ok("OK"));


app.MapPost("/auth/register", async (AppDbContext db, JwtService jwt, RegisterDto dto) =>
{
    if (string.IsNullOrWhiteSpace(dto.Phone) || string.IsNullOrWhiteSpace(dto.Password))
        return Results.BadRequest("Phone and Password are required.");

    var existing = await db.Users.FirstOrDefaultAsync(u => u.Phone == dto.Phone);
    if (existing != null)
        return Results.Conflict("Phone already exists.");

    var salt = BCrypt.Net.BCrypt.GenerateSalt();
    var hash = BCrypt.Net.BCrypt.HashPassword(dto.Password, salt);

    var user = new User
    {
        Phone = dto.Phone,
        Name = dto.Name,
        Surname = dto.Surname,
        Password = hash,
        Salt = salt,
        AccessLevel = 0
    };

    db.Users.Add(user);
    await db.SaveChangesAsync();

    var token = jwt.GenerateToken(user.Id, user.AccessLevel, user.Name, user.Phone);
    var response = new AuthResponseDto(token, user.Id, user.AccessLevel, user.Name ?? "", user.Surname ?? "", user.Passport ?? "", user.Phone ?? "");
    return Results.Ok(response);
});


app.MapPost("/auth/login", async (AppDbContext db, JwtService jwt, LoginDto dto) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Phone == dto.Phone);
    if (user == null) return Results.Unauthorized();

    if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
        return Results.Unauthorized();

    var accessToken = jwt.GenerateToken(user.Id, user.AccessLevel, user.Name, user.Phone);
    var refreshToken = jwt.GenerateRefreshToken();

    db.RefreshTokens.Add(new RefreshToken {
        UserId = user.Id,
        Token = refreshToken,
        ExpiresAt = DateTime.UtcNow.AddDays(7)
    });

    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        accessToken,
        refreshToken,
        userId = user.Id,
        accessLevel = user.AccessLevel,
        name = user.Name,
        phone = user.Phone
    });
});


app.MapPost("/auth/refresh", async (AppDbContext db, JwtService jwt, RefreshTokenDto refreshTokenDto) =>
{
    var tokenRow = await db.RefreshTokens
        .Include(t => t.User)
        .FirstOrDefaultAsync(t =>
            t.Token == refreshTokenDto.RefreshToken &&
            !t.Revoked &&
            t.ExpiresAt > DateTime.UtcNow
        );

    if (tokenRow == null)
        return Results.Unauthorized();

    var user = tokenRow.User;

    tokenRow.Revoked = true;

    var newRefreshToken = jwt.GenerateRefreshToken();
    var newAccessToken = jwt.GenerateToken(
        user.Id,
        user.AccessLevel,
        user.Name,
        user.Phone
    );

    db.RefreshTokens.Add(new RefreshToken {
        UserId = user.Id,
        Token = newRefreshToken,
        ExpiresAt = DateTime.UtcNow.AddDays(7)
    });

    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        accessToken = newAccessToken,
        refreshToken = newRefreshToken
    });
});

app.MapGet("/users", async (AppDbContext db) => await db.Users.ToListAsync());
app.MapGet("/users/{id:long}", async (AppDbContext db, long id) =>
    await db.Users.FindAsync(id) is User u ? Results.Ok(u) : Results.NotFound());

app.MapDelete("/users/{id:long}", async (AppDbContext db, long id) =>
{
    var u = await db.Users.FindAsync(id);
    if (u == null) return Results.NotFound();

    db.Users.Remove(u);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Stations
app.MapGet("/stations", async (AppDbContext db) => await db.Stations.ToListAsync());
app.MapGet("/stations/{id:long}", async (AppDbContext db, long id) =>
    await db.Stations.FindAsync(id) is Station u ? Results.Ok(u) : Results.NotFound());
app.MapPost("/stations", async (AppDbContext db, Station s) =>
{
    db.Stations.Add(s);
    await db.SaveChangesAsync();
    return Results.Created($"/stations/{s.Id}", s);
}).RequireAuthorization("Admin");
app.MapDelete("/stations/{id:long}", async (AppDbContext db, long id) =>
{

    var s = await db.Stations.FindAsync(id);
    if (s == null) return Results.NotFound();

    db.Stations.Remove(s);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization("Admin");

// Train types
app.MapGet("/traintypes", async (AppDbContext db) => await db.TrainTypes.ToListAsync());
app.MapGet("/traintypes/{id:long}", async (AppDbContext db, long id) =>
    await db.TrainTypes.FindAsync(id) is TrainType u ? Results.Ok(u) : Results.NotFound());
app.MapPost("/traintypes", async (AppDbContext db, TrainType t) =>
{
    db.TrainTypes.Add(t);
    await db.SaveChangesAsync();
    return Results.Created($"/traintypes/{t.Id}", t);
}).RequireAuthorization("Admin");
app.MapDelete("/traintypes/{id:long}", async (AppDbContext db, long id) =>
{
    var t = await db.TrainTypes.FindAsync(id);
    if (t == null) return Results.NotFound();

    db.TrainTypes.Remove(t);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization("Admin");

// Trains
app.MapGet("/trains", async (AppDbContext db) => await db.Trains.Include(t => t.Source).Include(t => t.Destination).Include(t => t.Type).ToListAsync());
app.MapGet("/trains/{id:long}", async (AppDbContext db, long id) =>
    await db.Trains.FindAsync(id) is Train u ? Results.Ok(u) : Results.NotFound());
app.MapPost("/trains", async (AppDbContext db, Train t) =>
{
    db.Trains.Add(t);
    await db.SaveChangesAsync();
    return Results.Created($"/trains/{t.Id}", t);
}).RequireAuthorization("Admin");
app.MapDelete("/trains/{id:long}", async (AppDbContext db, long id) =>
{
    var t = await db.Trains.FindAsync(id);
    if (t == null) return Results.NotFound();

    db.Trains.Remove(t);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization("Admin");

// Price policies
app.MapGet("/pricepolicies", async (AppDbContext db) => await db.PricePolicies.ToListAsync()).RequireAuthorization("Admin");
app.MapGet("/pricepolicies/{id:long}", async (AppDbContext db, long id) =>
    await db.PricePolicies.FindAsync(id) is PricePolicy u ? Results.Ok(u) : Results.NotFound()).RequireAuthorization("Admin");
app.MapPost("/pricepolicies", async (AppDbContext db, PricePolicy p) =>
{
    db.PricePolicies.Add(p);
    await db.SaveChangesAsync();
    return Results.Created($"/pricepolicies/{p.Id}", p);
}).RequireAuthorization("Admin");
app.MapDelete("/pricepolicies/{id:long}", async (AppDbContext db, long id) =>
{
    var p = await db.PricePolicies.FindAsync(id);
    if (p == null) return Results.NotFound();

    db.PricePolicies.Remove(p);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization("Admin");

// Perk groups
app.MapGet("/perkgroups", async (AppDbContext db) => await db.PerkGroups.ToListAsync());
app.MapGet("/perkgroups/{id:long}", async (AppDbContext db, long id) =>
    await db.PerkGroups.FindAsync(id) is PerkGroup u ? Results.Ok(u) : Results.NotFound());
app.MapPost("/perkgroups", async (AppDbContext db, PerkGroup pg) =>
{
    db.PerkGroups.Add(pg);
    await db.SaveChangesAsync();
    return Results.Created($"/perkgroups/{pg.Id}", pg);
}).RequireAuthorization("Admin");
app.MapDelete("/perkgroups/{id:long}", async (AppDbContext db, long id) =>
{
    var pg = await db.PerkGroups.FindAsync(id);
    if (pg == null) return Results.NotFound();

    db.PerkGroups.Remove(pg);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization("Admin");

// Time table entries
app.MapGet("/timetable", async (DateTime? after, AppDbContext db) =>
{
    var query = db.TimeTableEntries
        .Include(e => e.Train)
        .AsQueryable();

    if (after.HasValue)
        query = query.Where(e => e.Arrival >= after.Value);

    return await query
        .OrderBy(e => e.Departure)
        .ToListAsync();
});

app.MapGet("/timetable/{id:long}", async (
    AppDbContext db,
    long id) =>
{
    var query =
        from e in db.TimeTableEntries
        join t in db.Trains on e.TrainId equals t.Id
        join s in db.Stations on t.SourceId equals s.Id
        join d in db.Stations on t.DestinationId equals d.Id
        join tt in db.TrainTypes on t.TypeId equals tt.Id
        select new { e, t, s, d, tt };

    query = query.Where(x => x.e.Id == id);

    var result = await query
        .Select(x => new TimetableSearchDto(
            x.e.Id,
            x.t.Id,
            x.tt.Name,
            x.s.Name,
            x.d.Name,
            x.e.Departure,
            x.e.Arrival
        ))
        .FirstAsync();

    return Results.Ok(result);
});
app.MapPost("/timetable", async (AppDbContext db, TimeTableEntry entry) =>
{
    db.TimeTableEntries.Add(entry);
    await db.SaveChangesAsync();
    return Results.Created($"/timetable/{entry.Id}", entry);
}).RequireAuthorization("Admin");
app.MapDelete("/timetable/{id:long}", async (AppDbContext db, long id) =>
{
    var e = await db.TimeTableEntries.FindAsync(id);
    if (e == null) return Results.NotFound();

    db.TimeTableEntries.Remove(e);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization("Admin");

// Tickets
app.MapGet("/tickets", async (AppDbContext db) => await db.Tickets.ToListAsync());
app.MapGet("/tickets/{id:long}", async (AppDbContext db, long id) =>
    await db.Tickets.FindAsync(id) is Ticket u ? Results.Ok(u) : Results.NotFound());
app.MapPost("/tickets", async (AppDbContext db, Ticket t) =>
{
    db.Tickets.Add(t);
    await db.SaveChangesAsync();
    return Results.Created($"/tickets/{t.Id}", t);
}).RequireAuthorization("Support");
app.MapDelete("/tickets/{id:long}", async (AppDbContext db, long id) =>
{
    var t = await db.Tickets.FindAsync(id);
    if (t == null) return Results.NotFound();

    db.Tickets.Remove(t);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization("Support");

// TicketLocks
app.MapGet("/ticketlocks", async (AppDbContext db) => await db.TicketLocks.ToListAsync());
app.MapGet("/ticketlocks/{id:long}", async (AppDbContext db, long id) =>
    await db.TicketLocks.FindAsync(id) is TicketLock u ? Results.Ok(u) : Results.NotFound());

app.MapDelete("/ticketlocks/{id:long}", async (AppDbContext db, long id) =>
{
    var l = await db.TicketLocks.FindAsync(id);
    if (l == null) return Results.NotFound();

    db.TicketLocks.Remove(l);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Payments
app.MapGet("/payments", async (AppDbContext db) => await db.Payments.ToListAsync());
app.MapGet("/payments/{id:long}", async (AppDbContext db, long id) =>
    await db.Payments.FindAsync(id) is Payment u ? Results.Ok(u) : Results.NotFound());

app.MapDelete("/payments/{id:long}", async (AppDbContext db, long id) =>
{
    var p = await db.Payments.FindAsync(id);
    if (p == null) return Results.NotFound();

    db.Payments.Remove(p);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization("Admin");


app.MapPost("/buy", async (
    AppDbContext db,
    ITicketPricingService pricingService,
    [FromBody] BuyTicketDto buyTicketDto
) =>
{
    var entry = await db.TimeTableEntries.FindAsync(buyTicketDto.EntryId);
    if (entry == null) return Results.BadRequest();

    var policy = entry.PricePolicy;
    if (policy == null)
    {
        policy = new PricePolicy();
    }

    if (buyTicketDto.PerkGroupId != null)
    {
        var selectedPerk = await db.PricePolicyPerkGroups.FindAsync(buyTicketDto.PerkGroupId);
        if (selectedPerk == null) return Results.InternalServerError();
        
        if (!policy.PerkGroups.Contains(selectedPerk)) return Results.BadRequest();
    }

    var price = pricingService.CalculateTicketPrice(policy, entry.Train.SourceId, entry.Train.DestinationId);

    var lockRow = new TicketLock
    {
        EntryId = buyTicketDto.EntryId,
        UserId = buyTicketDto.UserId,
        Paid = false,
        CreatedAt = DateTime.UtcNow,
        InvoiceId = Guid.NewGuid().ToString(), // simulating invoice id
        Sum = price
    };

    db.TicketLocks.Add(lockRow);
    await db.SaveChangesAsync();

    return Results.Ok(lockRow);
});


app.MapGet("/timetable/search", async (
    AppDbContext db,
    long? fromStationId,
    long? toStationId,
    DateTime? after,
    DateTime? before) =>
{
    var query =
        from e in db.TimeTableEntries
        join t in db.Trains on e.TrainId equals t.Id
        join s in db.Stations on t.SourceId equals s.Id
        join d in db.Stations on t.DestinationId equals d.Id
        join tt in db.TrainTypes on t.TypeId equals tt.Id
        select new { e, t, s, d, tt };

    if (after != null)
        after = after.Value.ToUniversalTime();

    if (before != null)
        before = before.Value.ToUniversalTime();


    if (fromStationId != null)
        query = query.Where(x => x.s.Id == fromStationId);

    if (toStationId != null)
        query = query.Where(x => x.d.Id == toStationId);

    if (after != null)
        query = query.Where(x => x.e.Departure >= after);

    if (before != null)
        query = query.Where(x => x.e.Arrival <= before);

    var result = await query
        .Select(x => new TimetableSearchDto(
            x.e.Id,
            x.t.Id,
            x.tt.Name,
            x.s.Name,
            x.d.Name,
            x.e.Departure,
            x.e.Arrival
        ))
        .ToListAsync();

    return Results.Ok(result);
});


app.MapPost("/pay/{lockId:long}", async (
    AppDbContext db,
    long lockId
) =>
{
    var lockEntry = await db.TicketLocks.FindAsync(lockId);
    if (lockEntry == null) return Results.NotFound();

    lockEntry.Paid = true;

    db.Tickets.Add(new Ticket
    {
        EntryId = lockEntry.EntryId,
        UserId = lockEntry.UserId,
        Used = false
    });

    db.Payments.Add(new Payment
    {
        LockId = lockEntry.Id,
        Successful = true, // simulate succesful payments
        Sum = lockEntry.Sum,
        InvoiceId = lockEntry.InvoiceId,
        DateTime = DateTime.UtcNow
    });

    await db.SaveChangesAsync();

    return Results.Ok();
});


app.Run();
