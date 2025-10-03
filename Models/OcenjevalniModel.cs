using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace IzracunInvalidnostiBlazor.Models
{
    public class OcenjevalniModel
    {
        public List<Pogoj> PogojSeznam { get; set; }

        public List<Segment> SegmentSeznam { get; set; }
        //trenutni segment na katerem smo oz. ga prikazujemo
        public Segment TrenutniSegment { get; set; }

        // Ko ocenimo segment/del telesa, ga dodamo v ta seznam


        public List<Atribut> AtributiZaPrikaz
        {
            get
            {
                var filtrirani = TrenutniSegment?.Atributi?
                    .Where(x => x.SegmentId == TrenutniSegment.SegmentId)
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
                    .Where(x => x.NadsegmentId == TrenutniSegment?.SegmentId)
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


        public List<Segment> BreadcrumbPath
        {
            get
            {
                var path = new List<Segment>();
                var current = TrenutniSegment;

                while (current != null)
                {
                    path.Insert(0, current);
                    current = GetParentSegment(current.SegmentId);
                }

                return path;
            }
        }


        public async Task SetTrenutniSegment(string trenutniSegmentId)
        {
            this.TrenutniSegment = SegmentSeznam.Where(x => x.SegmentId == trenutniSegmentId).First();

            //this.ocenjevalniModel.trenutniSegment = ocenjevalniModel.SegmentSeznam.Where(x => x.SegmentId == trenutniSegmentId).First();
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



    }


}