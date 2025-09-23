using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace IzracunInvalidnostiBlazor.Models
{
    public class OcenjevalniModel
    {
        public List<Pogoj> PogojSeznam { get;  set; }
        public  Pogoj IzbranPogoj{ get; set; }
        public List<Segment> SegmentSeznam { get; set; }

        public Segment trenutniSegment { get; set; }

        public string kjeSeNahajamo { get; set; }


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
                    .Where(x => x.NadsegmentId==trenutniSegment?.SegmentId)  
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

        public List<Segment> GetBreadcrumbPath(string segmentId)
        {
            var path = new List<Segment>();
            var current = SegmentSeznam?.FirstOrDefault(s => s.SegmentId == segmentId);

            while (current != null)
            {
                path.Insert(0, current);
                current = GetParentSegment(current.SegmentId);
            }

            return path;
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


        public List<Segment> GetSegmentsByStranskost(string stranskost)
        {
            return SegmentSeznam
                .Where(s => s.Stranskost.Equals(stranskost, StringComparison.OrdinalIgnoreCase)
                         || s.Stranskost.Equals("oboje", StringComparison.OrdinalIgnoreCase))
                .ToList();
        }


    }




}
