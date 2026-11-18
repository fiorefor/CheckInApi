// Datos que llegan en el body de POST /api/pasajeros y PUT /api/pasajeros/{id}.
public class PasajeroRequest
{
    public string? Nombre { get; set; }
    public string? Apellido { get; set; }
    public string? Documento { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? PasajeroFrecuente { get; set; }
}
