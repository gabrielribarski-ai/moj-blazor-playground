namespace IzracunInvalidnostiBlazor;


public class Segment
{
    public string SegmentId { get; set; } = string.Empty;

    public string Ime { get; set; } = string.Empty;

    // Hierarhična pot, npr. "TELO->ROKA->PRST"
    public string Pot { get; set; } = string.Empty;

    // L/D/oboje – za stranskost telesa
    public string Stranskost { get; set; } = string.Empty;

    // Neposredni nadrejeni segment (če uporabljaš drevesno gradnjo)
    public string NadsegmentId { get; set; }

    // Povezani atributi (npr. gibljivost, brazgotina, dolžina)
    public List<Atribut> Atributi { get; set; } = new();

    // Za gradnjo drevesa v aplikaciji
    public List<Segment> Children { get; set; } = new();

    public bool ImaOcenjevalneAtribute { get; set; }

    public bool OcenaPotrjena { get; set; }


}
