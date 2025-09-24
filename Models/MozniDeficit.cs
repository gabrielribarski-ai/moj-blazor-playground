using static IzracunInvalidnostiBlazor.OcenjevalniAtribut;

namespace IzracunInvalidnostiBlazor.Models
{
    public class MozniDeficit
    {
        public MoznaPrimerjava moznePrimerjave { get; set; }
        public decimal? Deficit { get; set; }
        public decimal? Odstotek { get; set; }
        public StranLDE StranLDE { get; set; }
    }
}
