using System.Collections.Generic;
using CheckInApi.Modelos;

namespace CheckInApi.Repositorios
{
    public interface IPasajeroRepository
    {
        List<Pasajero> ObtenerTodos();
        Pasajero? ObtenerPorId(int Id);
        Pasajero? ObtenerPorDocumento(string documento);
        bool Agregar(Pasajero pasajero);
        bool Actualizar(Pasajero pasajero);
        bool Eliminar(int id);
    }
}
    