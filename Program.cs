using CheckInApi.Modelos.Exceptions;
using CheckInApi.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CheckInApi.Repositorios;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(opciones =>
    {
        // Usa el mismo formato de error que el manejador central.
        opciones.InvalidModelStateResponseFactory = _ =>
            new BadRequestObjectResult(new
            {
                mensaje = "Los datos enviados no son válidos. Verifique el formato del JSON y los campos obligatorios."
            });
    });

string path = Path.Combine(builder.Environment.ContentRootPath, "Data", "pasajeros.txt");

builder.Services.AddScoped<IPasajeroRepository>(provider => new PasajeroRepositoryArchivo(path));
builder.Services.AddScoped<PasajeroService>();
builder.Services.AddScoped<CheckInService>();
builder.Services.AddScoped<CheckInApi.Services.PasajeService>();
builder.Services.AddScoped<EquipajeService>();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Traduce las excepciones de las capas inferiores a respuestas HTTP uniformes.
app.UseExceptionHandler(error => error.Run(async context =>
{
    var excepcion = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    var (estado, mensaje) = excepcion switch
    {
        ArchivoException archivo => (StatusCodes.Status500InternalServerError, archivo.Message),
        DatosException datos => (StatusCodes.Status400BadRequest, datos.Message),
        EquipajeException equipaje => (StatusCodes.Status400BadRequest, equipaje.Message),
        PasajeroException pasajero => (StatusCodes.Status400BadRequest, pasajero.Message),
        UnauthorizedAccessException => (StatusCodes.Status500InternalServerError, "No hay permisos para acceder al archivo de datos."),
        IOException => (StatusCodes.Status500InternalServerError, "No se pudo leer o escribir el archivo de datos."),
        _ => (StatusCodes.Status500InternalServerError, "Ocurrió un error inesperado. Intente nuevamente.")
    };
    context.Response.StatusCode = estado;
    await context.Response.WriteAsJsonAsync(new { mensaje });
}));

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/", () => Results.Redirect("/api"));
app.MapFallback(() => Results.NotFound(new { mensaje = "La ruta solicitada no existe." }));

app.Run();
