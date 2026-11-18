namespace CheckInApi.Modelos.Exceptions
{
    // Error propio de la capa de persistencia.
    public class ArchivoException : Exception
    {
        public ArchivoException(string mensaje) : base(mensaje) { }

        public ArchivoException(string mensaje, Exception inner) : base(mensaje, inner) { }
    }
}
