using Cassandra;
using NexusGrid.NotificationService.Middleware;
using NexusGrid.NotificationService.Repositories;
using NexusGrid.NotificationService.Services;
using Prometheus;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ServiceName", "NotificationService")
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {ServiceName} | {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// Cassandra
builder.Services.Configure<CassandraSettings>(builder.Configuration.GetSection(CassandraSettings.SectionName));

var cassandraSettings = builder.Configuration.GetSection(CassandraSettings.SectionName).Get<CassandraSettings>()!;

builder.Services.AddSingleton<ICluster>(_ =>
{
    var clusterBuilder = Cluster.Builder()
        .AddContactPoint(cassandraSettings.ContactPoint)
        .WithPort(cassandraSettings.Port)
        .WithDefaultKeyspace(cassandraSettings.Keyspace);

    if (!string.IsNullOrEmpty(cassandraSettings.Username))
    {
        clusterBuilder.WithCredentials(cassandraSettings.Username, cassandraSettings.Password ?? "");
    }

    return clusterBuilder.Build();
});

builder.Services.AddSingleton<Cassandra.ISession>(sp =>
{
    var cluster = sp.GetRequiredService<ICluster>();
    return cluster.Connect();
});

builder.Services.AddScoped<ICassandraRepository, CassandraRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks();

var app = builder.Build();

// Middleware pipeline
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpMetrics();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapMetrics();

// Initialize Cassandra schema
using (var scope = app.Services.CreateScope())
{
    var repo = scope.ServiceProvider.GetRequiredService<ICassandraRepository>();
    await repo.InitializeSchemaAsync();
}

app.Run();

public partial class Program;
