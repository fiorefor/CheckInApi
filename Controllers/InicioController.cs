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
                    ruta = "/api/pasajeros",
                    operaciones = new[]
                    {
                        "GET /api/pasajeros",
                        "GET /api/pasajeros/{id}",
                        "GET /api/pasajeros/documento/{documento}",
                        "POST /api/pasajeros",
                        "PUT /api/pasajeros/{id}",
                        "DELETE /api/pasajeros/{id}"
                    }
                },
                new
                {
                    entidad = "Pasajes",
                    ruta = "/api/pasajes",
                    operaciones = new[]
                    {
                        "GET /api/pasajes",
                        "GET /api/pasajes/{idPasajero}",
                        "POST /api/pasajes/{idPasajero}",
                        "PUT /api/pasajes/{idPasajero}",
                        "DELETE /api/pasajes/{idPasajero}"
                    }
                },
                new
                {
                    entidad = "Equipajes",
                    ruta = "/api/equipajes",
                    operaciones = new[]
                    {
                        "GET /api/equipajes",
                        "GET /api/equipajes/{idPasajero}",
                        "GET /api/equipajes/{idPasajero}/{codigoEtiqueta}",
                        "POST /api/equipajes/{idPasajero}",
                        "PUT /api/equipajes/{idPasajero}/{codigoEtiqueta}",
                        "DELETE /api/equipajes/{idPasajero}/{codigoEtiqueta}"
                    }
                }
            }
        });
    }
}
