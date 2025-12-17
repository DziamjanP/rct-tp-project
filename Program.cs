using System.Text;
using CourseProject.Application.Interfaces.Services;
using CourseProject.Application.Services;
using CourseProject.Application.Interfaces.Repositories;
using CourseProject.Infrastructure.Data.Repositories;
using CourseProject.Data;
using CourseProject.Services;
using CourseProject.Api.Endpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var conn = builder.Configuration.GetConnectionString("DefaultConnection") 
           ?? throw new InvalidOperationException("Missing DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(conn));

builder.Services.AddSingleton<JwtService>();

builder.Services.AddScoped<IStationDistanceCalculator, StationDistanceCalculator>();
builder.Services.AddScoped<ITicketPricingService, TicketPricingService>();

builder.Services.AddHostedService<ExpiredLockCleanup>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITrainRepository, TrainRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<IStationRepository, StationRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<ITrainTypeRepository, TrainTypeRepository>();
builder.Services.AddScoped<IPerkGroupRepository, PerkGroupRepository>();
builder.Services.AddScoped<IPricePolicyRepository, PricePolicyRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<ITicketBookingRepository, TicketBookingRepository>();
builder.Services.AddScoped<ITimeTableEntryRepository, TimeTableEntryRepository>();
builder.Services.AddScoped<IPricePolicyPerkGroupRepository, PricePolicyPerkGroupRepository>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IReportService, ReportService>();

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
    options.RequireHttpsMetadata = false; // in dev, should be true for prod
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
    ).AddPolicy("Inspector", policy =>
        policy.RequireAssertion(context =>
        {
            var claim = context.User.FindFirst("access_level")?.Value;
            if (string.IsNullOrEmpty(claim)) return false;
            return int.TryParse(claim, out var level) && level == -1;
        })
    );

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddHostedService<ExpiredLockCleanup>();
builder.Services.AddScoped<IStationDistanceCalculator, StationDistanceCalculator>();
builder.Services.AddScoped<ITicketPricingService, TicketPricingService>();

string[] devOrigins =
{
    "http://localhost:5041",
    "http://localhost:5042",
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

//app.MapControllers();

app.MapAuth();
app.MapUsers();
app.MapPayments();
app.MapPricePolicies();
app.MapPerkGroups();
app.MapTrainTypes();
app.MapTrains();
app.MapTimeTableEntries();
app.MapTicketBookings();
app.MapStations();
app.MapTickets();
app.MapReports();

app.MapGet("/health", () => Results.Ok("OK"));

using (var scope = app.Services.CreateScope())
{
    var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (env.IsDevelopment())
    {
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

app.Run();
