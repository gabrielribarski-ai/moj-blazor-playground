using IzracunInvalidnostiBlazor.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace IzracunInvalidnostiBlazor
{
    public class AtributOcena
    {
        // obstoječe
        public decimal? VrednostL { get; set; }
        public decimal? VrednostD { get; set; }

        public decimal? VrednostE { get; set; }

        public bool VrednostL_Bool { get; set; }
        public bool VrednostD_Bool { get; set; }

        public bool VrednostE_Bool { get; set; }

        public string Opis { get; set; } = string.Empty;
        public TipMeritveEnum TipMeritve { get; set; } = TipMeritveEnum.NUM;
        public StranLDE StranLDE { get; set; } 
        public string Enota { get; set; } = string.Empty;


        // Izračun: izračunani deficit in izbrana stopnja
        public decimal? Deficit { get; private set; }
        public string IzbranaStopnja { get; private set; } = string.Empty;

        /*
        // Metoda, ki po vnosu vrednosti izračuna Deficit
        public void IzracunajDeficitLevega()
        {
            if (VrednostLeva is null || StandardnaVrednost is null) return;
            var rezultat = StopnjaEvaluator.Izracunaj(
                izmerjeno: VrednostLeva.Value,
                referenca: StandardnaVrednost.Value,
                pasovi: Stopnje
            );
            Deficit = rezultat.MaxProcent;
            IzbranaStopnja = rezultat.Stopnja;
        }

        public void IzracunajDeficitDesnega()
        {
            if (VrednostDesna is null || StandardnaVrednost is null) return;
            var rezultat = StopnjaEvaluator.Izracunaj(
                izmerjeno: VrednostDesna.Value,
                referenca: StandardnaVrednost.Value,
                pasovi: Stopnje
            );
            Deficit = rezultat.MaxProcent;
            IzbranaStopnja = rezultat.Stopnja;
        }
        */
    }
}
