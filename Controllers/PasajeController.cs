using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using CheckInApi.Modelos;
using CheckInApi.Modelos.Exceptions;
using CheckInApi.Servicios;

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

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                var pasaje = _pasajeService.ObtenerPasajePorId(id);
                return Ok(pasaje);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Create([FromBody] PasajeRequest request)
        {
            try
            {
                var creado = _pasajeService.EmitirPasaje(request);
                return CreatedAtAction(nameof(GetById), new { id = creado.IdPasaje }, creado);
            }
            catch (DatosInvalidosException ex)
            {
                return BadRequest(new { campo = ex.Campo, mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            bool eliminado = _pasajeService.EliminarPasaje(id);
            if (!eliminado)
                return NotFound(new { mensaje = $"No existe el pasaje con ID {id}." });

            return NoContent();
        }
    }
}