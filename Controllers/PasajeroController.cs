using CheckInApi.Modelos;
using Microsoft.AspNetCore.Mvc;

namespace CheckInApi.Controllers;

[ApiController]
[Route("api/[controller]")]
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
            return NotFound($"No se encontró un pasajero con ID {id}.");
        }
        return Ok(pasajero);
    }

    [HttpGet("documento/{documento}")]
    public ActionResult<Pasajero> ObtenerPorDocumento(string documento)
    {
        var pasajero = servicio.ObtenerPorDocumento(documento);
        if (pasajero == null)
        {
            return NotFound($"No se encontró un pasajero con el documento {documento}.");
        }
        return Ok(pasajero);
    }

    [HttpPost]
    public ActionResult Agregar([FromBody] PasajeroRequest pasajero)
    {
        var (exito, mensaje) = servicio.Agregar(pasajero);
        if (!exito)
        {
            return BadRequest(mensaje);
        }
        return Ok(mensaje);
    }

    [HttpPut("{id:int}")]
    public ActionResult Actualizar(int id, [FromBody] PasajeroRequest pasajero)
    {
        if (servicio.ObtenerPorId(id) == null)
        {
            return NotFound($"No se encontró un pasajero con ID {id}.");
        }

        var (exito, mensaje) = servicio.Actualizar(id, pasajero);
        if (!exito)
        {
            return BadRequest(mensaje);
        }
        return Ok(mensaje);
    }

    [HttpDelete("{id:int}")]
    public ActionResult Eliminar(int id)
    {
        if (servicio.ObtenerPorId(id) == null)
        {
            return NotFound($"No se encontró un pasajero con ID {id}.");
        }

        var (exito, mensaje) = servicio.Eliminar(id);
        if (!exito)
        {
            return BadRequest(mensaje);
        }
        return Ok(mensaje);
    }
}
