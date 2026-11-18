using System.Globalization;
using CheckInApi.Modelos.Exceptions;

namespace CheckInApi.Modelos
{
    // Entidad del dominio: una persona que viaja, con su pasaje y equipajes.
    public class Pasajero
    {
        private string _nombre;
        private string _apellido;
        private string _documento;
        private string _email;
        private string _telefono;
        private string _pasajeroFrecuente;
        private List<Equipaje> _equipajes = new List<Equipaje>();

        public Pasajero(string nombre,
            string apellido,
            string documento,
            string email,
            string telefono,
            string pasajeroFrecuente,
            int id = 0)
        {
            if (string.IsNullOrWhiteSpace(documento))
            {
                throw new DatosInvalidosException("Documento", "El DNI no puede estar vacío.");
            }
            if (string.IsNullOrWhiteSpace(nombre))
            {
                throw new DatosInvalidosException("Nombre", "El nombre no puede estar vacío.");
            }
            if (string.IsNullOrWhiteSpace(apellido))
            {
                throw new DatosInvalidosException("Apellido", "El apellido no puede estar vacío.");
            }

            Id = id;
            _nombre = nombre.Trim();
            _apellido = apellido.Trim();
            _documento = documento.Trim();
            _email = (email ?? string.Empty).Trim().ToLower();
            _telefono = (telefono ?? string.Empty).Trim();
            _pasajeroFrecuente = pasajeroFrecuente ?? string.Empty;
        }

        public int Id { get; set; }

        public Pasaje? Pasaje { get; private set; }

        public bool TienePasaje
        {
            get { return Pasaje != null; }
        }

        public IReadOnlyList<Equipaje> Equipajes
        {
            get { return _equipajes; }
        }

        public string Nombre
        {
            get { return _nombre; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    _nombre = value.Trim();
                }
            }
        }

