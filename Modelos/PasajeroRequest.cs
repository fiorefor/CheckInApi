// Datos que llegan en el body de POST/PUT /api/Pasajero.
public class PasajeroRequest
{
    public string? Nombre { get; set; }
    public string? Apellido { get; set; }
    public string? Documento { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? PasajeroFrecuente { get; set; }
}
