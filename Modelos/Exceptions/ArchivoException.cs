namespace CheckInApi.Modelos.Exceptions
{
    public class ArchivoException : Exception
    {//Constructor simple. Solo un mensaje
        public ArchivoException(string mensaje) : base (mensaje) { }
        //Constructor con "inner" ademas del mensaje guarda el error original. No se pierde el detalle tecnico
        public ArchivoException (string mensaje, Exception inner): base (mensaje, inner) { }
    }
}
//EXCEPCION PROPIA DE LA CAPA PERSISTENCIA 
//con la falla del disco hay errores "tecnicos" con las capas de arriba (Service y Constroller) que no conocen estos detalles
//por ende el repo atrapa esos errores y los convierte en este unico tipo de error asi en capa de negocios solo se ve "error de persistencia"
