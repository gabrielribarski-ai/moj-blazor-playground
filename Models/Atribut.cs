using IzracunInvalidnostiBlazor.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace IzracunInvalidnostiBlazor
{
    public class Atribut
    {
        // obstoječe
        public decimal? VrednostLeva { get; set; }
        public decimal? VrednostDesna { get; set; }
        public decimal? StandardnaVrednost { get; set; }

        [NotMapped]
        public bool TFVrednostLevaBool
        {
            get => TFVrednostLeva ?? false;
            set => TFVrednostLeva = value;
        }

        [NotMapped]
        public bool TFVrednostDesnaBool
        {
            get => TFVrednostDesna ?? false;
            set => TFVrednostDesna = value;
        }

        public bool? TFVrednostLeva { get; set; }
        public bool? TFVrednostDesna { get; set; }

        public string AtributId { get; set; }
        public string SegmentId { get; set; }
        public string Opis { get; set; } = string.Empty;
        public string TipMeritve { get; set; } = "NUM";
        public string Enota { get; set; } = string.Empty;

        // novo: podatki iz B1_POGOJ_ATRIBUT_STOPNJA
        public List<StopnjaDeficita> Stopnje { get; set; } = new();

        // Izračun: izračunani deficit in izbrana stopnja
        public decimal? Deficit { get; private set; }
        public string IzbranaStopnja { get; private set; } = string.Empty;

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
    }
}
