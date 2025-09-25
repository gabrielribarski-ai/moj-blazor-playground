namespace IzracunInvalidnostiBlazor.Models
{
    public enum OdstotekFR
    { F,R}

    public class StopnjaDeficita
    {
        public string TockaOpis { get; set; }
        public string PogojAtributId { get; set; }
        public OdstotekFR OdstotekFR { get; set; }
        public int ZapSt { get; set; }
        public string Operator { get; set; } = "<=";
        public decimal? ObmocjeNum { get; set; }
        public decimal StopnjaNum { get; set; }
        public string StopnjaOpis { get; set; } = "";

    }

}
