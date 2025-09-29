namespace IzracunInvalidnostiBlazor.Models
{
    public class SegmentOcenaSkupaj
    {
        public decimal IzracunanSkupniOdstotek { get; set; }

        public bool ImaKorekcijo { get; set; } = false;
        public string KomentarOcenjevalca { get; set; }
        public decimal KoncniSkupniOdstotek { get; set; } // ročno korigiran s strani ocenjevalca

    }
}
