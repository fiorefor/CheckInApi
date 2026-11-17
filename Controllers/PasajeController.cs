using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using CheckInApi.Modelos;
using CheckInApi.Services;

namespace CheckInApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PasajesController : ControllerBase
    {
        private readonly PasajeService _pasajeService;

        public PasajesController(PasajeService pasajeService)
        {
            _pasajeService = pasajeService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_pasajeService.ListarPasajes());
        }

        [HttpGet("{idPasajero}")]
        public IActionResult GetById(int idPasajero)
        {
            var pasaje = _pasajeService.ObtenerPasaje(idPasajero);
            if (pasaje == null)
                return NotFound(new { mensaje = $"No se encontró un pasaje para el pasajero con ID {idPasajero}." });

            return Ok(pasaje);
        }

        [HttpPost("{idPasajero:int}")]
        public IActionResult Create(int idPasajero, [FromBody] PasajeRequest request)
        {
            var resultado = _pasajeService.AgregarPasaje(idPasajero, request);
            if (!resultado.exito)
            {
                if (resultado.noEncontrado)
                    return NotFound(new { mensaje = resultado.mensaje });

                return BadRequest(new { mensaje = resultado.mensaje });
            }

            var pasajeCreado = _pasajeService.ObtenerPasaje(idPasajero);
            return CreatedAtAction(nameof(GetById), new { idPasajero }, pasajeCreado);
        }

        [HttpPut("{idPasajero:int}")]
        public IActionResult Update(int idPasajero, [FromBody] PasajeRequest request)
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
        public IActionResult Delete(int idPasajero)
        {
            var resultado = _pasajeService.EliminarPasaje(idPasajero);
            if (!resultado.exito)
                return NotFound(new { mensaje = resultado.mensaje });

            return NoContent();
        }
    }
}