using CheckInApi.Modelos;
using CheckInApi.Modelos.Exceptions;

namespace CheckInApi.Repositorios
{
//CAPA DATOS/PERSISTENCIA 

//Peticion Controller-PasajeroService-IPasajeroRepository-Esta seccion-txt
public class PasajeroRepositoryArchivo : IPasajeroRepository
{
    //RUTA completa del archivo .txt definida por Program.cs
    private readonly string path;
    private static readonly object candado = new object();


    public PasajeroRepositoryArchivo(string path)
    {
        this.path = path;
    }
    //Devuelve todos los pasajeros guardados y la lista vacia si no hay archivo
    public List<Pasajero> ObtenerTodos()
    {
        return Ejecutar(() => LeerArchivo());
    }
    //Se lee todo y se filtra en memoria
    public Pasajero? ObtenerPorId(int Id)
    {
        return Ejecutar(() => LeerArchivo().FirstOrDefault(p => p.Id == Id));
    }
    //Service para detectar DNI duplicado antes de un alta
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

            //StreamWriter escribe texto en un archivo

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
                return false; //no hay nada que actualizar
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
            //RemoveAll devuelve cuantos elementos quito
            int eliminados = pasajeros.RemoveAll(p => p.Id == id);
            if (eliminados == 0)
            {
                return false; //no hay nada que eliminar
            }
            ReescribirArchivo(pasajeros);
            return true;
        });
    }

    //METODOS PRIVADOS ("Motor" de persistencia)
    private List<Pasajero> LeerArchivo()
    {
        List<Pasajero> pasajeros = new List<Pasajero>();

        //Si el archivo todavia no existe (primera ejecucion) no es error, simplemente no hay nada guardado
        if (!File.Exists(path))
        {
            return pasajeros;
        }

        using (StreamReader lector = new StreamReader(path))
        {
            string? linea;
            int numeroLinea = 0;

            // ReadLine devuelve null cuando se termino el archivo.
            while ((linea = lector.ReadLine()) != null)
            {
                numeroLinea++;

                if (string.IsNullOrWhiteSpace(linea))
                {
                    continue;   // ignora lineas vacias
                }

                try
                {
                    pasajeros.Add(Pasajero.FromString(linea));
                }
                catch (Exception ex)
                {
                    // Si FromString falla es porque la linea no respeta el formato
                    // (archivo editado a mano o danado). NO la ignoramos en silencio:
                    // si siguieramos con una lista incompleta, un Actualizar/Eliminar
                    // posterior reescribiria el archivo SIN esos pasajeros y se
                    // perderian para siempre. Avisamos con el numero de linea.
                    throw new ArchivoException(
                        $"La línea {numeroLinea} de {Path.GetFileName(path)} está dañada y no se pudo leer: \"{linea}\"", ex);
                }
            }
        }
        return pasajeros;
    }
         
    // REESCRITURA SEGURA: escribe la lista completa en un archivo TEMPORAL y
    // recién cuando terminó bien reemplaza al original.
    // Si usáramos new StreamWriter(path, false) directamente, el archivo se vacía
    // ANTES de escribir; un error a mitad de camino dejaría los datos perdidos.
    private void ReescribirArchivo(List<Pasajero> pasajeros)
    {
        CrearCarpetaSiNoExiste();

        string temporal = path + ".tmp";

        // "false" = sobrescribir (no agregar al final).
        using (StreamWriter escritor = new StreamWriter(temporal, false))
        {
            foreach (Pasajero p in pasajeros)
            {
                escritor.WriteLine(p.ToString());
            }
        }

        // Reemplaza el archivo original por el temporal (overwrite: true).
        File.Move(temporal, path, true);
    }

    // Crea la carpeta del archivo (por ejemplo "Data") si todavía no existe.
    // Sin esto, el primer Agregar fallaría con DirectoryNotFoundException.
    private void CrearCarpetaSiNoExiste()
    {
        string? carpeta = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(carpeta))
        {
            Directory.CreateDirectory(carpeta);
        }
    }

    // ENVOLTORIO de TODAS las operaciones públicas. Hace dos cosas:
    //   1) lock: un solo pedido toca el archivo a la vez.
    //   2) traduce los errores de disco de .NET a nuestra ArchivoException.
    // "Func<T>" significa "una operación que devuelve un valor de tipo T":
    // le pasamos lo que queremos hacer y Ejecutar lo corre protegido.
    // El orden de los catch importa: FileNotFoundException y
    // DirectoryNotFoundException son casos particulares de IOException,
    // así que van PRIMERO (si no, IOException los atraparía a todos).
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
    








       