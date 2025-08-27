using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OpenApi;
using OpenTelemetry.Trace;
using OpenTelemetry.Instrumentation.EntityFrameworkCore; // <-- trae AddEntityFrameworkCoreInstrumentation
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using TemplateService.Infrastructure.Data;
using TemplateService.Infrastructure.Models;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

// Config
var conn = builder.Configuration.GetConnectionString("Default")
           ?? builder.Configuration["ConnectionStrings:Default"];
var rabbitHost = builder.Configuration["RabbitMq:Host"] ?? "localhost";
var otlpEndpoint = builder.Configuration["Otlp:Endpoint"] ?? "http://localhost:4317";

// DbContext
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(conn));

// MassTransit + RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    // x.AddConsumer<YourConsumer>(); // agrega tus consumers si los tienes
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitHost, "/", h => { });
        cfg.ConfigureEndpoints(context);
    });
});

// OpenTelemetry
var serviceName = "TemplateService";
builder.Services.AddOpenTelemetry()
   .ConfigureResource(r => r.AddService(serviceName))
   .WithTracing(tracing => tracing
       .AddAspNetCoreInstrumentation()
       .AddHttpClientInstrumentation()
       .AddEntityFrameworkCoreInstrumentation()
       .AddSource("MassTransit")
       .AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint)))
   .WithMetrics(metrics => metrics
       .AddAspNetCoreInstrumentation()
       .AddHttpClientInstrumentation());

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// HealthChecks
builder.Services.AddHealthChecks()
    .AddSqlServer(conn!, name: "sqlserver");

// CORS dev
builder.Services.AddCors(options =>
{
    options.AddPolicy("dev", p => p
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
        .SetIsOriginAllowed(_ => true));
});

var app = builder.Build();

// Migraciones automáticas (solo dev)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("dev");
app.UseSerilogRequestLogging();

// Endpoints de ejemplo
app.MapGet("/api/todos", async (AppDbContext db) =>
{
    return await db.Todos.AsNoTracking().ToListAsync();
})
.WithName("GetTodos").WithOpenApi();

app.MapPost("/api/todos", async (AppDbContext db, TodoItem todo) =>
{
    db.Todos.Add(todo);
    await db.SaveChangesAsync();
    return Results.Created($"/api/todos/{todo.Id}", todo);
})
.WithName("CreateTodo").WithOpenApi();

app.MapGet("/", () => "TemplateService up")
   .WithName("Root").WithOpenApi();

// Health
app.MapHealthChecks("/health");

app.Run();
