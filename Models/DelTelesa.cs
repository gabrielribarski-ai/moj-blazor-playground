/* filename: Segment.cs  */

namespace IzracunInvalidnostiBlazor.Models;
    

// L/D/oboje – za stranskost telesa
public enum SimetrijaEnum
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
    Urejanje, 
    Vpogled
}


public class DelTelesa: ICloneable
{
    public string DelTelesaId { get; set; } = string.Empty;
    public string Opis { get; set; } = string.Empty;
    public string DelTelesaTreePath { get; set; }
    // L/D/oboje – za stranskost telesa
    public SimetrijaEnum SimetrijaTelesa { get; set; } 
    // Neposredni nadrejeni segment (če uporabljaš drevesno gradnjo)
    public string NadrejeniId { get; set; }
    public decimal? KorekcijaOdstotkaE { get; set; }
    public decimal? KorekcijaOdstotkaL { get; set; }
    public decimal? KorekcijaOdstotkaD { get; set; }
    public string KomentarKorekcijaE { get; set; }
    public string KomentarKorekcijaL { get; set; }
    public string KomentarKorekcijaD { get; set; }

    public decimal? GetKorekcija(StranLDE stran) => stran switch
    {
        StranLDE.L => KorekcijaOdstotkaL,
        StranLDE.D => KorekcijaOdstotkaD,
        StranLDE.E => KorekcijaOdstotkaE,
        _ => null
    };

    public void SetKorekcija(StranLDE stran, decimal? value)
    {
        switch (stran)
        {
            case StranLDE.L: KorekcijaOdstotkaL = value; break;
            case StranLDE.D: KorekcijaOdstotkaD = value; break;
            case StranLDE.E: KorekcijaOdstotkaE = value; break;
        }
    }

    public string? GetKomentar(StranLDE stran) => stran switch
    {
        StranLDE.L => KomentarKorekcijaL,
        StranLDE.D => KomentarKorekcijaD,
        StranLDE.E => KomentarKorekcijaE,
        _ => null
    };

    public void SetKomentar(StranLDE stran, string? value)
    {
        switch (stran)
        {
            case StranLDE.L: KomentarKorekcijaL = value; break;
            case StranLDE.D: KomentarKorekcijaD = value; break;
            case StranLDE.E: KomentarKorekcijaE = value; break;
        }
    }
    // Povezani atributi (npr. gibljivost, brazgotina, dolžina)
    public List<Atribut> Atributi { get; set; }

    public IzmerjeniDeficit IzmerjeniDeficit { get; set; }


    public List<MozniDeficit> MozniDeficitSeznam { get; set; } = new();

    public decimal? SkupniOdstotek
    {
        get
        {
            var e = IzbranDeficitE?.IzracunaniOdstotek ?? 0m;
            var l = IzbranDeficitL?.IzracunaniOdstotek ?? 0m;
            var d = IzbranDeficitD?.IzracunaniOdstotek ?? 0m;

            return Math.Min(l + d + e, 100m);
        }
    }


    public FazaOcenjevanjaEnum FazaOcenjevanja { get; set; } = FazaOcenjevanjaEnum.NiOcene;

    public IEnumerable<MozniDeficit> OpcijeL =>
        MozniDeficitSeznam
            .Where(x => x.StranLDE == StranLDE.L && x.IzracunaniOdstotek.HasValue)
            .OrderBy(x => x.IzracunaniOdstotek);

    public IEnumerable<MozniDeficit> OpcijeD =>
        MozniDeficitSeznam
            .Where(x => x.StranLDE == StranLDE.D && x.IzracunaniOdstotek.HasValue)
            .OrderBy(x => x.IzracunaniOdstotek);

    public IEnumerable<MozniDeficit> OpcijeE =>
        MozniDeficitSeznam
            .Where(x => x.StranLDE == StranLDE.E && x.IzracunaniOdstotek.HasValue)
            .OrderBy(x => x.IzracunaniOdstotek);


    // Za gradnjo drevesa v aplikaciji
    //public List<Segment> PodSegment { get; set; } 

    public bool ImaOcenjevalneAtribute { get; set; }

    public MozniDeficit? IzbranDeficit(StranLDE stran)
    {       
        var a= MozniDeficitSeznam
            .FirstOrDefault(x => x.StranLDE == stran && x.JeIzbran && x.IzracunaniOdstotek.HasValue);
        return a;
    }

    public MozniDeficit? IzbranDeficitL =>
       IzbranDeficit(StranLDE.L);


    public MozniDeficit? IzbranDeficitD =>
       IzbranDeficit(StranLDE.D);


    public MozniDeficit? IzbranDeficitE =>
       IzbranDeficit(StranLDE.E);

    public object Clone()
    {
        // plitka kopija (shallow copy)
        return this.MemberwiseClone();
    }





    public void IzberiMozniDeficit(StranLDE stran, decimal odstotek)
    {
        ClearIzberiMozniDeficit(stran);
        var def = MozniDeficitSeznam
            .FirstOrDefault(x => x.StranLDE == stran && x.IzracunaniOdstotek == odstotek);
        if (def != null)
            def.JeIzbran = true;
    }


    public void ClearIzberiMozniDeficit(StranLDE? stran = null)
    {
        foreach (var def in MozniDeficitSeznam.Where(x => x.StranLDE == stran))
        {
            def.JeIzbran = false;
        }
    }



}
