using Microsoft.EntityFrameworkCore;
using CourseProject.Data;
using CourseProject.Models;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var conn = builder.Configuration.GetConnectionString("DefaultConnection") 
           ?? throw new InvalidOperationException("Missing DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(conn));

// for basic OpenAPI convenience in dev (optional)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

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
            .AllowCredentials();   // only if you use cookies or auth
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

// ---------- Simple CRUD endpoints for main entities ----------
// Users
app.MapGet("/users", async (AppDbContext db) => await db.Users.ToListAsync());
app.MapGet("/users/{id:long}", async (AppDbContext db, long id) =>
    await db.Users.FindAsync(id) is User u ? Results.Ok(u) : Results.NotFound());
app.MapPost("/users", async (AppDbContext db, User user) =>
{
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Created($"/users/{user.Id}", user);
});
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
});
app.MapDelete("/stations/{id:long}", async (AppDbContext db, long id) =>
{
    var s = await db.Stations.FindAsync(id);
    if (s == null) return Results.NotFound();

    db.Stations.Remove(s);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Train types
app.MapGet("/traintypes", async (AppDbContext db) => await db.TrainTypes.ToListAsync());
app.MapGet("/traintypes/{id:long}", async (AppDbContext db, long id) =>
    await db.TrainTypes.FindAsync(id) is TrainType u ? Results.Ok(u) : Results.NotFound());
app.MapPost("/traintypes", async (AppDbContext db, TrainType t) =>
{
    db.TrainTypes.Add(t);
    await db.SaveChangesAsync();
    return Results.Created($"/traintypes/{t.Id}", t);
});
app.MapDelete("/traintypes/{id:long}", async (AppDbContext db, long id) =>
{
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
    db.Trains.Add(t);
    await db.SaveChangesAsync();
    return Results.Created($"/trains/{t.Id}", t);
});
app.MapDelete("/trains/{id:long}", async (AppDbContext db, long id) =>
{
    var t = await db.Trains.FindAsync(id);
    if (t == null) return Results.NotFound();

    db.Trains.Remove(t);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Price policies
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
    db.Tickets.Add(t);
    await db.SaveChangesAsync();
    return Results.Created($"/tickets/{t.Id}", t);
});
app.MapDelete("/tickets/{id:long}", async (AppDbContext db, long id) =>
{
    var t = await db.Tickets.FindAsync(id);
    if (t == null) return Results.NotFound();

    db.Tickets.Remove(t);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// TicketLocks
app.MapGet("/ticketlocks", async (AppDbContext db) => await db.TicketLocks.ToListAsync());
app.MapGet("/ticketlocks/{id:long}", async (AppDbContext db, long id) =>
    await db.TicketLocks.FindAsync(id) is TicketLock u ? Results.Ok(u) : Results.NotFound());
app.MapPost("/ticketlocks", async (AppDbContext db, TicketLock l) =>
{
    db.TicketLocks.Add(l);
    await db.SaveChangesAsync();
    return Results.Created($"/ticketlocks/{l.Id}", l);
});
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
app.MapPost("/payments", async (AppDbContext db, Payment p) =>
{
    p.DateTime = DateTime.UtcNow;
    db.Payments.Add(p);
    await db.SaveChangesAsync();
    return Results.Created($"/payments/{p.Id}", p);
});
app.MapDelete("/payments/{id:long}", async (AppDbContext db, long id) =>
{
    var p = await db.Payments.FindAsync(id);
    if (p == null) return Results.NotFound();

    db.Payments.Remove(p);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();
