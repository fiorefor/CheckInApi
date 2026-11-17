// DTO (Data Transfer Object): es la "forma" del JSON que llega en el body
// de un POST/PUT. Todas las propiedades son nullable (string?) porque el
// cliente podría no enviarlas; el servicio se encarga de validarlas.
//
// Ejemplo de JSON:
// {
//   "nombre": "Ada", "apellido": "Lovelace", "documento": "12.345.678",
//   "email": "ada@mail.com", "telefono": "+54 11 5555-1234",
//   "pasajeroFrecuente": "1234567890"
// }
public class PasajeroRequest
{
    public string? Nombre { get; set; }
    public string? Apellido { get; set; }
    public string? Documento { get; set; }   // DNI
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? PasajeroFrecuente { get; set; }
}
