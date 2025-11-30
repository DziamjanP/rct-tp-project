using Microsoft.EntityFrameworkCore;
using CourseProject.Data;
using CourseProject.Models;
using CourseProject.Dtos;
using CourseProject.Services;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var conn = builder.Configuration.GetConnectionString("DefaultConnection") 
           ?? throw new InvalidOperationException("Missing DefaultConnection");

// Add JwtService
builder.Services.AddSingleton<JwtService>();

// Add DbContext (already present in your app). Example:
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(conn));

// Configure authentication: JWT Bearer
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
);

// for basic OpenAPI convenience in dev (optional)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddHostedService<ExpiredLockCleanup>();

string[] devOrigins =
{
    "http://localhost:5173",  // Vite
    "http://localhost:3000"   // optional other tools
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


// auto-migrate in Development for prototype convenience ONLY
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


// ---------- AUTH endpoints ----------
app.MapPost("/auth/register", async (AppDbContext db, JwtService jwt, RegisterDto dto) =>
{
    // minimal validation
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
    if (string.IsNullOrWhiteSpace(dto.Phone) || string.IsNullOrWhiteSpace(dto.Password))
        return Results.BadRequest("Phone and Password are required.");

    var user = await db.Users.FirstOrDefaultAsync(u => u.Phone == dto.Phone);
    if (user == null) return Results.Unauthorized();

    var salt = user.Salt;
    var hash = BCrypt.Net.BCrypt.HashPassword(dto.Password, salt);

    if (hash != user.Password) return Results.Unauthorized();

    var token = jwt.GenerateToken(user.Id, user.AccessLevel, user.Name, user.Phone);
    var response = new AuthResponseDto(token, user.Id, user.AccessLevel, user.Name ?? "", user.Surname ?? "", user.Passport ?? "", user.Phone ?? "");
    return Results.Ok(response);
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
    // TODO: check priveleges >= 2
    db.Stations.Add(s);
    await db.SaveChangesAsync();
    return Results.Created($"/stations/{s.Id}", s);
}).RequireAuthorization("Admin");
app.MapDelete("/stations/{id:long}", async (AppDbContext db, long id) =>
{
    // TODO: check priveleges >= 2
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
    // TODO: check priveleges >= 2
    db.TrainTypes.Add(t);
    await db.SaveChangesAsync();
    return Results.Created($"/traintypes/{t.Id}", t);
});
app.MapDelete("/traintypes/{id:long}", async (AppDbContext db, long id) =>
{
    // TODO: check priveleges >= 2
    var t = await db.TrainTypes.FindAsync(id);
    if (t == null) return Results.NotFound();

    db.TrainTypes.Remove(t);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Trains
app.MapGet("/trains", async (AppDbContext db) => await db.Trains.Include(t => t.Source).Include(t => t.Destination).Include(t => t.Type).ToListAsync());
app.MapGet("/trains/{id:long}", async (AppDbContext db, long id) =>
    await db.Trains.FindAsync(id) is Train u ? Results.Ok(u) : Results.NotFound());
app.MapPost("/trains", async (AppDbContext db, Train t) =>
{
    // TODO: check priveleges >= 2
    db.Trains.Add(t);
    await db.SaveChangesAsync();
    return Results.Created($"/trains/{t.Id}", t);
});
app.MapDelete("/trains/{id:long}", async (AppDbContext db, long id) =>
{
    // TODO: check priveleges >= 2
    var t = await db.Trains.FindAsync(id);
    if (t == null) return Results.NotFound();

    db.Trains.Remove(t);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Price policies
// TODO: check priveleges >= 2
app.MapGet("/pricepolicies", async (AppDbContext db) => await db.PricePolicies.ToListAsync());
app.MapGet("/pricepolicies/{id:long}", async (AppDbContext db, long id) =>
    await db.PricePolicies.FindAsync(id) is PricePolicy u ? Results.Ok(u) : Results.NotFound());
app.MapPost("/pricepolicies", async (AppDbContext db, PricePolicy p) =>
{
    db.PricePolicies.Add(p);
    await db.SaveChangesAsync();
    return Results.Created($"/pricepolicies/{p.Id}", p);
});
app.MapDelete("/pricepolicies/{id:long}", async (AppDbContext db, long id) =>
{
    var p = await db.PricePolicies.FindAsync(id);
    if (p == null) return Results.NotFound();

    db.PricePolicies.Remove(p);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Perk groups
// TODO: check priveleges >= 2
app.MapGet("/perkgroups", async (AppDbContext db) => await db.PerkGroups.ToListAsync());
app.MapGet("/perkgroups/{id:long}", async (AppDbContext db, long id) =>
    await db.PerkGroups.FindAsync(id) is PerkGroup u ? Results.Ok(u) : Results.NotFound());
app.MapPost("/perkgroups", async (AppDbContext db, PerkGroup pg) =>
{
    db.PerkGroups.Add(pg);
    await db.SaveChangesAsync();
    return Results.Created($"/perkgroups/{pg.Id}", pg);
});
app.MapDelete("/perkgroups/{id:long}", async (AppDbContext db, long id) =>
{
    var pg = await db.PerkGroups.FindAsync(id);
    if (pg == null) return Results.NotFound();

    db.PerkGroups.Remove(pg);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Time table entries
// TODO: check priveleges >= 2
app.MapGet("/timetable", async (AppDbContext db) => await db.TimeTableEntries.Include(e => e.Train).ToListAsync());
app.MapGet("/timetable/{id:long}", async (AppDbContext db, long id) =>
    await db.TimeTableEntries.FindAsync(id) is TimeTableEntry u ? Results.Ok(u) : Results.NotFound());
app.MapPost("/timetable", async (AppDbContext db, TimeTableEntry entry) =>
{
    db.TimeTableEntries.Add(entry);
    await db.SaveChangesAsync();
    return Results.Created($"/timetable/{entry.Id}", entry);
});
app.MapDelete("/timetable/{id:long}", async (AppDbContext db, long id) =>
{
    var e = await db.TimeTableEntries.FindAsync(id);
    if (e == null) return Results.NotFound();

    db.TimeTableEntries.Remove(e);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Tickets
app.MapGet("/tickets", async (AppDbContext db) => await db.Tickets.ToListAsync());
app.MapGet("/tickets/{id:long}", async (AppDbContext db, long id) =>
    await db.Tickets.FindAsync(id) is Ticket u ? Results.Ok(u) : Results.NotFound());
app.MapPost("/tickets", async (AppDbContext db, Ticket t) =>
{
    // TODO: check priveleges >= 1
    db.Tickets.Add(t);
    await db.SaveChangesAsync();
    return Results.Created($"/tickets/{t.Id}", t);
}).RequireAuthorization("Support");
app.MapDelete("/tickets/{id:long}", async (AppDbContext db, long id) =>
{
    // TODO: check priveleges >= 1
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
    // TODO: check priveleges >= 2
    var p = await db.Payments.FindAsync(id);
    if (p == null) return Results.NotFound();

    db.Payments.Remove(p);
    await db.SaveChangesAsync();
    return Results.NoContent();
});


app.MapPost("/buy/{entryId:long}", async (
    AppDbContext db,
    long entryId,
    long userId
) =>
{
    var entry = await db.TimeTableEntries.FindAsync(entryId);
    if (entry == null) return Results.NotFound();

    var lockRow = new TicketLock
    {
        EntryId = entryId,
        UserId = userId,
        Paid = false,
        CreatedAt = DateTime.UtcNow,
        InvoiceId = Guid.NewGuid().ToString(), // simulating invoice id
        Sum = 10   // TODO: compute via policies
    };

    db.TicketLocks.Add(lockRow);
    await db.SaveChangesAsync();

    return Results.Ok(lockRow);
});



app.MapGet("/search", async (
    AppDbContext db,
    long? fromId,
    long? toId,
    DateTime? after,
    DateTime? before) =>
{
    var query =
        from e in db.TimeTableEntries
        join t in db.Trains on e.TrainId equals t.Id
        join s in db.Stations on t.SourceId equals s.Id
        join d in db.Stations on t.DestinationId equals d.Id
        join tt in db.TrainTypes on t.TypeId equals tt.Id

        select new TimetableSearchDto(
            e.Id,        // entry id
            tt.Name,     // train type name
            s.Name,      // dep name
            d.Name,      // arr name
            e.Departure, // dep time
            e.Arrival    // arr time
        );

    if (fromId != null)
        query = query.Where(x => x.Source == fromId.ToString());

    if (toId != null)
        query = query.Where(x => x.Destination == toId.ToString());

    if (after != null)
        query = query.Where(x => x.Departure >= after);

    if (before != null)
        query = query.Where(x => x.Departure <= before);

    return Results.Ok(await query.ToListAsync());
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
