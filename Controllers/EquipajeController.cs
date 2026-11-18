using CheckInApi.Modelos;
using CheckInApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CheckInApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EquipajeController : ControllerBase
{
    private readonly EquipajeService servicio;

    public EquipajeController(EquipajeService servicio)
    {
        this.servicio = servicio;
    }

    [HttpGet]
    public ActionResult<List<Equipaje>> ObtenerTodos()
    {
        return Ok(servicio.ListarEquipajes());
    }

    [HttpGet("{idPasajero:int}")]
    public ActionResult<List<Equipaje>> ObtenerPorPasajero(int idPasajero)
    {
        List<Equipaje>? equipajes = servicio.ListarPorPasajero(idPasajero);
        if (equipajes == null)
        {
            return NotFound(new { mensaje = $"No se encontró un pasajero con ID {idPasajero}." });
        }

        return Ok(equipajes);
    }

    [HttpGet("{idPasajero:int}/{codigoEtiqueta}")]
    public ActionResult<Equipaje> ObtenerPorCodigo(int idPasajero, string codigoEtiqueta)
    {
        Equipaje? equipaje = servicio.ObtenerPorCodigo(idPasajero, codigoEtiqueta);
        if (equipaje == null)
        {
            return NotFound(new
            {
                mensaje = $"No se encontró un equipaje con el código '{codigoEtiqueta}' para el pasajero con ID {idPasajero}."
            });
        }

        return Ok(equipaje);
    }

    [HttpPost("{idPasajero:int}")]
    public ActionResult<Equipaje> Registrar(int idPasajero, [FromBody] EquipajeRequest datos)
    {
        var equipaje = new Equipaje
        {
            PasajeroId = idPasajero.ToString(),
            CodigoEtiqueta = datos.CodigoEtiqueta ?? string.Empty,
            TipoEquipaje = datos.TipoEquipaje ?? string.Empty,
            PesoKg = datos.PesoKg
        };

        servicio.RegistrarEquipaje(equipaje);

        return StatusCode(StatusCodes.Status201Created, equipaje);
    }

    [HttpPut("{idPasajero:int}/{codigoEtiqueta}")]
    public ActionResult Actualizar(int idPasajero, string codigoEtiqueta, [FromBody] EquipajeRequest datos)
    {
        var (exito, mensaje, noEncontrado) = servicio.ActualizarEquipaje(idPasajero, codigoEtiqueta, datos);

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

    [HttpDelete("{idPasajero:int}/{codigoEtiqueta}")]
    public ActionResult Eliminar(int idPasajero, string codigoEtiqueta)
    {
        var (exito, mensaje, noEncontrado) = servicio.EliminarEquipaje(idPasajero, codigoEtiqueta);

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
