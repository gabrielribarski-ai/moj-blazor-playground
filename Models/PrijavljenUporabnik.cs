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

    public List<Segment> OcenjenSegmentSeznam { get; set; }


    public async Task SetIzbranPogoj(string pogojId)
    {
        IzbranPogoj = new Pogoj();
        IzbranPogoj = OcenjevalniModel.PogojSeznam.Where(x => x.PogojId == pogojId).First();
        // najprej kaže na root element
        await OcenjevalniModel.SetTrenutniSegment(GetRootSegment().SegmentId);
    }

    public async Task VpogledVOcenjeniSegment(string ocenjeniSegmentId)
    {
        this.OcenjevalniModel.TrenutniSegment = OcenjenSegmentSeznam.Where(x => x.SegmentId == ocenjeniSegmentId).First();
    }


    public decimal? SkupnaOcena
    {
        get
        {
            if (OcenjenSegmentSeznam == null || OcenjenSegmentSeznam.Count == 0)
                return null;

            var vsota = OcenjenSegmentSeznam
                .SelectMany(seg => seg.MozniDeficitNabor.Where(d => d.JeIzbran))
                .Sum(d => d.IzracunaniOdstotek ?? 0m);

            return Math.Min(vsota, 100m);
        }
    }

    public void DodajMedOcenjeneSegmente(Segment segment)
    {
        if (OcenjenSegmentSeznam == null)
            OcenjenSegmentSeznam = new List<Segment>();
        var SegmentClone = (Segment)segment.Clone();
        if (!OcenjenSegmentSeznam.Any(s => s.SegmentId == SegmentClone.SegmentId))
            OcenjenSegmentSeznam.Add(SegmentClone);
    }

    public Segment GetRootSegment()
    {
        var s = this.OcenjevalniModel.SegmentSeznam
            .Where(s => s.NadsegmentId == null)
            .First();
        return s;
    }
}
