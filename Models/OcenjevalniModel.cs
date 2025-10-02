using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace IzracunInvalidnostiBlazor.Models
{
    public class OcenjevalniModel
    {
        public List<Pogoj> PogojSeznam { get; set; }

        // kateri pogoj je izbran
        public Pogoj IzbranPogoj { get; set; }
        //seznam vseh segmentov v modelu    
        public List<Segment> SegmentSeznam { get; set; }
        //trenutni segment na katerem smo oz. ga prikazujemo
        public Segment trenutniSegment { get; set; }

        // Ko ocenimo segment/del telesa, ga dodamo v ta seznam
        public List<Segment> OcenjenSegmentSeznam { get; set; }

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


        public List<Atribut> AtributiZaPrikaz
        {
            get
            {
                var filtrirani = trenutniSegment?.Atributi?
                    .Where(x => x.SegmentId == trenutniSegment.SegmentId)
                    .ToList();

                return (filtrirani == null || filtrirani.Count == 0) ? null : filtrirani;
            }
        }

        public List<Segment> SegmentiZaPrikaz
        {
            get
            {
                if (SegmentSeznam == null)
                    return null;
                return SegmentSeznam
                    .Where(x => x.NadsegmentId == trenutniSegment?.SegmentId)
                    .ToList();
            }
        }



        public List<Segment> GetSegmentChildren(string segmentId)
        {
            return SegmentSeznam
                .Where(s => s.NadsegmentId == segmentId)
                .ToList();
        }

        public Segment? GetParentSegment(string segmentId)
        {
            var current = SegmentSeznam.FirstOrDefault(s => s.SegmentId == segmentId);
            if (current?.NadsegmentId == null) return null;

            return SegmentSeznam.FirstOrDefault(s => s.SegmentId == current.NadsegmentId);
        }

        public List<Segment> GetParentsChildren(string segmentId)
        {
            var parent = GetParentSegment(segmentId);
            if (parent == null) return new();

            return GetSegmentChildren(parent.SegmentId);
        }

public List<Segment> BreadcrumbPath
{
    get
    {
        var path = new List<Segment>();
        var current = trenutniSegment;

        while (current != null)
        {
            path.Insert(0, current);
            current = GetParentSegment(current.SegmentId);
        }

        return path;
    }
}



        public async Task IzbranPogojSet(string pogojId)
        {
            IzbranPogoj = new Pogoj();
            IzbranPogoj = PogojSeznam.Where(x => x.PogojId == pogojId).First();
            // najprej kaže na root element
            TrenutniSegmentSet(GetRootSegment().SegmentId);
        }

        public async Task TrenutniSegmentSet(string trenutniSegmentId)
        {
            this.trenutniSegment = SegmentSeznam.Where(x => x.SegmentId == trenutniSegmentId).First();

            //this.ocenjevalniModel.trenutniSegment = ocenjevalniModel.SegmentSeznam.Where(x => x.SegmentId == trenutniSegmentId).First();
        }

        public async Task VpogledVOcenjeniSegment(string ocenjeniSegmentId)
        {
            this.trenutniSegment = OcenjenSegmentSeznam.Where(x => x.SegmentId == ocenjeniSegmentId).First();

            //this.ocenjevalniModel.trenutniSegment = ocenjevalniModel.SegmentSeznam.Where(x => x.SegmentId == trenutniSegmentId).First();
        }


        public Segment? FindSegmentByName(string ime)
        {
            return SegmentSeznam.FirstOrDefault(s => s.Opis.Equals(ime, StringComparison.OrdinalIgnoreCase));
        }


        public List<Segment> GetRootSegments()
        {
            return SegmentSeznam
                .Where(s => s.NadsegmentId == null)
                .ToList();
        }

        public Segment GetRootSegment()
        {
            var s = SegmentSeznam
                .Where(s => s.NadsegmentId == null)
                .First();
            return s;
        }

        public List<Segment> GetRootChildren()
        {
            var root = GetRootSegment();
            return SegmentSeznam
                .Where(s => s.NadsegmentId == root.SegmentId)
                .ToList();
        }


        public void DodajMedOcenjeneSegmente(Segment segment)
        {
            if (OcenjenSegmentSeznam == null)
                OcenjenSegmentSeznam = new List<Segment>();
            var SegmentClone = (Segment)segment.Clone();
            if (!OcenjenSegmentSeznam.Any(s => s.SegmentId == SegmentClone.SegmentId))  
                OcenjenSegmentSeznam.Add(SegmentClone);
        }
    }


}
