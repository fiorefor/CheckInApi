using System;
using CheckInApi.Modelos;
using CheckInApi.Modelos.Exceptions;
using CheckInApi.Repositorios; 

namespace CheckInApi.Services
{
    public class EquipajeService
    {
        private readonly IPasajeroRepository _pasajeRepository; // O el servicio/repositorio que maneje pasajes

        // Tarifas y límites de peso base según el tipo de vuelo
        private const decimal TarifaPorKiloExcedido = 2500.0m;

        public EquipajeService(IPasajeroRepository pasajeroRepository)
        {
            _pasajeRepository = pasajeroRepository;
        }

        public void RegistrarEquipaje(Equipaje equipaje)
        {
            // 1. Control de errores e integridad: Verificar si el pasaje asociado existe
            var pasajeroAsociado = _pasajeRepository.ObtenerPorId(int.Parse(equipaje.PasajeroId));
            if (pasajeroAsociado == null || pasajeroAsociado.Pasaje == null)
            {
                throw new EquipajeException($"Error de integridad: El pasaje con ID '{equipaje.PasajeroId}' no existe en el sistema.");
            }

            Pasaje pasajeAsociado = pasajeroAsociado.Pasaje;

            // 2. Validar peso básico
            if (equipaje.PesoKg <= 0)
            {
                throw new EquipajeException("El peso del equipaje debe ser mayor a 0 kg.");
            }

            // 3. Determinar el límite de peso según el tipo de vuelo del pasaje asociado
            double pesoMaximoPermitido = ObtenerPesoMaximoPorTipoVuelo(pasajeAsociado.TipoVuelo);

            // 4. Validación de peso máximo y cálculo de tarifa por exceso
            if (equipaje.PesoKg > pesoMaximoPermitido)
            {
                double excesoKg = equipaje.PesoKg - pesoMaximoPermitido;
                equipaje.TarifaExceso = (decimal)excesoKg * TarifaPorKiloExcedido;
            }
            else
            {
                equipaje.TarifaExceso = 0.0m;
            }
        }

        private double ObtenerPesoMaximoPorTipoVuelo(string tipoVuelo)
        {
            // Regla de negocio: Los vuelos internacionales suelen permitir hasta 23kg, los nacionales 15kg o 20kg (ajustable según tu enunciado)
            if (!string.IsNullOrEmpty(tipoVuelo) && tipoVuelo.Equals("Internacional", StringComparison.OrdinalIgnoreCase))
            {
                return 23.0; // Límite para internacional
            }

            return 15.0; // Límite por defecto para vuelos nacionales
        }
    }
}
