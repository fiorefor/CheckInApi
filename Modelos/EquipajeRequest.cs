// Datos que llegan en el body de POST/PUT /api/equipajes/{idPasajero}.
// El ID del pasajero va en la ruta y la tarifa la calcula el servidor.
public class EquipajeRequest
{
    public string? CodigoEtiqueta { get; set; }
    public string? TipoEquipaje { get; set; }
    public double PesoKg { get; set; }
}
