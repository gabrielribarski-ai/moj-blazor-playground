/* filename: Segment.cs  */

namespace IzracunInvalidnostiBlazor.Models;
    

// L/D/oboje – za stranskost telesa
public enum SimetrijaDelaTelesa
{
    LD, E
}


public class Segment
{
    public string SegmentId { get; set; } = string.Empty;

    public string Opis { get; set; } = string.Empty;

    public string SegmentTreePath { get; set; }
    // L/D/oboje – za stranskost telesa
    public SimetrijaDelaTelesa SimetrijaDelaTelesa { get; set; } 

    // Neposredni nadrejeni segment (če uporabljaš drevesno gradnjo)
    public string NadsegmentId { get; set; }

    // Povezani atributi (npr. gibljivost, brazgotina, dolžina)
    public List<Atribut> Atributi { get; set; }

    public IzmerjeniDeficit IzmerjeniDeficit { get; set; }

    public List<MozniDeficit> MozniDeficitNabor { get; set; }

    // Za gradnjo drevesa v aplikaciji
    public List<Segment> PodSegment { get; set; } 

    public bool ImaOcenjevalneAtribute { get; set; }

     public bool OcenaPotrjena { get; set; }

    public MozniDeficit FindMozniDeficit(StranLDE stran, decimal odstotek)
    {
        return MozniDeficitNabor.Where(x => x.IzracunaniOdstotek == odstotek).FirstOrDefault();
    }

    public void IzberiMozniDeficit(StranLDE stran, decimal odstotek)
    {
        var a= MozniDeficitNabor.Where(x => x.IzracunaniOdstotek == odstotek).FirstOrDefault();
        if (a != null)
        {
            a.JeIzbran = true;
        }
    }

    public void ClearIzberiMozniDeficit()
    {
     foreach(var def in MozniDeficitNabor)
        {
            def.JeIzbran = false;
        }

    }

    public IEnumerable<MozniDeficit> OpcijeL =>
        MozniDeficitNabor?
            .Where(x => x.StranLDE == StranLDE.L && x.IzracunaniOdstotek.HasValue)
            .OrderBy(x => x.IzracunaniOdstotek)
        ?? Enumerable.Empty<MozniDeficit>();

    public IEnumerable<MozniDeficit> OpcijeD =>
        MozniDeficitNabor?
            .Where(x => x.StranLDE == StranLDE.D && x.IzracunaniOdstotek.HasValue)
            .OrderBy(x => x.IzracunaniOdstotek)
        ?? Enumerable.Empty<MozniDeficit>();

    public IEnumerable<MozniDeficit> OpcijeE =>
        MozniDeficitNabor?
            .Where(x => x.StranLDE == StranLDE.E && x.IzracunaniOdstotek.HasValue)
            .OrderBy(x => x.IzracunaniOdstotek)
        ?? Enumerable.Empty<MozniDeficit>();

}
