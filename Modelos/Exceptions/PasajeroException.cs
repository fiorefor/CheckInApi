// Nota: no hace falta "using System;" porque el proyecto tiene
// <ImplicitUsings>enable</ImplicitUsings> en el .csproj.
namespace CheckInApi.Modelos.Exceptions
{
    // =====================================================================
    // EXCEPCIONES DEL DOMINIO "PASAJERO"
    // ---------------------------------------------------------------------
    // Seguimos el mismo patrón que EquipajeException y DatosException:
    //   1) Una clase BASE (PasajeroException) que hereda de Exception.
    //   2) Clases HIJAS, una por cada situación de error concreta.
    //
    // ¿Por qué una jerarquía y no una sola excepción con un mensaje?
    //   - Porque quien captura puede decidir QUÉ errores atrapar:
    //       catch (PasajeroExistenteException)  -> solo duplicados
    //       catch (PasajeroException)           -> cualquier error de pasajero
    //   - Y cada excepción puede llevar DATOS extra (ej: el DNI repetido),
    //     no solo un texto. Eso sirve para loguear o armar respuestas HTTP.
    // =====================================================================

    /// <summary>
    /// Excepción base para cualquier error de negocio relacionado a un pasajero.
    /// </summary>
    public class PasajeroException : Exception
    {
        // ": base(mensaje)" llama al constructor de Exception,
        // que guarda el texto en la propiedad Message.
        public PasajeroException(string mensaje) : base(mensaje) { }
    }

    /// <summary>
    /// Se lanza cuando se intenta dar de alta (o actualizar) un pasajero
    /// con un DNI que ya pertenece a otro pasajero registrado.
    /// </summary>
    public class PasajeroExistenteException : PasajeroException
    {
        // Guardamos el DNI que causó el conflicto.
        // Es de solo lectura ({ get; }): se asigna una vez en el constructor
        // y nadie puede modificarlo después.
        public string Dni { get; }

        public PasajeroExistenteException(string dni)
            : base($"Ya existe un pasajero registrado con el DNI {dni}.")
        {
            Dni = dni;
        }
    }

    /// <summary>
    /// Se lanza cuando se busca un pasajero por ID y no existe.
    /// (La usamos en Actualizar del servicio.)
    /// </summary>
    public class PasajeroNoEncontradoException : PasajeroException
    {
        public int Id { get; }

        public PasajeroNoEncontradoException(int id)
            : base($"No existe un pasajero con el ID {id}.")
        {
            Id = id;
        }
    }
}
