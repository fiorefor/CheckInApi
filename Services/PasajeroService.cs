using System.Text.RegularExpressions;
using CheckInApi.Modelos;
using CheckInApi.Modelos.Exceptions;

// =========================================================================
// CAPA DE SERVICIO (lógica de negocio) para Pasajeros
// -------------------------------------------------------------------------
// Flujo de una petición:
//   Controller  ->  PasajeroService  ->  IPasajeroRepository  ->  archivo
//
// Este servicio:
//   1) VALIDA formatos (DNI, Email, Teléfono).
//   2) VERIFICA DUPLICADOS antes de un alta (consultando el repositorio).
//   3) LANZA excepciones personalizadas cuando una regla se rompe
//      (DatosInvalidosException, PasajeroExistenteException, ...).
//   4) CAPTURA esas excepciones en los métodos públicos y las convierte en
//      la tupla (exito, mensaje) que el Controller ya sabe manejar.
//
// ¿Por qué lanzar y capturar en el mismo servicio?
//   Los métodos de validación chicos (ValidarDatos, VerificarDuplicado) son
//   más simples si ante un error simplemente "tiran" una excepción: cortan
//   la ejecución en el acto, sin tener que devolver bool y chequearlo en
//   cada paso. Después, UN solo bloque try/catch en Agregar decide qué
//   responder según el TIPO de excepción.
// =========================================================================
public class PasajeroService
{
    private readonly IPasajeroRepository repositorio;

    // ---------------- Expresiones regulares (Regex) ----------------
    // "static readonly": se crean UNA sola vez para toda la aplicación
    // (no en cada llamada), lo que es más eficiente.

    // DNI argentino: 7 u 8 dígitos (ej: 1234567 o 12345678).
    //   ^      -> inicio del texto
    //   \d     -> un dígito (0-9)
    //   {7,8}  -> entre 7 y 8 veces lo anterior
    //   $      -> fin del texto
    // Los puntos ("12.345.678") se quitan ANTES con NormalizarDni.
    private static readonly Regex RegexDni = new Regex(@"^\d{7,8}$");

    // Email: algo@algo.algo (validación razonable, no exhaustiva).
    //   [^@\s]+  -> uno o más caracteres que NO sean "@" ni espacios
    //   @        -> una arroba literal
    //   \.       -> un punto literal (el "\" escapa el punto)
    //   [^@\s]{2,} -> la terminación (com, ar, ...) con al menos 2 caracteres
    private static readonly Regex RegexEmail = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$");

    // Teléfono: permite dígitos, espacios, guiones, paréntesis y un "+" inicial.
    // La cantidad de dígitos se controla aparte en EsTelefonoValido.
    private static readonly Regex RegexTelefono = new Regex(@"^\+?[\d\s\-()]+$");

    public PasajeroService(IPasajeroRepository repositorio)
    {
        // Inyección de dependencias: Program.cs nos pasa el repositorio.
        // El servicio depende de la INTERFAZ, no de la clase concreta,
        // así se puede cambiar el archivo por una base de datos sin tocar esto.
        this.repositorio = repositorio;
    }

    // =====================================================================
    // CONSULTAS (sin cambios de lógica)
    // =====================================================================

    public List<Pasajero> ObtenerTodos()
    {
        return repositorio.ObtenerTodos();
    }

    public Pasajero? ObtenerPorDocumento(string documento)
    {
        // Normalizamos para que "12.345.678" encuentre a "12345678".
        return repositorio.ObtenerPorDocumento(NormalizarDni(documento));
    }

    public Pasajero? ObtenerPorId(int id)
    {
        return repositorio.ObtenerPorId(id);
    }

    // =====================================================================
    // MÉTODOS DE VALIDACIÓN DE FORMATO
    // ---------------------------------------------------------------------
    // Son "public static" porque:
    //   - no usan el repositorio (no dependen del estado del objeto), y
    //   - así se pueden reutilizar o probar fácilmente:
    //       PasajeroService.EsDniValido("12.345.678")  -> true
    // =====================================================================

