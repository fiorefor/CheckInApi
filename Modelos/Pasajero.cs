using System.Globalization;   // CultureInfo: para guardar decimales y fechas sin depender del idioma de la PC
using CheckInApi.Modelos.Exceptions;

namespace CheckInApi.Modelos
{
    // =====================================================================
    // CLASE DE DOMINIO: Pasajero
    // ---------------------------------------------------------------------
    // Representa a una persona que viaja. Datos principales:
    //   ID, DNI (lo guardamos en "Documento"), Nombre, Apellido, Email, Teléfono.
    // Además conserva lo que ya existía: PasajeroFrecuente, Pasaje y Equipajes.
    //
    // RESPONSABILIDADES (reparto entre capas):
    //   - La clase de dominio garantiza su INTEGRIDAD MÍNIMA: no puede existir
    //     un Pasajero sin DNI, nombre o apellido. Si llegan vacíos -> excepción.
    //   - El SERVICIO (PasajeroService) se encarga de las reglas de NEGOCIO:
    //     formato de DNI/Email y que no haya duplicados (eso requiere consultar
    //     el repositorio, cosa que una entidad no debe hacer).
    // =====================================================================
    public class Pasajero
    {
        // ---------------- Atributos privados (encapsulamiento) ----------------
        // Son privados para que nadie de afuera los cambie sin pasar
        // por las propiedades (donde podemos poner validaciones).
        private string _nombre;
        private string _apellido;
        private string _documento;   // DNI del pasajero
        private string _email;
        private string _telefono;
        private string _pasajeroFrecuente;
        private List<Equipaje> _equipajes = new List<Equipaje>();

        // ---------------- Constructor ----------------
        // Recibe todos los datos y valida los obligatorios.
        // "id = 0" es un parámetro opcional: al crear un pasajero nuevo
        // no tenemos ID todavía (lo asigna el repositorio al guardarlo).
        public Pasajero(string nombre,
            string apellido,
            string documento,
            string email,
            string telefono,
            string pasajeroFrecuente,
            int id = 0)
        {
            // Validaciones de campos OBLIGATORIOS.
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
            // Trim() quita espacios al principio y al final ("  Ana " -> "Ana").
            // El operador "??" usa string.Empty si el valor viene null.
            _nombre = nombre.Trim();
            _apellido = apellido.Trim();
            _documento = documento.Trim();
            _email = (email ?? string.Empty).Trim().ToLower(); // emails sin mayúsculas para comparar fácil
            _telefono = (telefono ?? string.Empty).Trim();
            _pasajeroFrecuente = pasajeroFrecuente ?? string.Empty;
        }

        // ---------------- Propiedades ----------------

        // Propiedad autoimplementada: C# crea el atributo oculto por nosotros.
        public int Id { get; set; }

        // ---------------- Pasaje (OPCIONAL) ----------------
        // El "?" en "Pasaje?" indica que puede ser null: un pasajero puede
        // estar registrado SIN pasaje todavía (se lo asignan después en el check-in).
        //
        // "private set": desde afuera se puede LEER (pasajero.Pasaje) pero NO
        // asignar directamente. Para cambiarlo hay que usar AsignarPasaje()
        // o QuitarPasaje(), que aplican las reglas (ej: que el DNI coincida).
        public Pasaje? Pasaje { get; private set; }

        // Propiedad calculada (solo get): true si tiene pasaje asignado.
        // Evita escribir "pasajero.Pasaje != null" en todos lados.
        public bool TienePasaje
        {
            get { return Pasaje != null; }
        }

