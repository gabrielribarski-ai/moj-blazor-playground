

namespace IzracunInvalidnostiBlazor.Models
{
    public class MozniDeficit
    {
        public MoznaPrimerjavaEnum MoznaPrimerjava { get; set; }
        public decimal? Deficit { get; set; }

        public string IzbiraOpis { get; set; }
        public decimal? IzracunaniOdstotek { get; set; }
        public StranLDE StranLDE { get; set; }
        public bool JeIzbran { get; set; } = false;
    }
}
