namespace CheckInApi.Modelos.Exceptions
{
    public class PasajeroException : Exception
    {
        public PasajeroException(string mensaje) : base(mensaje) { }
    }

    public class PasajeroExistenteException : PasajeroException
    {
        public string Dni { get; }

        public PasajeroExistenteException(string dni)
            : base($"Ya existe un pasajero registrado con el DNI {dni}.")
        {
            Dni = dni;
        }
    }

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
