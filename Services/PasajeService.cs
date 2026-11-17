using System.Collections.Generic;
using System.Linq;
using CheckInApi.Modelos;
using CheckInApi.Modelos.Exceptions;

namespace CheckInApi.Services
{
    public class PasajeService
    {
        private readonly IPasajeroRepository _repositorio;

        public PasajeService(IPasajeroRepository repositorio)
        {
            _repositorio = repositorio;
        }

        public List<Pasaje> ListarPasajes()
        {
            return _repositorio.ObtenerTodos()
                .Where(p => p.Pasaje != null)
                .Select(p => p.Pasaje!)
                .ToList();
        }

        public Pasaje? ObtenerPasaje(int idPasajero)
        {
            Pasajero? pasajero = _repositorio.ObtenerPorId(idPasajero);
            if (pasajero == null)
                return null;

            return pasajero.Pasaje;
        }

        public (bool exito, string mensaje, bool noEncontrado) AgregarPasaje(int idPasajero, PasajeRequest datos)
        {
            Pasajero? pasajero = _repositorio.ObtenerPorId(idPasajero);
            if (pasajero == null)
                return (false, "No existe un pasajero con el ID indicado.", true);

            try
            {
                Pasaje nuevo = new Pasaje(
                    datos.TipoVuelo ?? string.Empty,
                    datos.TipoTarifa ?? string.Empty,
                    datos.NroVuelo ?? string.Empty
                );
                nuevo.IdPasaje = pasajero.Id;

                pasajero.AsignarPasaje(nuevo);

                if (!_repositorio.Actualizar(pasajero))
                    return (false, "No se pudo guardar el pasaje para el pasajero indicado.", true);

                return (true, "Pasaje agregado correctamente.", false);
            }
            catch (DatosInvalidosException ex)
            {
                return (false, ex.Message, false);
            }
        }

        public (bool exito, string mensaje, bool noEncontrado) ActualizarPasaje(int idPasajero, PasajeRequest datos)
        {
            Pasajero? pasajero = _repositorio.ObtenerPorId(idPasajero);
            if (pasajero == null || pasajero.Pasaje == null)
                return (false, "No se encontró un pasaje asignado al pasajero indicado.", true);

            try
            {
                Pasaje actual = pasajero.Pasaje;
                string tipoVuelo = datos.TipoVuelo ?? string.Empty;
                string tipoTarifa = datos.TipoTarifa ?? string.Empty;
                Pasaje actualizado = new Pasaje(
                    actual.IdPasaje == 0 ? pasajero.Id : actual.IdPasaje,
                    Pasaje.CalcularPrecioBase(tipoVuelo),
                    Pasaje.CalcularPrecioFinal(tipoVuelo, tipoTarifa),
                    actual.FechaEmision,
                    pasajero.Documento,
                    actual.Asiento,
                    tipoVuelo,
                    tipoTarifa,
                    datos.NroVuelo ?? string.Empty
                );

                pasajero.AsignarPasaje(actualizado);

                if (!_repositorio.Actualizar(pasajero))
                    return (false, "No se pudo actualizar el pasaje para el pasajero indicado.", true);

                return (true, "Pasaje actualizado correctamente.", false);
            }
            catch (DatosInvalidosException ex)
            {
                return (false, ex.Message, false);
            }
        }

        public (bool exito, string mensaje) EliminarPasaje(int idPasajero)
        {
            Pasajero? pasajero = _repositorio.ObtenerPorId(idPasajero);
            if (pasajero == null)
                return (false, "No se encontró el pasajero.");

            if (pasajero.Pasaje == null)
                return (false, "El pasajero no tiene un pasaje asignado.");

            // USAMOS EL MÉTODO PÚBLICO DE LA CLASE PASAJERO EN VEZ DE "="
            pasajero.QuitarPasaje();

            _repositorio.Actualizar(pasajero);
            return (true, "Pasaje eliminado correctamente.");
        }
    }
}