using CustomTypeExtensions;
using IzracunInvalidnostiBlazor.Models;  // Atribut, StopnjaDeficita
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace IzracunInvalidnostiBlazor.Services
{
    public class OcenjevalniModelLoader: OcenjevalniModel
    {
        private readonly IConfiguration _config;

        private readonly string connStr;

       // public OcenjevalniModel ocenjevalniModel { get; set; }

        public OcenjevalniModelLoader(IConfiguration config)
        {
            _config = config;
            this.connStr = _config.GetConnectionString("APL_INVALIDNOST");
            //ocenjevalniModel = new OcenjevalniModel();
        }


        public async Task LoadPogojSeznamFromDB()
        {
            PogojSeznam = new List<Pogoj>();
            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();

                using var cmd = new OracleCommand("SELECT ID, SIFRA, OPIS FROM B1_POGOJI", conn);
                using var reader = cmd.ExecuteReader();
                var dt = new DataTable();
                dt.Load(reader);

                foreach (DataRow dr in dt.Rows)
                {
                    PogojSeznam.Add(new Pogoj
                    {
                        PogojId = dr[0].ToString(),
                        Sifra = dr[1].ToString(),
                        Opis = dr[2].ToString()
                    });
                }
            }
        }


        public async Task IzbranPogojSet(string pogojId)
        {
            IzbranPogoj = new Pogoj();
            IzbranPogoj = PogojSeznam.Where(x => x.PogojId == pogojId).First();
            // ko je izbran pogoj gremo naložit še ostale podatke
            LoadSegmentSeznamFromDB();
            // najprej kaže na root element
            //ocenjevalniModel.trenutniSegment = GetRootSegment();
            TrenutniSegmentSet(GetRootSegment().SegmentId);
        }

        public  async Task TrenutniSegmentSet(string trenutniSegmentId)
        {
            this.trenutniSegment = SegmentSeznam.Where(x => x.SegmentId == trenutniSegmentId).First();

            //this.ocenjevalniModel.trenutniSegment = ocenjevalniModel.SegmentSeznam.Where(x => x.SegmentId == trenutniSegmentId).First();
        }

        /*
        public async Task IzbranPogojSet(string pogojSifra)
        {
            ocenjevalniModel.IzbranPogoj = new Pogoj();
            ocenjevalniModel.IzbranPogoj = ocenjevalniModel.PogojSeznam.Where(x => x.Sifra == pogojSifra).First();
            // ko je izbran pogoj gremo naložit še ostale podatke
            LoadSegmentSeznamFromDB();
        }
        */

        public async Task LoadSegmentSeznamFromDB()
        {
            SegmentSeznam = new List<Segment>();
            using (var conn = new OracleConnection(this.connStr))
            {
                conn.Open();
            string q = "";
                q += "SELECT seg.ID AS SEGMENT_ID, seg.OPIS, seg.NADREJENI_ID, seg.TIP, seg.LDE,";
                q += " (select count(atr.ID) from b1_atributi atr   where atr.segment_id = seg.ID    ) as st_kriterijev";
                q += " FROM B1_SEGMENTI seg";

                using (OracleCommand cmd = new OracleCommand(q, conn))
                {
                    DataTable dt = new DataTable();
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader, LoadOption.PreserveChanges);
                    }
                    foreach (DataRow dr in dt.Rows)
                    {
                        SegmentSeznam.Add(new Segment
                        {
                            SegmentId = dr["SEGMENT_ID"].ToString(),
                            Opis = dr["OPIS"].ToString(),
                            NadsegmentId = (dr["NADREJENI_ID"].ToString() == "" ? null : dr["NADREJENI_ID"].ToString()),
                            //Tip = reader.GetString(3),
                            Stranskost = dr["LDE"].ToString(),
                            Atributi = new List<Atribut>(),
                            PodSegment = new List<Segment>(),
                            ImaOcenjevalneAtribute = dr["st_kriterijev"].ToString() != "0",
                        });
                    }
                }
            }
            await PreberiAtributeDB_Sync_Segmenti();
        }


        public async Task PreberiAtributeDB_Sync_Segmenti()
        {
            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();
                string q = "SELECT ID, SEGMENT_ID, STANDARD, ENOTA, TIP_MERITVE, MOZNA_PRIMERJAVA, OPIS from B1_ATRIBUTI ";

                using (OracleCommand cmd = new OracleCommand(q, conn))
                {
                    DataTable dt = new DataTable();
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader, LoadOption.PreserveChanges);
                    }

                    foreach (Segment segment in this.SegmentSeznam)
                    {
                        if (segment.ImaOcenjevalneAtribute)
                        {
                            IEnumerable<DataRow> atributiRows = dt.AsEnumerable().Select(x => x).Where(x => x["segment_id"].ToString() == segment.SegmentId);
                            segment.Atributi = new();
                            foreach (DataRow dr in atributiRows)
                            {
                                Atribut atr = new Atribut()
                                {
                                    AtributId = dr["ID"].ToString(),
                                    Opis = dr["OPIS"].ToString(),
                                    SegmentId = dr["SEGMENT_ID"].ToString(),
                                    StandardnaVrednost = dr["STANDARD"]?.ToDecimal(),
                                    TipMeritve = dr["TIP_MERITVE"].ToString(),
                                    Enota = dr["ENOTA"].ToString(),
                                };
                                segment.Atributi.Add(atr);
                            }
                        }
                    }
                }
            }

        }
        /*

        public  List<Segment> SegmentiZaPrikaz()
        {
           var s=
              SegmentSeznam
                ?.Where(s => s.NadsegmentId == trenutniSegment?.SegmentId)
                .ToList();
            return s;
        }
        */



    }
}
