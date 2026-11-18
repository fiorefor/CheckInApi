using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using CheckInApi.Modelos;
using CheckInApi.Services;

namespace CheckInApi.Controllers
{
    [ApiController]
    [Route("api/pasajes")]
    public class PasajesController : ControllerBase
    {
        private readonly PasajeService _pasajeService;

        public PasajesController(PasajeService pasajeService)
        {
            _pasajeService = pasajeService;
        }

        [HttpGet]
        public IActionResult ObtenerTodos()
        {
            return Ok(_pasajeService.ListarPasajes());
        }

        [HttpGet("{idPasajero}")]
        public IActionResult ObtenerPorId(int idPasajero)
        {
            var pasaje = _pasajeService.ObtenerPasaje(idPasajero);
            if (pasaje == null)
                return NotFound(new { mensaje = $"No se encontró un pasaje para el pasajero con ID {idPasajero}." });

            return Ok(pasaje);
        }

        [HttpPost("{idPasajero:int}")]
        public IActionResult Agregar(int idPasajero, [FromBody] PasajeRequest request)
        {
            var resultado = _pasajeService.AgregarPasaje(idPasajero, request);
            if (!resultado.exito)
            {
                if (resultado.noEncontrado)
                    return NotFound(new { mensaje = resultado.mensaje });

                return BadRequest(new { mensaje = resultado.mensaje });
            }

            var pasajeCreado = _pasajeService.ObtenerPasaje(idPasajero);
            return CreatedAtAction(nameof(ObtenerPorId), new { idPasajero }, pasajeCreado);
        }

        [HttpPut("{idPasajero:int}")]
        public IActionResult Actualizar(int idPasajero, [FromBody] PasajeRequest request)
        {
            var resultado = _pasajeService.ActualizarPasaje(idPasajero, request);
            if (!resultado.exito)
            {
                if (resultado.noEncontrado)
                    return NotFound(new { mensaje = resultado.mensaje });

                return BadRequest(new { mensaje = resultado.mensaje });
            }

            return Ok(new { mensaje = resultado.mensaje });
        }

        [HttpDelete("{idPasajero:int}")]
        public IActionResult Eliminar(int idPasajero)
        {
            var resultado = _pasajeService.EliminarPasaje(idPasajero);
            if (resultado.noEncontrado)
                return NotFound(new { mensaje = resultado.mensaje });

            if (!resultado.exito)
                return BadRequest(new { mensaje = resultado.mensaje });

            return Ok(new { mensaje = resultado.mensaje });
        }
    }
}
