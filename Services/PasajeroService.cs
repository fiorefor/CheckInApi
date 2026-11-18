using System.Text.RegularExpressions;
using CheckInApi.Modelos;
using CheckInApi.Modelos.Exceptions;
using CheckInApi.Repositorios;

public class PasajeroService
{
    private readonly IPasajeroRepository repositorio;

    private static readonly Regex RegexDni = new Regex(@"^\d{7,8}$");
    private static readonly Regex RegexEmail = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$");
    private static readonly Regex RegexTelefono = new Regex(@"^\+?[\d\s\-()]+$");

    public PasajeroService(IPasajeroRepository repositorio)
    {
        this.repositorio = repositorio;
    }

    public List<Pasajero> ObtenerTodos()
    {
        return repositorio.ObtenerTodos();
    }

    public Pasajero? ObtenerPorDocumento(string documento)
    {
        return repositorio.ObtenerPorDocumento(NormalizarDni(documento));
    }

    public Pasajero? ObtenerPorId(int id)
    {
        return repositorio.ObtenerPorId(id);
    }

    // Quita puntos, espacios y guiones para que "12.345.678" y "12345678" sean el mismo DNI.
    public static string NormalizarDni(string? dni)
    {
        if (dni == null)
        {
            return string.Empty;
        }
        return dni.Replace(".", "").Replace(" ", "").Replace("-", "").Trim();
    }

    public static bool EsDniValido(string? dni)
    {
        string limpio = NormalizarDni(dni);
        return RegexDni.IsMatch(limpio);
    }

    public static bool EsEmailValido(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }
        return RegexEmail.IsMatch(email.Trim());
    }

    // El teléfono es opcional: vacío es válido. Si viene, exige 6 a 15 dígitos.
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
        int cantidadDigitos = telefono.Count(char.IsDigit);
        return cantidadDigitos >= 6 && cantidadDigitos <= 15;
    }

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

    // idPropio se usa al actualizar: el pasajero no es duplicado de sí mismo.
    private void VerificarDuplicado(string dni, int? idPropio = null)
    {
        Pasajero? existente = repositorio.ObtenerPorDocumento(dni);

        if (existente != null && existente.Id != idPropio)
        {
            throw new PasajeroExistenteException(dni);
        }
    }

    public (bool exito, string mensaje) Agregar(PasajeroRequest datos)
    {
        try
        {
            ValidarDatos(datos);

            string dni = NormalizarDni(datos.Documento);
            VerificarDuplicado(dni);

            Pasajero nuevo = new Pasajero(
                datos.Nombre ?? string.Empty,
                datos.Apellido ?? string.Empty,
                dni,
                datos.Email ?? string.Empty,
                datos.Telefono ?? string.Empty,
                datos.PasajeroFrecuente ?? string.Empty
            );

            if (!repositorio.Agregar(nuevo))
            {
                return (false, "Error al guardar el pasajero en el repositorio.");
            }
            return (true, $"Pasajero agregado exitosamente con ID {nuevo.Id}.");
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

    public (bool exito, string mensaje, bool noEncontrado) Actualizar(int id, PasajeroRequest datos)
    {
        try
        {
            Pasajero? actual = repositorio.ObtenerPorId(id);
            if (actual == null)
            {
                throw new PasajeroNoEncontradoException(id);
            }

            ValidarDatos(datos);
            string dni = NormalizarDni(datos.Documento);
            VerificarDuplicado(dni, id);

            Pasajero actualizado = new Pasajero(
                datos.Nombre ?? string.Empty,
                datos.Apellido ?? string.Empty,
                dni,
                datos.Email ?? string.Empty,
                datos.Telefono ?? string.Empty,
                datos.PasajeroFrecuente ?? string.Empty,
                id
            );

            if (actual.TienePasaje)
            {
                actual.Pasaje!.DniPasajero = dni;
                actualizado.AsignarPasaje(actual.Pasaje);
            }

            foreach (Equipaje equipaje in actual.getEquipajes())
            {
                actualizado.agregarEquipaje(equipaje);
            }

            if (!repositorio.Actualizar(actualizado))
            {
                return (false, "Error al actualizar el pasajero en el repositorio.", false);
            }
            return (true, "Pasajero actualizado exitosamente.", false);
        }
        catch (PasajeroNoEncontradoException ex)
        {
            return (false, ex.Message, true);
        }
        catch (PasajeroExistenteException ex)
        {
            return (false, ex.Message, false);
        }
        catch (DatosInvalidosException ex)
        {
            return (false, $"Datos inválidos: {ex.Message}", false);
        }
    }

    public (bool exito, string mensaje, bool noEncontrado) Eliminar(int id)
    {
        if (repositorio.ObtenerPorId(id) == null)
        {
            return (false, "No existe un pasajero con el ID proporcionado.", true);
        }

        bool exito = repositorio.Eliminar(id);
        if (!exito)
        {
            return (false, "Error al eliminar el pasajero del repositorio.", false);
        }
        return (true, "Pasajero eliminado exitosamente.", false);
    }
}
