public class PasajeRequest
{
    public string? TipoVuelo { get; set; }
    public string? TipoTarifa { get; set; }
    public string? NroVuelo { get; set; }
}

/* 
    Ejemplo de JSON:
    {
    "tipoVuelo": "N", "tipoTarifa": "P", "nroVuelo": "AR1234"
    }
    */