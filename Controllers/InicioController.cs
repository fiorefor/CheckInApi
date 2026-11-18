using Microsoft.AspNetCore.Mvc;

namespace CheckInApi.Controllers;

[ApiController]
[Route("api")]
public class InicioController : ControllerBase
{
    [HttpGet]
    public ActionResult Menu()
    {
        return Ok(new
        {
            api = "CheckInApi",
            descripcion = "API de check-in de una aerolínea",
            modulos = new[]
            {
                new
                {
                    entidad = "Pasajeros",
                    ruta = "/api/Pasajero",
                    operaciones = new[]
                    {
                        "GET /api/Pasajero",
                        "GET /api/Pasajero/{id}",
                        "GET /api/Pasajero/documento/{documento}",
                        "POST /api/Pasajero",
                        "PUT /api/Pasajero/{id}",
                        "DELETE /api/Pasajero/{id}"
                    }
                },
                new
                {
                    entidad = "Pasajes",
                    ruta = "/api/Pasajes",
                    operaciones = new[]
                    {
                        "GET /api/Pasajes",
                        "GET /api/Pasajes/{idPasajero}",
                        "POST /api/Pasajes/{idPasajero}",
                        "PUT /api/Pasajes/{idPasajero}",
                        "DELETE /api/Pasajes/{idPasajero}"
                    }
                },
                new
                {
                    entidad = "Equipajes",
                    ruta = "/api/Equipaje",
                    operaciones = new[]
                    {
                        "GET /api/Equipaje",
                        "GET /api/Equipaje/{idPasajero}",
                        "GET /api/Equipaje/{idPasajero}/{codigoEtiqueta}",
                        "POST /api/Equipaje/{idPasajero}",
                        "PUT /api/Equipaje/{idPasajero}/{codigoEtiqueta}",
                        "DELETE /api/Equipaje/{idPasajero}/{codigoEtiqueta}"
                    }
                }
            }
        });
    }
}
