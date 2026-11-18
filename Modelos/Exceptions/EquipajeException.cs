using System;
using System.Collections.Generic;
using System.Text;

namespace CheckInApi.Modelos.Exceptions
{
    public class EquipajeException : Exception
    {
        public EquipajeException(string message) : base(message)
        {
        }

        public EquipajeException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
