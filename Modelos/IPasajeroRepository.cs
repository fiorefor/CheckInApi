using System.Collections.Generic;

namespace CheckInApi.Modelos
{
    public interface IPasajeRepository
    {
        List<Pasaje> ObtenerTodos();
        Pasaje? ObtenerPorId(int id);
        void Agregar(Pasaje pasaje);
        void Actualizar(Pasaje pasaje);
        void Eliminar(int id);
    }
}