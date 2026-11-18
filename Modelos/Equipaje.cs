using System;
using System.Collections.Generic;
using System.Text;

namespace CheckInApi.Modelos
{
    public class Equipaje
    {
        public string CodigoEtiqueta { get; set; }
        public string PasajeroId { get; set; }
        public double PesoKg { get; set; }
        public string TipoEquipaje { get; set; }
        public decimal TarifaExceso { get; set; }
    }
}