using CheckInApi.Modelos;
using Microsoft.AspNetCore.Mvc;

namespace CheckInApi.Controllers;

[ApiController]
[Route("api/pasajeros")]
public class PasajeroController : ControllerBase
{
    private readonly PasajeroService servicio;

    public PasajeroController(PasajeroService servicio)
    {
        this.servicio = servicio;
    }

    [HttpGet]
    public ActionResult<List<Pasajero>> ObtenerTodos()
    {
        return Ok(servicio.ObtenerTodos());
    }

    [HttpGet("{id:int}")]
    public ActionResult<Pasajero> ObtenerPorId(int id)
    {
        var pasajero = servicio.ObtenerPorId(id);
        if (pasajero == null)
        {
            return NotFound(new { mensaje = $"No se encontró un pasajero con ID {id}." });
        }
        return Ok(pasajero);
    }

    [HttpGet("documento/{documento}")]
    public ActionResult<Pasajero> ObtenerPorDocumento(string documento)
    {
        var pasajero = servicio.ObtenerPorDocumento(documento);
        if (pasajero == null)
        {
            return NotFound(new { mensaje = $"No se encontró un pasajero con el documento {documento}." });
        }
        return Ok(pasajero);
    }

    [HttpPost]
    public ActionResult Agregar([FromBody] PasajeroRequest pasajero)
    {
        var (exito, mensaje) = servicio.Agregar(pasajero);
        if (!exito)
        {
            return BadRequest(new { mensaje });
        }
        return StatusCode(StatusCodes.Status201Created, new { mensaje });
    }

    [HttpPut("{id:int}")]
    public ActionResult Actualizar(int id, [FromBody] PasajeroRequest pasajero)
    {
        var (exito, mensaje, noEncontrado) = servicio.Actualizar(id, pasajero);
        if (noEncontrado)
        {
            return NotFound(new { mensaje });
        }
        if (!exito)
        {
            return BadRequest(new { mensaje });
        }
        return Ok(new { mensaje });
    }

    [HttpDelete("{id:int}")]
    public ActionResult Eliminar(int id)
    {
        var (exito, mensaje, noEncontrado) = servicio.Eliminar(id);
        if (noEncontrado)
        {
            return NotFound(new { mensaje });
        }
        if (!exito)
        {
            return BadRequest(new { mensaje });
        }
        return Ok(new { mensaje });
    }
}
