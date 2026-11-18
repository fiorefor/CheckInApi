using System;
using CheckInApi.Modelos.Exceptions;

namespace CheckInApi.Modelos
{
    public class Pasaje
    {
        private int _idPasaje;
        private decimal _precioBase;
        private decimal _precioFinal;
        private DateTime _fechaEmision;
        private string _dniPasajero = string.Empty;
        private string _asiento = string.Empty;
        private string _tipoVuelo = string.Empty;
        private string _tipoTarifa = string.Empty;
        private string _nroVuelo = string.Empty;

        public Pasaje() { }

        public Pasaje(int idPasaje, decimal precioBase, decimal precioFinal, DateTime fechaEmision, string dniPasajero, string asiento, string tipoVuelo, string tipoTarifa, string nroVuelo)
        {
            if (string.IsNullOrEmpty(dniPasajero))
            {
                throw new DatosInvalidosException("Dni Pasajero", "El DNI del pasajero no puede estar vacío.");
            }
            if (string.IsNullOrWhiteSpace(nroVuelo))
            {
                throw new DatosInvalidosException("Número de Vuelo", "El número de vuelo no puede estar vacío.");
            }
            if (string.IsNullOrWhiteSpace(asiento))
            {
                throw new DatosInvalidosException("Asiento", "El asiento no puede estar vacío.");
            }
            string vueloNorm = tipoVuelo?.Trim().ToUpper() ?? "";
            if (vueloNorm != "N" && vueloNorm != "I")
            {
                throw new DatosInvalidosException("Tipo de Vuelo", "El tipo de vuelo debe ser 'Nacional' o 'Internacional'.");
            }
            string tarifaNorm = tipoTarifa?.Trim().ToUpper() ?? "";
            if (tarifaNorm != "P" && tarifaNorm != "B" && tarifaNorm != "PE" && tarifaNorm != "F")
            {
                throw new DatosInvalidosException("Tipo de Tarifa", "El tipo de tarifa debe ser 'P' (Primera), 'B' (Business), 'PE' (Premium Economy) o 'F' (Economy).");
            }
            if (precioBase <= 0)
            {
                throw new DatosInvalidosException("Precio Base", "El precio base debe ser mayor a cero.");
            }
            _idPasaje = idPasaje;
            _precioBase = precioBase;
            _precioFinal = precioFinal;
            _dniPasajero = dniPasajero.Trim();
            _asiento = asiento.Trim().ToUpper();
            _tipoVuelo = vueloNorm;
            _tipoTarifa = tarifaNorm;
            _fechaEmision = fechaEmision;
            _nroVuelo = nroVuelo.Trim().ToUpper();
        }

        public Pasaje(string tipoVuelo, string tipoTarifa, string nroVuelo, string asiento = "1")
            : this(0, CalcularPrecioBase(tipoVuelo), CalcularPrecioFinal(tipoVuelo, tipoTarifa), DateTime.Now, "0", asiento, tipoVuelo, tipoTarifa, nroVuelo)
        {
        }

        public static decimal CalcularPrecioBase(string tipoVuelo)
        {
            string vueloNorm = tipoVuelo?.Trim().ToUpperInvariant() ?? string.Empty;
            return vueloNorm switch
            {
                "N" => 200000m,
                "I" => 500000m,
                _ => throw new DatosInvalidosException("Tipo de Vuelo", "El tipo de vuelo debe ser 'Nacional' o 'Internacional'.")
            };
        }

        public static decimal CalcularPrecioFinal(string tipoVuelo, string tipoTarifa)
        {
            string tarifaNorm = tipoTarifa?.Trim().ToUpperInvariant() ?? string.Empty;
            decimal adicionalTarifa = tarifaNorm switch
            {
                "F" => 250000m,
                "PE" => 300000m,
                "B" => 350000m,
                "P" => 400000m,
                _ => throw new DatosInvalidosException("Tipo de Tarifa", "El tipo de tarifa debe ser 'P' (Primera/Premium), 'B' (Business), 'PE' (Premium Economy) o 'F' (Economy).")
            };

            return CalcularPrecioBase(tipoVuelo) + adicionalTarifa;
        }

        public int IdPasaje
        {
            get { return _idPasaje; }
            set { _idPasaje = value; }
        }
        public decimal PrecioBase
        {
            get { return _precioBase; }
            set { _precioBase = value; }
        }
        public decimal PrecioFinal
        {
            get { return _precioFinal; }
            set { _precioFinal = value; }
        }
        public DateTime FechaEmision
        {
            get { return _fechaEmision; }
            set { _fechaEmision = value; }
        }
        public string DniPasajero
        {
            get { return _dniPasajero; }
            set { _dniPasajero = value; }
        }
        public string Asiento
        {
            get { return _asiento; }
            set { _asiento = value; }
        }
        public string TipoVuelo
        {
            get { return _tipoVuelo; }
            set { _tipoVuelo = value; }
        }
        public string TipoTarifa
        {
            get { return _tipoTarifa; }
            set { _tipoTarifa = value; }
        }
        public string NroVuelo
        {
            get { return _nroVuelo; }
            set { _nroVuelo = value; }
        }
    }
}
