using System;
using System.Collections.Generic;
using System.Text;

namespace CheckInApi.Modelos
{
    public class Equipaje
    {
        public string CodigoEtiqueta { get; set; } = string.Empty;
        public string PasajeroId { get; set; } = string.Empty;
        public double PesoKg { get; set; }
        public string TipoEquipaje { get; set; } = string.Empty;
        public decimal TarifaExceso { get; set; }
    }
}