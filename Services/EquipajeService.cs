using System;
using System.Collections.Generic;
using System.Linq;
using CheckInApi.Modelos;
using CheckInApi.Modelos.Exceptions;
using CheckInApi.Repositorios;

namespace CheckInApi.Services
{
    public class EquipajeService
    {
        private readonly IPasajeroRepository _pasajeroRepository;

        private const decimal TarifaPorKiloExcedido = 2500.0m;

        public EquipajeService(IPasajeroRepository pasajeroRepository)
        {
            _pasajeroRepository = pasajeroRepository;
        }

        public List<Equipaje> ListarEquipajes()
        {
            return _pasajeroRepository.ObtenerTodos()
                .SelectMany(p => p.getEquipajes())
                .ToList();
        }

        // null si el pasajero no existe; lista vacía si no tiene equipajes.
        public List<Equipaje>? ListarPorPasajero(int idPasajero)
        {
            Pasajero? pasajero = _pasajeroRepository.ObtenerPorId(idPasajero);
            return pasajero?.getEquipajes().ToList();
        }

        public Equipaje? ObtenerPorCodigo(int idPasajero, string codigoEtiqueta)
        {
            Pasajero? pasajero = _pasajeroRepository.ObtenerPorId(idPasajero);
            if (pasajero == null)
            {
                return null;
            }

            return pasajero.getEquipajes().FirstOrDefault(e =>
                string.Equals(e.CodigoEtiqueta, codigoEtiqueta, StringComparison.OrdinalIgnoreCase));
        }

        public void RegistrarEquipaje(Equipaje equipaje)
        {
            if (!int.TryParse(equipaje.PasajeroId, out int idPasajero))
            {
                throw new EquipajeException("El ID del pasajero no es válido.");
            }

            Pasajero? pasajeroAsociado = _pasajeroRepository.ObtenerPorId(idPasajero);
            if (pasajeroAsociado == null)
            {
                throw new EquipajeException($"Error de integridad: El pasajero con ID '{equipaje.PasajeroId}' no existe en el sistema.");
            }

            if (pasajeroAsociado.Pasaje == null)
            {
                throw new EquipajeException($"Error de integridad: El pasajero con ID '{equipaje.PasajeroId}' no tiene un pasaje asignado. Asigne un pasaje antes de registrar equipajes.");
            }

            if (equipaje.PesoKg <= 0)
            {
                throw new EquipajeException("El peso del equipaje debe ser mayor a 0 kg.");
            }

            if (string.IsNullOrWhiteSpace(equipaje.CodigoEtiqueta))
            {
                throw new EquipajeException("El código de etiqueta es obligatorio.");
            }

            bool repetido = pasajeroAsociado.getEquipajes().Any(e =>
                string.Equals(e.CodigoEtiqueta, equipaje.CodigoEtiqueta, StringComparison.OrdinalIgnoreCase));
            if (repetido)
            {
                throw new EquipajeException($"Ya existe un equipaje con el código de etiqueta '{equipaje.CodigoEtiqueta}' para este pasajero.");
            }

            equipaje.PasajeroId = pasajeroAsociado.Id.ToString();
            equipaje.TarifaExceso = CalcularTarifaExceso(equipaje.PesoKg, pasajeroAsociado.Pasaje);

            pasajeroAsociado.agregarEquipaje(equipaje);
            if (!_pasajeroRepository.Actualizar(pasajeroAsociado))
            {
                throw new EquipajeException("No se pudo guardar el equipaje en el archivo de datos.");
            }
        }

        public (bool exito, string mensaje, bool noEncontrado) ActualizarEquipaje(int idPasajero, string codigoEtiqueta, EquipajeRequest datos)
        {
            Pasajero? pasajero = _pasajeroRepository.ObtenerPorId(idPasajero);
            if (pasajero == null)
            {
                return (false, $"No se encontró un pasajero con ID {idPasajero}.", true);
            }

            if (pasajero.Pasaje == null)
            {
                return (false, "No existe un pasaje asociado al pasajero indicado.", true);
            }

            Equipaje? equipaje = pasajero.getEquipajes().FirstOrDefault(e =>
                string.Equals(e.CodigoEtiqueta, codigoEtiqueta, StringComparison.OrdinalIgnoreCase));
            if (equipaje == null)
            {
                return (false, $"No se encontró un equipaje con el código '{codigoEtiqueta}' para el pasajero con ID {idPasajero}.", true);
            }

            if (datos.PesoKg <= 0)
            {
                return (false, "El peso del equipaje debe ser mayor a 0 kg.", false);
            }

            equipaje.PesoKg = datos.PesoKg;
            equipaje.TipoEquipaje = datos.TipoEquipaje ?? string.Empty;
            equipaje.TarifaExceso = CalcularTarifaExceso(equipaje.PesoKg, pasajero.Pasaje);

            if (!_pasajeroRepository.Actualizar(pasajero))
            {
                return (false, "No se pudo actualizar el equipaje en el archivo de datos.", false);
            }

            return (true, "Equipaje actualizado correctamente.", false);
        }

        public (bool exito, string mensaje, bool noEncontrado) EliminarEquipaje(int idPasajero, string codigoEtiqueta)
        {
            Pasajero? pasajero = _pasajeroRepository.ObtenerPorId(idPasajero);
            if (pasajero == null)
            {
                return (false, $"No se encontró un pasajero con ID {idPasajero}.", true);
            }

            if (!pasajero.quitarEquipaje(codigoEtiqueta))
            {
                return (false, $"No se encontró un equipaje con el código '{codigoEtiqueta}' para el pasajero con ID {idPasajero}.", true);
            }

            if (!_pasajeroRepository.Actualizar(pasajero))
            {
                return (false, "No se pudo eliminar el equipaje del archivo de datos.", false);
            }

            return (true, "Equipaje eliminado correctamente.", false);
        }

        private decimal CalcularTarifaExceso(double pesoKg, Pasaje pasaje)
        {
            double pesoMaximoPermitido = ObtenerPesoMaximoPorTipoVuelo(pasaje.TipoVuelo);

            if (pesoKg <= pesoMaximoPermitido)
            {
                return 0.0m;
            }

            double excesoKg = pesoKg - pesoMaximoPermitido;
            return (decimal)excesoKg * TarifaPorKiloExcedido;
        }

        // 23 kg para vuelos internacionales y 15 kg para nacionales.
        private double ObtenerPesoMaximoPorTipoVuelo(string tipoVuelo)
        {
            bool esInternacional = !string.IsNullOrEmpty(tipoVuelo) &&
                (tipoVuelo.Equals("I", StringComparison.OrdinalIgnoreCase) ||
                 tipoVuelo.Equals("Internacional", StringComparison.OrdinalIgnoreCase));

            return esInternacional ? 23.0 : 15.0;
        }
    }
}
