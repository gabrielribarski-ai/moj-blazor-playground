using System.ComponentModel;

namespace IzracunInvalidnostiBlazor.Models
{
    public enum GlobalnaFaza
    {
        Ocenjevanje,          // še zbiramo ocene po segmentih
        OcenjevanjePotrjeno,  // uporabnik je potrdil zaključek celotnega ocenjevanja
        OcenjevanjeZakljuceno            // poročilo generirano / zaključeno
    }

    public enum SegmentnaFaza
    {
        NiOcene,
        VrednostiSoVnesene,
        DeficitiIzracunani,   // izračunani možni deficiti
        Zaklenjeno         // ko je globalna faza OcenjevanjePotrjeno ali OcenjevanjeZakljuceno
    }

    public enum SimetrijaEnum
    {
        LD, E
    }

    public enum TipMeritveEnum { NUM, BOOL };

    public enum StranLDE { L, D, E }
    public enum Enum
    {
        [Description("L <=> S")]
        LS, //LD
        [Description("D <=> S")]
        DS, //LD
        [Description("L <=> D")]
        LD, //LD
        [Description("D <=> L")]
        DL,//LD
        [Description("E <=> S")]
        ES //E
    }
}
