using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using Reservations.Api.ExceptionHandling;
using Reservations.Application;
using Reservations.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// --- Services ---

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialize enums as text ("Created", "Standard") instead of numbers.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// System clock as an injectable abstraction (enables deterministic tests).
builder.Services.AddSingleton(TimeProvider.System);

// Application layers.
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

// Centralized error handling with ProblemDetails (RFC 7807).
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// OpenAPI / Swagger documentation.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Transfer Reservations API",
        Version = "v1",
        Description = "REST API for managing transfer reservations (Backend .NET technical test)."
    });

    // Include XML comments in the Swagger documentation.
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// --- HTTP pipeline ---

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Transfer Reservations API v1");
        options.RoutePrefix = string.Empty; // Swagger UI available at the root "/".
    });
}
else
{
    // HTTPS redirection only applies outside development. Locally it avoids the Swagger
    // "Failed to fetch" error when the development certificate is not trusted.
    app.UseHttpsRedirection();
}

app.MapControllers();

app.Run();

// Required to expose the Program class to integration tests (WebApplicationFactory).
public partial class Program { }
