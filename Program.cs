using CheckInApi.Repositorios;

var builder = WebApplication.CreateBuilder(args);

// Registro de servicios en el contenedor de dependencias
//Habilita Controllers (pasajerocontroller, pasajecontroller,etc
builder.Services.AddControllers();

// PERSISTENCIA Y LOGICA DE NEGOCIO

string path = Path.Combine(builder.Environment.ContentRootPath, "Data", "pasajeros.txt");

//Cuando en interfaz se pida un IpasajeroRepository se le entrega un PasajeroRepositoryArchivo (clase que guarda el .txt)
//Si deseamos cambiar a bd solo cambiamos esta linea
builder.Services.AddScoped<IPasajeroRepository>(provider => new PasajeroRepositoryArchivo(path));

//Capa de nogocios: recibe el repositorio por su construcción (inyecciónde dependencia)
builder.Services.AddScoped<PasajeroService>();
builder.Services.AddScoped<CheckInService>();
builder.Services.AddScoped<CheckInApi.Services.PasajeService>();


//OpenAI: genera descripción de la Api. La plantilla de Visual, la trae por defecto https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP peticiones pipeline.s
//Solo en modo de desarrollo se publica la descripcion
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
//Redirige peticiones Http a Https
app.UseHttpsRedirection();
//Habilita Autorizacion (trae plantilla aunque el proyecto no lo use)
app.UseAuthorization();
//Conecta rutas de controllers
app.MapControllers();
//Arranca api
app.Run();
