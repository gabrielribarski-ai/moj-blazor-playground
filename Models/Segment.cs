/* filename: Segment.cs  */

namespace IzracunInvalidnostiBlazor.Models;
    

// L/D/oboje – za stranskost telesa
public enum SimetrijaTelesaEnum
{
    LD, E
}

public enum FazaOcenjevanjaEnum
{
    NiOcene,
    Ocenjevanje,
    OceneSoVnesene,
    DeficitiIzracunani,
    DeficitiIzbrani,
    Urejanje, Vpogled
}


public class Segment: ICloneable
{
    public string SegmentId { get; set; } = string.Empty;

    public string Opis { get; set; } = string.Empty;

    public string SegmentTreePath { get; set; }
    // L/D/oboje – za stranskost telesa
    public SimetrijaTelesaEnum SimetrijaTelesa { get; set; } 

    // Neposredni nadrejeni segment (če uporabljaš drevesno gradnjo)
    public string NadsegmentId { get; set; }

    // Povezani atributi (npr. gibljivost, brazgotina, dolžina)
    public List<Atribut> Atributi { get; set; }

    public IzmerjeniDeficit IzmerjeniDeficit { get; set; }


    public List<MozniDeficit> MozniDeficitNabor { get; set; } = new();

    public decimal? SkupniOdstotekSegmenta
    {
        get
        {
            if (MozniDeficitNabor == null || MozniDeficitNabor.Count == 0)
                return null;

            var izbrani = MozniDeficitNabor.Where(d => d.JeIzbran).ToList();
            if (izbrani.Count == 0)
                return null;

            // če je E izbran, vzameš samo njega, sicer sešteješ L+D
            var e = izbrani.FirstOrDefault(d => d.StranLDE == StranLDE.E)?.IzracunaniOdstotek;
            if (e.HasValue)
                return Math.Min(e.Value, 100m);

            var l = izbrani.FirstOrDefault(d => d.StranLDE == StranLDE.L)?.IzracunaniOdstotek ?? 0m;
            var d = izbrani.FirstOrDefault(d => d.StranLDE == StranLDE.D)?.IzracunaniOdstotek ?? 0m;

            return Math.Min(l + d, 100m);
        }
    }


    public FazaOcenjevanjaEnum FazaOcenjevanja { get; set; } = FazaOcenjevanjaEnum.NiOcene;

    public IEnumerable<MozniDeficit> OpcijeL =>
        MozniDeficitNabor
            .Where(x => x.StranLDE == StranLDE.L && x.IzracunaniOdstotek.HasValue)
            .OrderBy(x => x.IzracunaniOdstotek);

    public IEnumerable<MozniDeficit> OpcijeD =>
        MozniDeficitNabor
            .Where(x => x.StranLDE == StranLDE.D && x.IzracunaniOdstotek.HasValue)
            .OrderBy(x => x.IzracunaniOdstotek);

    public IEnumerable<MozniDeficit> OpcijeE =>
        MozniDeficitNabor
            .Where(x => x.StranLDE == StranLDE.E && x.IzracunaniOdstotek.HasValue)
            .OrderBy(x => x.IzracunaniOdstotek);


    // Za gradnjo drevesa v aplikaciji
    //public List<Segment> PodSegment { get; set; } 

    public bool ImaOcenjevalneAtribute { get; set; }

    public MozniDeficit FindMozniDeficit(StranLDE stran, decimal odstotek)
    {
        return MozniDeficitNabor.Where(x => x?.IzracunaniOdstotek == odstotek).FirstOrDefault();
    }

    public MozniDeficit? IzbranDeficit(StranLDE stran)
    {       
        var a= MozniDeficitNabor
            .FirstOrDefault(x => x.StranLDE == stran && x.JeIzbran);
        return a;   
    }

    public object Clone()
    {
        // plitka kopija (shallow copy)
        return this.MemberwiseClone();
    }
    
    public void IzberiMozniDeficit(StranLDE stran, decimal odstotek)
    {
        var a= MozniDeficitNabor.Where(x => x.IzracunaniOdstotek == odstotek && x.StranLDE==stran).FirstOrDefault();
        if (a != null)
        {
            a.JeIzbran = true;
        }
    }

    public void ClearIzberiMozniDeficit(StranLDE? stran = null)
    {
        foreach (var def in MozniDeficitNabor.Where(x=>x.StranLDE==stran))
        {
            def.JeIzbran = false;
        }
    }



}