    /// <summary>
    /// Quita puntos, espacios y guiones del DNI. "12.345.678" -> "12345678".
    /// Guardamos SIEMPRE el DNI normalizado para que la detección de
    /// duplicados funcione aunque el usuario lo escriba de distintas formas.
    /// </summary>
    public static string NormalizarDni(string? dni)
    {
        if (dni == null)
        {
            return string.Empty;
        }
        return dni.Replace(".", "").Replace(" ", "").Replace("-", "").Trim();
    }

    /// <summary>Devuelve true si el DNI (ya normalizado o no) tiene 7 u 8 dígitos.</summary>
    public static bool EsDniValido(string? dni)
    {
        string limpio = NormalizarDni(dni);
        return RegexDni.IsMatch(limpio);
    }

    /// <summary>Devuelve true si el email tiene formato usuario@dominio.ext.</summary>
    public static bool EsEmailValido(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }
        return RegexEmail.IsMatch(email.Trim());
    }

    /// <summary>
    /// El teléfono es OPCIONAL: vacío se considera válido.
    /// Si viene, debe tener caracteres permitidos y entre 6 y 15 dígitos
    /// (15 es el máximo del estándar internacional E.164).
    /// </summary>
    public static bool EsTelefonoValido(string? telefono)
    {
        if (string.IsNullOrWhiteSpace(telefono))
        {
            return true;
        }
        if (!RegexTelefono.IsMatch(telefono.Trim()))
        {
            return false;
        }
        // Count(char.IsDigit) cuenta cuántos caracteres del string son dígitos.
        int cantidadDigitos = telefono.Count(char.IsDigit);
        return cantidadDigitos >= 6 && cantidadDigitos <= 15;
    }

    // =====================================================================
    // MÉTODOS PRIVADOS QUE LANZAN EXCEPCIONES
    // ---------------------------------------------------------------------
    // Usan los validadores de arriba y, si algo está mal, hacen "throw".
    // Un "throw" corta la ejecución del método y "sube" hasta el primer
    // try/catch que capture ese tipo de excepción.
    // =====================================================================

    /// <summary>
    /// Valida el formato de los datos recibidos.
    /// Lanza DatosInvalidosException indicando el campo que falló.
    /// </summary>
    private void ValidarDatos(PasajeroRequest datos)
    {
        if (!EsDniValido(datos.Documento))
        {
            throw new DatosInvalidosException("Documento",
                "El DNI debe tener 7 u 8 dígitos numéricos (se admiten puntos, ej: 12.345.678).");
        }
        if (!EsEmailValido(datos.Email))
        {
            throw new DatosInvalidosException("Email",
                "El email es obligatorio y debe tener el formato usuario@dominio.com.");
        }
        if (!EsTelefonoValido(datos.Telefono))
        {
            throw new DatosInvalidosException("Telefono",
                "El teléfono solo puede contener números, espacios, guiones, paréntesis y '+', con 6 a 15 dígitos.");
        }
    }

    /// <summary>
    /// Verifica que el DNI no pertenezca ya a otro pasajero.
    /// </summary>
    /// <param name="dni">DNI ya normalizado.</param>
    /// <param name="idPropio">
    /// Al ACTUALIZAR, el pasajero se encuentra a sí mismo con su propio DNI;
    /// eso no es un duplicado. Por eso pasamos su ID para ignorarlo.
    /// Al dar de ALTA se deja en null (no tiene ID todavía).
    /// </param>
    private void VerificarDuplicado(string dni, int? idPropio = null)
    {
        Pasajero? existente = repositorio.ObtenerPorDocumento(dni);

        // Hay duplicado si existe alguien con ese DNI Y no es el mismo pasajero.
        if (existente != null && existente.Id != idPropio)
        {
            throw new PasajeroExistenteException(dni);
        }
    }

    // =====================================================================
    // ALTA DE PASAJERO
    // =====================================================================
    public (bool exito, string mensaje) Agregar(PasajeroRequest datos)
    {
        try
        {
            // PASO 1: validar formatos. Si falla -> DatosInvalidosException.
            ValidarDatos(datos);

            // PASO 2: normalizar el DNI ("12.345.678" -> "12345678").
            string dni = NormalizarDni(datos.Documento);

            // PASO 3: verificar duplicados ANTES de crear/guardar.
            // Si ya existe -> PasajeroExistenteException.
            VerificarDuplicado(dni);

            // PASO 4: crear el objeto de dominio. El constructor de Pasajero
            // puede lanzar DatosInvalidosException (ej: nombre vacío).
            Pasajero nuevo = new Pasajero(
                datos.Nombre ?? string.Empty,
                datos.Apellido ?? string.Empty,
                dni,
                datos.Email ?? string.Empty,
                datos.Telefono ?? string.Empty,
                datos.PasajeroFrecuente ?? string.Empty
            );

            // PASO 5: persistir. El repositorio devuelve false si falló la escritura.
            if (!repositorio.Agregar(nuevo))
            {
                return (false, "Error al guardar el pasajero en el repositorio.");
            }
            return (true, $"Pasajero agregado exitosamente con ID {nuevo.Id}.");
        }
        // ---- CAPTURA de excepciones ----
        // El orden de los catch importa: van de la MÁS específica a la MÁS general.
        // Si pusiéramos "catch (Exception)" primero, atraparía todo y los demás
        // catch nunca se ejecutarían (el compilador incluso da error).
        catch (PasajeroExistenteException ex)
        {
            // Duplicado: ex.Message = "Ya existe un pasajero registrado con el DNI ..."
            return (false, ex.Message);
        }
        catch (DatosInvalidosException ex)
        {
            // Formato inválido o campo obligatorio vacío.
            return (false, $"Datos inválidos: {ex.Message}");
        }
    }

    // =====================================================================
    // ACTUALIZACIÓN DE PASAJERO
    // =====================================================================
    public (bool exito, string mensaje) Actualizar(int id, PasajeroRequest datos)
    {
        try
        {
            // PASO 1: el pasajero a actualizar tiene que existir.
            Pasajero? actual = repositorio.ObtenerPorId(id);
            if (actual == null)
            {
                throw new PasajeroNoEncontradoException(id);
            }

            // PASO 2: mismas validaciones de formato que en el alta.
            ValidarDatos(datos);
            string dni = NormalizarDni(datos.Documento);

            // PASO 3: si cambió el DNI, no puede chocar con el de OTRO pasajero.
            // Pasamos "id" para que no se detecte a sí mismo como duplicado.
            VerificarDuplicado(dni, id);

            // PASO 4: construir el pasajero actualizado conservando su ID.
            Pasajero actualizado = new Pasajero(
                datos.Nombre ?? string.Empty,
                datos.Apellido ?? string.Empty,
                dni,
                datos.Email ?? string.Empty,
                datos.Telefono ?? string.Empty,
                datos.PasajeroFrecuente ?? string.Empty,
                id
            );
            // Conservamos el pasaje que ya tenía (si no, se perdería al guardar).
            // El pasaje es OPCIONAL: solo lo pasamos si el pasajero tenía uno.
            if (actual.TienePasaje)
            {
                // Si en esta actualización cambió el DNI, el pasaje viejo todavía
                // tiene el DNI anterior; lo actualizamos para que AsignarPasaje
                // no lo rechace como "pasaje de otra persona".
                actual.Pasaje!.DniPasajero = dni;   // "!" = le aseguramos al compilador que no es null
                actualizado.AsignarPasaje(actual.Pasaje);
            }

            if (!repositorio.Actualizar(actualizado))
            {
                return (false, "Error al actualizar el pasajero en el repositorio.");
            }
            return (true, "Pasajero actualizado exitosamente.");
        }
        catch (PasajeroNoEncontradoException ex)
        {
            return (false, ex.Message);
        }
        catch (PasajeroExistenteException ex)
        {
            return (false, ex.Message);
        }
        catch (DatosInvalidosException ex)
        {
            return (false, $"Datos inválidos: {ex.Message}");
        }
    }

    // =====================================================================
    // BAJA DE PASAJERO (sin cambios de lógica)
    // =====================================================================
    public (bool exito, string mensaje) Eliminar(int id)
    {
        if (repositorio.ObtenerPorId(id) == null)
        {
            return (false, "No existe un pasajero con el ID proporcionado.");
        }

        bool exito = repositorio.Eliminar(id);
        if (!exito)
        {
            return (false, "Error al eliminar el pasajero del repositorio.");
        }
        return (true, "Pasajero eliminado exitosamente.");
    }
}
