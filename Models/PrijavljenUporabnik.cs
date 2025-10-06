using IzracunInvalidnostiBlazor;
using IzracunInvalidnostiBlazor.Models;

public class PrijavljenUporabnik
{
    public string UpIme { get; set; } = string.Empty;
    public string Ime { get; set; } = string.Empty;
    public string Priimek { get; set; } = string.Empty;
    public DateTime CasPrijave { get; set; } = DateTime.Now;
    public string StDokumenta { get; set; }
    public Pogoj IzbranPogoj { get; set; }
    public OcenjevalniModel OcenjevalniModel { get; set; } = new();
    public List<DelTelesa> OcenaSeznam { get; set; }

    public async Task SetIzbranPogoj(string pogojId)
    {
        IzbranPogoj = new Pogoj();
        IzbranPogoj = OcenjevalniModel.PogojSeznam.Where(x => x.PogojId == pogojId).First();
        // najprej kaže na root element
        await OcenjevalniModel.SetTrenutniDelTelesa(GetRoot().DelTelesaId);
    }

    public async Task VpogledVOcenjeniSegment(string ocenjeniSegmentId)
    {
        this.OcenjevalniModel.TrenutniDelTelesa = OcenaSeznam.Where(x => x.DelTelesaId == ocenjeniSegmentId).First();
    }

    public decimal? SkupnaOcena
    {
        get
        {
            if (OcenaSeznam == null || OcenaSeznam.Count == 0)
                return null;

            var vsota = OcenaSeznam
                .SelectMany(seg => seg.MozniDeficitSeznam.Where(d => d.JeIzbran))
                .Sum(d => d.IzracunaniOdstotek ?? 0m);

            return Math.Min(vsota, 100m);
        }
    }

    public void DodajMedOcenjene(DelTelesa delTelesa)
    {
        if (OcenaSeznam == null)
            OcenaSeznam = new List<DelTelesa>();
        var DelTelesaClone = (DelTelesa)delTelesa.Clone();
        if (!OcenaSeznam.Any(s => s.DelTelesaId == DelTelesaClone.DelTelesaId))
            OcenaSeznam.Add(DelTelesaClone);
    }

    public DelTelesa GetRoot()
    {
        var s = this.OcenjevalniModel.DelTelesaSeznam
            .Where(s => s.NadrejeniId == null)
            .First();
        return s;
    }
}
