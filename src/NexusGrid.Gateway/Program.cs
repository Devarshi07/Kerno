using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NexusGrid.Gateway.Configuration;
using NexusGrid.Gateway.Middleware;
using Prometheus;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ServiceName", "Gateway")
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {ServiceName} | {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// Configuration
builder.Services.Configure<RateLimitSettings>(builder.Configuration.GetSection(RateLimitSettings.SectionName));
builder.Services.Configure<CacheSettings>(builder.Configuration.GetSection(CacheSettings.SectionName));

// Redis — singleton connection multiplexer (thread-safe, reuse across requests)
var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
    var options = ConfigurationOptions.Parse(redisConnectionString);
    options.AbortOnConnectFail = false; // Don't crash if Redis is down
    return ConnectionMultiplexer.Connect(options);
});

// JWT auth — gateway validates tokens before forwarding to services
var jwtSecret = builder.Configuration["Jwt:SecretKey"]
    ?? "NexusGrid-Super-Secret-Key-For-Development-Only-Min-32-Chars!";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "NexusGrid.UserService",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "NexusGrid",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// YARP reverse proxy — loads route/cluster config from appsettings.json
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddHealthChecks();

var app = builder.Build();

// Middleware order matters:
// 1. Correlation ID — tag every request first
// 2. Exception handling — catch gateway-level errors
// 3. Rate limiting — reject excessive traffic before auth
// 4. Response caching — serve cached responses before hitting backends
// 5. Authentication — validate JWT
// 6. Auth middleware — reject unauthenticated on protected routes
// 7. YARP proxy — forward to downstream services
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();
app.UseMiddleware<ResponseCachingMiddleware>();

app.UseAuthentication();
app.UseMiddleware<AuthMiddleware>();
app.UseAuthorization();

app.UseHttpMetrics();

app.MapReverseProxy();
app.MapHealthChecks("/health");
app.MapMetrics();

app.Run();

public partial class Program;