        public string Apellido
        {
            get { return _apellido; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    _apellido = value.Trim();
                }
            }
        }

        public string Documento
        {
            get { return _documento; }
            set
            {
                _documento = value;
                // El pasaje siempre apunta al DNI del pasajero.
                if (Pasaje != null)
                {
                    Pasaje.DniPasajero = value;
                }
            }
        }

        public string Email
        {
            get { return _email; }
            set { _email = (value ?? string.Empty).Trim().ToLower(); }
        }

        public string Telefono
        {
            get { return _telefono; }
            set { _telefono = (value ?? string.Empty).Trim(); }
        }

        public string PasajeroFrecuente
        {
            get { return _pasajeroFrecuente; }
            set { _pasajeroFrecuente = value; }
        }

        public List<Equipaje> getEquipajes()
        {
            return _equipajes;
        }

        public void agregarEquipaje(Equipaje unEquipaje)
        {
            _equipajes.Add(unEquipaje);
        }

        public bool quitarEquipaje(string codigoEtiqueta)
        {
            if (string.IsNullOrWhiteSpace(codigoEtiqueta))
            {
                return false;
            }

            return _equipajes.RemoveAll(e =>
                string.Equals(e.CodigoEtiqueta, codigoEtiqueta, StringComparison.OrdinalIgnoreCase)) > 0;
        }

        // Asigna el pasaje y verifica que pertenezca a este pasajero.
        public void AsignarPasaje(Pasaje pasaje)
        {
            if (pasaje == null)
            {
                throw new DatosInvalidosException("Pasaje", "El pasaje no puede ser nulo. Use QuitarPasaje() para eliminarlo.");
            }

            // El constructor corto de Pasaje deja el DNI en "0" hasta saber el dueño.
            bool sinDueño = string.IsNullOrWhiteSpace(pasaje.DniPasajero) || pasaje.DniPasajero == "0";

            if (!sinDueño && LimpiarDni(pasaje.DniPasajero) != LimpiarDni(_documento))
            {
                throw new DatosInvalidosException("Pasaje",
                    $"El pasaje pertenece al DNI {pasaje.DniPasajero} y no al pasajero {_documento}.");
            }

            pasaje.DniPasajero = _documento;
            Pasaje = pasaje;
        }

        public void QuitarPasaje()
        {
            Pasaje = null;
        }

        private static string LimpiarDni(string dni)
        {
            return dni.Replace(".", "").Replace(" ", "").Trim();
        }

        // Formato de una línea: campos separados por "|".
        // 0 Id | 1 Nombre | 2 Apellido | 3 DNI | 4 PasajeroFrecuente | 5 Email | 6 Teléfono
        // 7 IdPasaje | 8 TipoVuelo | 9 TipoTarifa | 10 NroVuelo | 11 PrecioBase
        // 12 PrecioFinal | 13 FechaEmision | 14 Asiento | 15 Equipajes (opcional)
        // Sin pasaje, las posiciones 7 a 14 quedan vacías. Los equipajes se guardan
        // como "codigo,tipo,peso,tarifa", separados por ";".
        public override string ToString()
        {
            string linea = $"{Id}|{Nombre}|{Apellido}|{Documento}|{PasajeroFrecuente}|{Email}|{Telefono}";

            if (Pasaje == null)
            {
                linea += $"||||||||";
            }
            else
            {
                // InvariantCulture y formato "o" para que el archivo se lea igual en cualquier PC.
                linea += $"|{Pasaje.IdPasaje}" 
                       + $"|{Pasaje.TipoVuelo}|{Pasaje.TipoTarifa}|{Pasaje.NroVuelo}"
                       + $"|{Pasaje.PrecioBase.ToString(CultureInfo.InvariantCulture)}"
                       + $"|{Pasaje.PrecioFinal.ToString(CultureInfo.InvariantCulture)}"
                       + $"|{Pasaje.FechaEmision.ToString("o", CultureInfo.InvariantCulture)}"
                       + $"|{Pasaje.Asiento}";
            }

            string equipajes = SerializarEquipajes();
            if (equipajes.Length > 0)
            {
                linea += $"|{equipajes}";
            }

            return linea;
        }

        public static Pasajero FromString(string linea)
        {
            string[] partes = linea.Split('|');
            int id = int.Parse(partes[0]);

            Pasajero pasajero = new Pasajero(Campo(partes, 1), Campo(partes, 2), Campo(partes, 3),
                Campo(partes, 5), Campo(partes, 6), Campo(partes, 4), id);

            string tipoVuelo = Campo(partes, 8);
            string tipoTarifa = Campo(partes, 9);
            string nroVuelo = Campo(partes, 10);
            bool tienePasaje = tipoVuelo != "" && tipoTarifa != "" && nroVuelo != "";

            if (tienePasaje)
            {
                Pasaje pasaje = new Pasaje(
                    int.Parse(Campo(partes, 7)),
                    decimal.Parse(Campo(partes, 11), CultureInfo.InvariantCulture),
                    decimal.Parse(Campo(partes, 12), CultureInfo.InvariantCulture),
                    DateTime.Parse(Campo(partes, 13), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    pasajero.Documento,
                    Campo(partes, 14),
                    tipoVuelo,
                    tipoTarifa,
                    nroVuelo);

                pasajero.AsignarPasaje(pasaje);
            }

            CargarEquipajes(pasajero, Campo(partes, 15));

            return pasajero;
        }

        private string SerializarEquipajes()
        {
            if (_equipajes.Count == 0)
            {
                return string.Empty;
            }

            List<string> bloques = new List<string>();
            foreach (Equipaje equipaje in _equipajes)
            {
                bloques.Add(string.Join(",",
                    Uri.EscapeDataString(equipaje.CodigoEtiqueta ?? string.Empty),
                    Uri.EscapeDataString(equipaje.TipoEquipaje ?? string.Empty),
                    equipaje.PesoKg.ToString(CultureInfo.InvariantCulture),
                    equipaje.TarifaExceso.ToString(CultureInfo.InvariantCulture)));
            }

            return string.Join(";", bloques);
        }

        private static void CargarEquipajes(Pasajero pasajero, string campo)
        {
            if (string.IsNullOrWhiteSpace(campo))
            {
                return;
            }

            foreach (string bloque in campo.Split(';'))
            {
                if (string.IsNullOrWhiteSpace(bloque))
                {
                    continue;
                }

                string[] datos = bloque.Split(',');
                if (datos.Length != 4)
                {
                    throw new DatosInvalidosException("Equipajes",
                        "El equipaje guardado no tiene el formato esperado.");
                }

                pasajero.agregarEquipaje(new Equipaje
                {
                    CodigoEtiqueta = Uri.UnescapeDataString(datos[0]),
                    PasajeroId = pasajero.Id.ToString(),
                    TipoEquipaje = Uri.UnescapeDataString(datos[1]),
                    PesoKg = double.Parse(datos[2], CultureInfo.InvariantCulture),
                    TarifaExceso = decimal.Parse(datos[3], CultureInfo.InvariantCulture)
                });
            }
        }

        // Devuelve "" si la línea es más corta (archivos de versiones anteriores).
        private static string Campo(string[] partes, int posicion)
        {
            if (partes.Length > posicion)
            {
                return partes[posicion].Trim();
            }

            return string.Empty;
        }
    }
}
