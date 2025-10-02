//file: OcenjevalniModelLoader.cs
using CustomTypeExtensions;
using IzracunInvalidnostiBlazor.Models;  // Atribut, StopnjaDeficita
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace IzracunInvalidnostiBlazor.Services
{
    public class OcenjevalniModelDBLoader
    {
        private readonly IConfiguration _config;

        private readonly string connStr;
        public bool IsPogojiSeznamLoaded { get; private set; }

        public OcenjevalniModel OcenjevalniModel { get; set; }

        // public OcenjevalniModel ocenjevalniModel { get; set; }

        public OcenjevalniModelDBLoader() {
            OcenjevalniModel = new OcenjevalniModel();
        }

        public OcenjevalniModelDBLoader(IConfiguration config)
        {
            _config = config;
            this.connStr = _config.GetConnectionString("APL_INVALIDNOST");
            OcenjevalniModel = new OcenjevalniModel();
            //ocenjevalniModel = new OcenjevalniModel();
        }

        public async Task LoadPogojSeznamFromDB()
        {
            if (IsPogojiSeznamLoaded) return; // 👈 prepreči dvojni query
            OcenjevalniModel.PogojSeznam = new List<Pogoj>();

            using (var conn = new OracleConnection(this.connStr))
            {
                await conn.OpenAsync();
                const string sql = "SELECT ID, SIFRA, OPIS FROM B1_POGOJI";
                await using var cmd = new OracleCommand(sql, conn);
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    OcenjevalniModel.PogojSeznam.Add(new Pogoj
                    {
                        PogojId = reader["ID"].ToString(),
                        Sifra = reader["SIFRA"].ToString(),
                        Opis = reader["OPIS"].ToString()
                    });
                }
            }
            IsPogojiSeznamLoaded = true; // 👈 označi kot naloženo
        }


        public async Task LoadSegmentSeznamFromDB()
        {
            OcenjevalniModel.SegmentSeznam = new List<Segment>();
            using (var conn = new OracleConnection(this.connStr))
            {
                conn.Open();
                string q = "";
                q += "SELECT * FROM VW_B1_SEGMENT";

                using (OracleCommand cmd = new OracleCommand(q, conn))
                {
                    DataTable dt = new DataTable();
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader, LoadOption.PreserveChanges);
                    }
                    foreach (DataRow dr in dt.Rows)
                    {
                        OcenjevalniModel.SegmentSeznam.Add(new Segment
                        {
                            SegmentId = dr["SEGMENT_ID"].ToString(),
                            Opis = dr["OPIS"].ToString(),
                            SegmentTreePath = dr["Segment_Tree_Path"].ToString(),
                            NadsegmentId = (dr["NADREJENI_ID"].ToString() == "" ? null : dr["NADREJENI_ID"].ToString()),
                            //Tip = reader.GetString(3),
                            SimetrijaTelesa = dr["LDE"].ToString()=="LD" ? SimetrijaTelesaEnum.LD: SimetrijaTelesaEnum.E,
                            Atributi = new List<Atribut>(),
                           // PodSegment = new List<Segment>(),
                            ImaOcenjevalneAtribute = dr["st_kriterijev"].ToString() != "0",
                        });
                    }
                }
            }
            await PreberiAtributeDB_Sync_Segmenti();
        }


        public Atribut? FindAtributById(string atributId)
        {
            return this.OcenjevalniModel.SegmentSeznam
                .Where(s => s.ImaOcenjevalneAtribute && s.Atributi != null)
                .SelectMany(s => s.Atributi)
                .FirstOrDefault(a => a.AtributId == atributId);
        }


        public async Task PreberiStopnjeDB_Sync(string pogojId)
        {
            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();
                string q = "select * from vw_b1_atr_stopnja where pogoj_id=:pogoj_id";

                using (OracleCommand cmd = new OracleCommand(q, conn))
                {
                    cmd.Parameters.Add(new OracleParameter("pogoj_id", OracleDbType.Varchar2, pogojId, ParameterDirection.Input));

                    DataTable dt = new DataTable();
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader, LoadOption.PreserveChanges);
                    }

                    foreach (DataRow dr in dt.Rows)
                    {
                        string atribut_id = dr["atribut_id"].ToString();
                        var atr = FindAtributById(atribut_id);
                        if (atr != null)
                        {
                            StopnjaDeficita stopnja = new();
                            stopnja.PogojAtributId = dr["pogoj_atribut_id"].ToString();
                            stopnja.ZapSt = dr["zap_st"].ToInt().Value;
                            stopnja.OdstotekFR = dr["fiksni_odstotek"].ToString() == "N" ? OdstotekFR.R : OdstotekFR.F;
                            stopnja.StopnjaOpis = dr["stopnja_opis"].ToString();
                            stopnja.ObmocjeNum = dr["obmocje_num"].AsDecimal();
                            stopnja.StopnjaNum = dr["stopnja_num"].ToInt().Value;
                            stopnja.TockaOpis = dr["tocka_opis"].ToString();
                            stopnja.Operator = dr["operator_1"].ToString();
                            stopnja.Operator = dr["operator_1"].ToString();
                            atr.Stopnje.Add(stopnja);
                        }
                    }
                }
            }
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

                    foreach (Segment segment in this.OcenjevalniModel.SegmentSeznam)
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
                                    StandardnaVrednost = dr["STANDARD"]?.AsDecimal(),
                                    TipMeritve = dr["TIP_MERITVE"].ToString()=="NUM"?TipMeritveEnum.NUM: TipMeritveEnum.BOOL,
                                    Enota = dr["ENOTA"].ToString(),
                                };
                                await PreberiStopnjeDB_Sync(atr.AtributId);
                                segment.Atributi.Add(atr);
                            }
                        }
                    }
                }
            }

        }




    }
}
