using System;
using System.Collections.Generic;
using System.Linq;
using CheckInApi.Modelos;
using CheckInApi.Modelos.Exceptions;
using CheckInApi.Repositorios;

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
                string asiento = ObtenerAsientoLibre(datos.NroVuelo ?? string.Empty);

                Pasaje nuevo = new Pasaje(
                    datos.TipoVuelo ?? string.Empty,
                    datos.TipoTarifa ?? string.Empty,
                    datos.NroVuelo ?? string.Empty,
                    asiento
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

        // Avión de 30 filas (1 a 30) por 6 columnas (A a F): 180 asientos.
        private string ObtenerAsientoLibre(string nroVuelo)
        {
            const string columnas = "ABCDEF";
            const int filas = 30;

            string vuelo = (nroVuelo ?? string.Empty).Trim().ToUpperInvariant();

            HashSet<string> ocupados = _repositorio.ObtenerTodos()
                .Where(p => p.Pasaje != null &&
                            string.Equals(p.Pasaje.NroVuelo, vuelo, StringComparison.OrdinalIgnoreCase))
                .Select(p => p.Pasaje!.Asiento.Trim().ToUpperInvariant())
                .ToHashSet();

            for (int fila = 1; fila <= filas; fila++)
            {
                foreach (char columna in columnas)
                {
                    string asiento = $"{fila}{columna}";
                    if (!ocupados.Contains(asiento))
                    {
                        return asiento;
                    }
                }
            }

            throw new DatosInvalidosException("Asiento",
                $"No quedan asientos disponibles para el vuelo '{vuelo}'.");
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

        public (bool exito, string mensaje, bool noEncontrado) EliminarPasaje(int idPasajero)
        {
            Pasajero? pasajero = _repositorio.ObtenerPorId(idPasajero);
            if (pasajero == null)
                return (false, $"No se encontró un pasajero con ID {idPasajero}.", true);

            if (pasajero.Pasaje == null)
                return (false, "El pasajero no tiene un pasaje asignado.", true);

            pasajero.QuitarPasaje();

            if (!_repositorio.Actualizar(pasajero))
                return (false, "No se pudo eliminar el pasaje del archivo de datos.", false);

            return (true, "Pasaje eliminado correctamente.", false);
        }
    }
}