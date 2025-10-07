using IzracunInvalidnostiBlazor;
using IzracunInvalidnostiBlazor.Models;

public class OcenaDelTelesa
{
    public string DelTelesaId { get; set; }
    public decimal? KoncnaOcena { get; set; }
    public bool UporabljenaKorekcija { get; set; }
    public string? Komentar { get; set; }
}

public enum FazaOcenjevanjaEnum
{
    NiOcene,
    Ocenjevanje,
    OcenjevanjePotrjeno,
    DeficitiIzracunani,
    DeficitiIzbrani,
    Urejanje,
    Vpogled,
    Zakljuceno
}


public class PrijavljenUporabnik
{
    public string UpIme { get; set; } = string.Empty;
    public string Ime { get; set; } = string.Empty;
    public string Priimek { get; set; } = string.Empty;
    public DateTime CasPrijave { get; set; } = DateTime.Now;
    public string StDokumenta { get; set; }
    public Pogoj IzbranPogoj { get; set; }
    public OcenjevalniModel OcenjevalniModel { get; set; } = new();
    public FazaOcenjevanjaEnum FazaOcenjevanja { get; set; }= FazaOcenjevanjaEnum.NiOcene;
    public decimal? KoncnaOcena { get; set; }
    public bool? UporabljenaKorekcija { get; set; }
    public string? KoncniKomentar { get; set; }

    
    public List<OcenaDelTelesa> OcenaSeznam { get; set; } = new(); // seznam ocenjenih delov telesa 

    public async Task SetIzbranPogoj(string pogojId)
    {
        IzbranPogoj = new Pogoj();
        IzbranPogoj = OcenjevalniModel.PogojSeznam.Where(x => x.PogojId == pogojId).First();
        // najprej kaže na root element
        await OcenjevalniModel.SetTrenutniDelTelesa(GetRoot().DelTelesaId);
    }

    /*
    public async Task VpogledVOcenjeniDelTelesa(string ocenjeniDelTelesaId)
    {
        // preveri, ali imamo oceno za ta ID
        var ocena = OcenaSeznam.FirstOrDefault(x => x.DelTelesaId == ocenjeniDelTelesaId);
        if (ocena == null) return;

        // poišči pravi DelTelesa v modelu
        var del = OcenjevalniModel.DelTelesaSeznam
            .FirstOrDefault(x => x.DelTelesaId == ocenjeniDelTelesaId);

        if (del != null)
        {
            OcenjevalniModel.TrenutniDelTelesa = del;
        }
    }
    */

    public async Task VpogledVOcenjeniDelTelesa(string ocenjeniDelTelesaId)
    {
        var del = OcenjevalniModel.DelTelesaSeznam
            .FirstOrDefault(x => x.DelTelesaId == ocenjeniDelTelesaId);

        if (del != null)
            OcenjevalniModel.TrenutniDelTelesa = del;
    }



    public decimal? SkupnaOcena
    {
        get
        {
            if (OcenaSeznam == null || OcenaSeznam.Count == 0)
                return null;

            var vsota = OcenaSeznam
                .Where(o => o.KoncnaOcena.HasValue)
                .Sum(o => o.KoncnaOcena.Value);

            return Math.Min(vsota, 100m);
        }
    }



    public void DodajMedOcenjene(DelTelesa delTelesa)
    {
        var existing = OcenaSeznam.FirstOrDefault(s => s.DelTelesaId == delTelesa.DelTelesaId);
        if (existing != null) OcenaSeznam.Remove(existing);

        OcenaSeznam.Add(new OcenaDelTelesa
        {
            DelTelesaId = delTelesa.DelTelesaId,
            KoncnaOcena = delTelesa.SkupniOdstotek,  
            UporabljenaKorekcija = delTelesa.KorekcijaOdstotkaL.HasValue
                                || delTelesa.KorekcijaOdstotkaD.HasValue
                                || delTelesa.KorekcijaOdstotkaE.HasValue,
            Komentar = delTelesa.GetKomentar(StranLDE.L)
                    ?? delTelesa.GetKomentar(StranLDE.D)
                    ?? delTelesa.GetKomentar(StranLDE.E)
        });
    }





    public DelTelesa GetRoot()
    {
        var s = this.OcenjevalniModel.DelTelesaSeznam
            .Where(s => s.NadrejeniId == null)
            .First();
        return s;
    }
}
