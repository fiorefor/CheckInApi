using CheckInApi.Modelos;
using CheckInApi.Modelos.Exceptions;

namespace CheckInApi.Repositorios
{
public class PasajeroRepositoryArchivo : IPasajeroRepository
{
    private readonly string path;
    private static readonly object candado = new object();

    public PasajeroRepositoryArchivo(string path)
    {
        this.path = path;
    }

    public List<Pasajero> ObtenerTodos()
    {
        return Ejecutar(() => LeerArchivo());
    }

    public Pasajero? ObtenerPorId(int Id)
    {
        return Ejecutar(() => LeerArchivo().FirstOrDefault(p => p.Id == Id));
    }

    public Pasajero? ObtenerPorDocumento(string documento)
    {
        return Ejecutar(() => LeerArchivo().FirstOrDefault(p => p.Documento == documento));
    }

    public bool Agregar(Pasajero pasajero)
    {
        ArgumentNullException.ThrowIfNull(pasajero);
        return Ejecutar(() =>
        {
            List<Pasajero> existentes = LeerArchivo();
            pasajero.Id = existentes.Count == 0 ? 1 : existentes.Max(p => p.Id) + 1;

            CrearCarpetaSiNoExiste();

            using (StreamWriter escritor = new StreamWriter(path, true))
            {
                escritor.WriteLine(pasajero.ToString());
            }
            return true;
        });
    }

    public bool Actualizar(Pasajero pasajero)
    {
        ArgumentNullException.ThrowIfNull(pasajero);
        return Ejecutar(() =>
        {
            List<Pasajero> pasajeros = LeerArchivo();
            int posicion = pasajeros.FindIndex(p => p.Id == pasajero.Id);
            if (posicion < 0)
            {
                return false;
            }
            pasajeros[posicion] = pasajero;
            ReescribirArchivo(pasajeros);
            return true;
        });
    }

    public bool Eliminar(int id)
    {
        return Ejecutar(() =>
        {
            List<Pasajero> pasajeros = LeerArchivo();
            int eliminados = pasajeros.RemoveAll(p => p.Id == id);
            if (eliminados == 0)
            {
                return false;
            }
            ReescribirArchivo(pasajeros);
            return true;
        });
    }

    private List<Pasajero> LeerArchivo()
    {
        List<Pasajero> pasajeros = new List<Pasajero>();

        if (!File.Exists(path))
        {
            return pasajeros;
        }

        using (StreamReader lector = new StreamReader(path))
        {
            string? linea;
            int numeroLinea = 0;

            while ((linea = lector.ReadLine()) != null)
            {
                numeroLinea++;

                if (string.IsNullOrWhiteSpace(linea))
                {
                    continue;
                }

                try
                {
                    pasajeros.Add(Pasajero.FromString(linea));
                }
                catch (Exception ex)
                {
                    // No se ignora la línea: si se siguiera con una lista incompleta,
                    // una reescritura posterior perdería esos pasajeros.
                    throw new ArchivoException(
                        $"La línea {numeroLinea} de {Path.GetFileName(path)} está dañada y no se pudo leer: \"{linea}\"", ex);
                }
            }
        }
        return pasajeros;
    }

    // Escribe en un archivo temporal y recién ahí reemplaza al original,
    // para no dejarlo vacío si falla la escritura.
    private void ReescribirArchivo(List<Pasajero> pasajeros)
    {
        CrearCarpetaSiNoExiste();

        string temporal = path + ".tmp";

        using (StreamWriter escritor = new StreamWriter(temporal, false))
        {
            foreach (Pasajero p in pasajeros)
            {
                escritor.WriteLine(p.ToString());
            }
        }

        File.Move(temporal, path, true);
    }

    private void CrearCarpetaSiNoExiste()
    {
        string? carpeta = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(carpeta))
        {
            Directory.CreateDirectory(carpeta);
        }
    }

    // Serializa el acceso al archivo y traduce los errores de disco
    // a ArchivoException.
    private T Ejecutar<T>(Func<T> operacion)
    {
        lock (candado)
        {
            try
            {
                return operacion();
            }
            catch (FileNotFoundException ex)
            {
                throw new ArchivoException("No se encontró el archivo de datos.", ex);
            }
            catch (DirectoryNotFoundException ex)
            {
                throw new ArchivoException("No se encontró la carpeta del archivo de datos.", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new ArchivoException("No hay permisos para acceder al archivo de datos.", ex);
            }
            catch (IOException ex)
            {
                throw new ArchivoException("Error de lectura o escritura en el archivo de datos.", ex);
            }
        }
    }
}
}
