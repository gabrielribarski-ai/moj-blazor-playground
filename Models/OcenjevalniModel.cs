using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace IzracunInvalidnostiBlazor.Models
{
    public class OcenjevalniModel
    {
        public List<Pogoj> PogojSeznam { get; set; }

        public List<DelTelesa> DelTelesaSeznam { get; set; }
        //trenutni segment na katerem smo oz. ga prikazujemo
        public DelTelesa TrenutniDelTelesa { get; set; }

        // Ko ocenimo segment/del telesa, ga dodamo v ta seznam

        public List<Atribut> AtributiZaPrikaz
        {
            get
            {
                var filtrirani = TrenutniDelTelesa?.Atributi?
                    .Where(x => x.DelTelesaId == TrenutniDelTelesa.DelTelesaId)
                    .ToList();

                return (filtrirani == null || filtrirani.Count == 0) ? null : filtrirani;
            }
        }

        public List<DelTelesa> DeliTelesaZaPrikazSeznam
        {
            get
            {
                if (DelTelesaSeznam == null)
                    return null;
                return DelTelesaSeznam
                    .Where(x => x.NadrejeniId == TrenutniDelTelesa?.DelTelesaId)
                    .ToList();
            }
        }


        public List<DelTelesa> GetDelTelesaChildren(string segmentId)
        {
            return DelTelesaSeznam
                .Where(s => s.NadrejeniId == segmentId)
                .ToList();
        }


        public DelTelesa? GetNadrejeniDelTelesa(string delTelesaId)
        {
            var current = DelTelesaSeznam.FirstOrDefault(s => s.DelTelesaId == delTelesaId);
            if (current?.NadrejeniId == null) return null;

            return DelTelesaSeznam.FirstOrDefault(s => s.DelTelesaId == current.NadrejeniId);
        }


        public List<DelTelesa> BreadcrumbPath
        {
            get
            {
                var path = new List<DelTelesa>();
                var current = TrenutniDelTelesa;

                while (current != null)
                {
                    path.Insert(0, current);
                    current = GetNadrejeniDelTelesa(current.DelTelesaId);
                }

                return path;
            }
        }


        public async Task SetTrenutniDelTelesa(string trenutniDelTelesaId)
        {
            this.TrenutniDelTelesa = DelTelesaSeznam.Where(x => x.DelTelesaId == trenutniDelTelesaId).First();

            //this.ocenjevalniModel.trenutniSegment = ocenjevalniModel.SegmentSeznam.Where(x => x.SegmentId == trenutniSegmentId).First();
        }




        public List<DelTelesa> GetRootDeliTelesa()
        {
            return DelTelesaSeznam
                .Where(s => s.NadrejeniId == null)
                .ToList();
        }

        public DelTelesa GetRoot()
        {
            var s = DelTelesaSeznam
                .Where(s => s.NadrejeniId == null)
                .First();
            return s;
        }



    }


}