        public string Nombre
        {
            get { return _nombre; }
            set
            {
                // Solo aceptamos el cambio si el valor no está vacío.
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

        /// <summary>DNI del pasajero.</summary>
        public string Documento
        {
            get { return _documento; }
            set
            {
                _documento = value;
                // Si cambia el DNI del pasajero, su pasaje (si tiene) debe
                // seguir apuntando a él (si no tiene pasaje, no hay nada que actualizar).
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

        // ---------------- Métodos ----------------

        public List<Equipaje> getEquipajes()
        {
            return _equipajes;
        }

        public void agregarEquipaje(Equipaje unEquipaje)
        {
            _equipajes.Add(unEquipaje);
        }

        // ---------------- Manejo del pasaje ----------------

        /// <summary>
        /// Asigna (o reemplaza) el pasaje del pasajero, verificando que le pertenezca.
        /// Lanza DatosInvalidosException si el pasaje es null o es de otro DNI.
        /// </summary>
        public void AsignarPasaje(Pasaje pasaje)
        {
            if (pasaje == null)
            {
                // Para sacar el pasaje está QuitarPasaje(); acá exigimos un pasaje real.
                throw new DatosInvalidosException("Pasaje", "El pasaje no puede ser nulo. Use QuitarPasaje() para eliminarlo.");
            }

            // El constructor corto de Pasaje (tipoVuelo, tipoTarifa, nroVuelo)
            // pone "0" como DNI de relleno porque todavía no sabe de quién es.
            // En ese caso (o si viene vacío) el pasaje "adopta" el DNI de este pasajero.
            bool sinDueño = string.IsNullOrWhiteSpace(pasaje.DniPasajero) || pasaje.DniPasajero == "0";

            if (!sinDueño && LimpiarDni(pasaje.DniPasajero) != LimpiarDni(_documento))
            {
                // El pasaje ya tiene un DNI y NO es el de este pasajero -> error.
                throw new DatosInvalidosException("Pasaje",
                    $"El pasaje pertenece al DNI {pasaje.DniPasajero} y no al pasajero {_documento}.");
            }

            pasaje.DniPasajero = _documento;   // dejamos el mismo formato de DNI que el pasajero
            Pasaje = pasaje;
        }

        /// <summary>Deja al pasajero sin pasaje (Pasaje = null).</summary>
        public void QuitarPasaje()
        {
            Pasaje = null;
        }

        // Compara DNIs ignorando puntos y espacios ("12.345.678" == "12345678").
        private static string LimpiarDni(string dni)
        {
            return dni.Replace(".", "").Replace(" ", "").Trim();
        }

        // ---------------- Persistencia en archivo de texto ----------------
        // Cada pasajero se guarda en UNA línea con los campos separados por "|".
        // Formato (posición -> campo):
        //    0 Id | 1 Nombre | 2 Apellido | 3 DNI | 4 PasajeroFrecuente
        //    5 Email | 6 Teléfono
        //    7 IdPasaje | 8 TipoVuelo | 9 TipoTarifa | 10 NroVuelo  |           <-- campos del pasaje (si tiene)
        //   11 PrecioBase | 12 PrecioFinal
        //   13 FechaEmision | 14 Asiento                      
        // Si NO tiene pasaje, las posiciones 7-14 quedan vacías.
        // El DNI del pasaje NO se guarda: siempre es el mismo que el del pasajero (pos. 3).

        public override string ToString()
        {
            // Datos propios del pasajero (posiciones 0 a 6)
            string linea = $"{Id}|{Nombre}|{Apellido}|{Documento}|{PasajeroFrecuente}|{Email}|{Telefono}";

            if (Pasaje == null)
            {
                // Sin pasaje: 8 campos vacíos.
                linea += $"||||||||";
            }
            else
            {
                // CultureInfo.InvariantCulture: guarda "1500.50" (con punto) aunque la PC
                // esté en español (que usaría "1500,50"). Así el archivo se lee igual en
                // cualquier máquina.
                // Formato "o" de fecha (ISO 8601, ej: 2026-11-13T10:30:00.0000000) se puede
                // volver a leer exactamente igual con DateTime.Parse.
                linea += $"|{Pasaje.IdPasaje}" 
                       + $"|{Pasaje.TipoVuelo}|{Pasaje.TipoTarifa}|{Pasaje.NroVuelo}"
                       + $"|{Pasaje.PrecioBase.ToString(CultureInfo.InvariantCulture)}"
                       + $"|{Pasaje.PrecioFinal.ToString(CultureInfo.InvariantCulture)}"
                       + $"|{Pasaje.FechaEmision.ToString("o", CultureInfo.InvariantCulture)}"
                       + $"|{Pasaje.Asiento}";
            }
            return linea;
        }

        // Método "static": se llama sobre la clase (Pasajero.FromString(...))
        // y no sobre un objeto. Hace lo inverso de ToString: línea -> objeto.
        public static Pasajero FromString(string linea)
        {
            string[] partes = linea.Split('|');
            int id = int.Parse(partes[0]);

            Pasajero pasajero = new Pasajero(Campo(partes, 1), Campo(partes, 2), Campo(partes, 3),
                Campo(partes, 5), Campo(partes, 6), Campo(partes, 4), id);

            // ---- Pasaje OPCIONAL (posiciones 7 a 14) ----
            // Solo existe si tipo de vuelo, tarifa y número de vuelo tienen valor.
            string tipoVuelo = Campo(partes, 8);
            string tipoTarifa = Campo(partes, 9);
            string nroVuelo = Campo(partes, 10);
            bool tienePasaje = tipoVuelo != "" && tipoTarifa != "" && nroVuelo != "";

            if (!tienePasaje)
            {
                return pasajero;   // pasajero sin pasaje: Pasaje queda en null
            }

            Pasaje pasaje = new Pasaje(
                int.Parse(Campo(partes, 7)),
                decimal.Parse(Campo(partes, 11), CultureInfo.InvariantCulture),
                decimal.Parse(Campo(partes, 12), CultureInfo.InvariantCulture),
                DateTime.Parse(Campo(partes, 13), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                pasajero.Documento,   // el DNI del pasaje es el del pasajero
                Campo(partes, 14),
                tipoVuelo,
                tipoTarifa,
                nroVuelo);

            // AsignarPasaje vincula el DNI y valida que corresponda a este pasajero.
            pasajero.AsignarPasaje(pasaje);
            return pasajero;
        }

        // Devuelve el campo en la posición indicada, o "" si la línea no lo tiene
        // (líneas viejas más cortas). Evita IndexOutOfRangeException.
        private static string Campo(string[] partes, int posicion)
        {
            if (partes.Length > posicion)
            {
                // La línea tiene ese campo: lo devolvemos sin espacios.
                return partes[posicion].Trim();
            }
            else
            {
                // La línea es más corta y no llega a esa posición.
                return string.Empty;
            }
        }
    }
}
