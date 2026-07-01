using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using Reservations.Api.ExceptionHandling;
using Reservations.Application;
using Reservations.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// --- Servicios ---

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serializa los enums como texto ("Created", "Standard") en lugar de números.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Reloj del sistema como abstracción inyectable (facilita pruebas deterministas).
builder.Services.AddSingleton(TimeProvider.System);

// Capas de la aplicación.
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

// Manejo de errores centralizado con ProblemDetails (RFC 7807).
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Documentación OpenAPI / Swagger.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Transfer Reservations API",
        Version = "v1",
        Description = "API REST para la gestión de reservas de traslados (prueba técnica Backend .NET)."
    });

    // Incluye los comentarios XML en la documentación de Swagger.
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// --- Pipeline HTTP ---

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Transfer Reservations API v1");
        options.RoutePrefix = string.Empty; // Swagger UI disponible en la raíz "/".
    });
}
else
{
    // La redirección a HTTPS solo aplica fuera de desarrollo. En local evita el error
    // "Failed to fetch" de Swagger cuando el certificado de desarrollo no es de confianza.
    app.UseHttpsRedirection();
}

app.MapControllers();

app.Run();

// Necesario para exponer la clase Program a las pruebas de integración (WebApplicationFactory).
public partial class Program { }
