// file: OcenjevalniModelDBLoader.cs
using CustomTypeExtensions;
using IzracunInvalidnostiBlazor.Models;  // Atribut, StopnjaDeficita
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace IzracunInvalidnostiBlazor.Services
{
    public class OcenjevalniModelDBLoader
    {
        private readonly string connStr;

        public OcenjevalniModelDBLoader(IConfiguration config)
        {
            connStr = config.GetConnectionString("APL_INVALIDNOST");
        }

        public async Task LoadPogojSeznamAsync(OcenjevalniModel model)
        {
            if (model.PogojSeznam?.Any() == true) return; // prepreči dvojni query
            model.PogojSeznam = new List<Pogoj>();

            await using var conn = new OracleConnection(connStr);
            await conn.OpenAsync();

            const string sql = "SELECT ID, SIFRA, OPIS FROM B1_POGOJI";
            await using var cmd = new OracleCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                model.PogojSeznam.Add(new Pogoj
                {
                    PogojId = reader["ID"].ToString(),
                    Sifra = reader["SIFRA"].ToString(),
                    Opis = reader["OPIS"].ToString()
                });
            }
        }

        public async Task LoadSegmentSeznamAsync(OcenjevalniModel model)
        {
            model.SegmentSeznam = new List<Segment>();

            await using var conn = new OracleConnection(connStr);
            await conn.OpenAsync();

            const string sql = "SELECT * FROM VW_B1_SEGMENT";
            await using var cmd = new OracleCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            var dt = new DataTable();
            dt.Load(reader);

            foreach (DataRow dr in dt.Rows)
            {
                model.SegmentSeznam.Add(new Segment
                {
                    SegmentId = dr["SEGMENT_ID"].ToString(),
                    Opis = dr["OPIS"].ToString(),
                    SegmentTreePath = dr["Segment_Tree_Path"].ToString(),
                    NadsegmentId = string.IsNullOrEmpty(dr["NADREJENI_ID"].ToString()) ? null : dr["NADREJENI_ID"].ToString(),
                    SimetrijaTelesa = dr["LDE"].ToString() == "LD" ? SimetrijaTelesaEnum.LD : SimetrijaTelesaEnum.E,
                    Atributi = new List<Atribut>(),
                    ImaOcenjevalneAtribute = dr["st_kriterijev"].ToString() != "0",
                });
            }

            await LoadAtributeAsync(model);
        }

        public Atribut? FindAtributById(OcenjevalniModel model, string atributId)
        {
            return model.SegmentSeznam
                .Where(s => s.ImaOcenjevalneAtribute && s.Atributi != null)
                .SelectMany(s => s.Atributi)
                .FirstOrDefault(a => a.AtributId == atributId);
        }

        public async Task LoadStopnjeAsync(OcenjevalniModel model, string pogojId)
        {
            await using var conn = new OracleConnection(connStr);
            await conn.OpenAsync();

            const string q = "select * from vw_b1_atr_stopnja where pogoj_id=:pogoj_id";
            await using var cmd = new OracleCommand(q, conn);
            cmd.Parameters.Add(new OracleParameter("pogoj_id", OracleDbType.Varchar2, pogojId, ParameterDirection.Input));

            var dt = new DataTable();
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                dt.Load(reader, LoadOption.PreserveChanges);
            }

            foreach (DataRow dr in dt.Rows)
            {
                string atribut_id = dr["atribut_id"].ToString();
                var atr = FindAtributById(model, atribut_id);
                if (atr != null)
                {
                    StopnjaDeficita stopnja = new()
                    {
                        PogojAtributId = dr["pogoj_atribut_id"].ToString(),
                        ZapSt = dr["zap_st"].ToInt().Value,
                        OdstotekFR = dr["fiksni_odstotek"].ToString() == "N" ? OdstotekFR.R : OdstotekFR.F,
                        StopnjaOpis = dr["stopnja_opis"].ToString(),
                        ObmocjeNum = dr["obmocje_num"].AsDecimal(),
                        StopnjaNum = dr["stopnja_num"].ToInt().Value,
                        TockaOpis = dr["tocka_opis"].ToString(),
                        Operator = dr["operator_1"].ToString()
                    };
                    atr.Stopnje.Add(stopnja);
                }
            }
        }

        public async Task LoadAtributeAsync(OcenjevalniModel model)
        {
            await using var conn = new OracleConnection(connStr);
            await conn.OpenAsync();

            const string q = "SELECT ID, SEGMENT_ID, STANDARD, ENOTA, TIP_MERITVE, MOZNA_PRIMERJAVA, OPIS from B1_ATRIBUTI";
            await using var cmd = new OracleCommand(q, conn);

            var dt = new DataTable();
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                dt.Load(reader, LoadOption.PreserveChanges);
            }

            foreach (Segment segment in model.SegmentSeznam)
            {
                if (segment.ImaOcenjevalneAtribute)
                {
                    var atributiRows = dt.AsEnumerable()
                        .Where(x => x["segment_id"].ToString() == segment.SegmentId);

                    segment.Atributi = new List<Atribut>();

                    foreach (DataRow dr in atributiRows)
                    {
                        Atribut atr = new()
                        {
                            AtributId = dr["ID"].ToString(),
                            Opis = dr["OPIS"].ToString(),
                            SegmentId = dr["SEGMENT_ID"].ToString(),
                            StandardnaVrednost = dr["STANDARD"]?.AsDecimal(),
                            TipMeritve = dr["TIP_MERITVE"].ToString() == "NUM" ? TipMeritveEnum.NUM : TipMeritveEnum.BOOL,
                            Enota = dr["ENOTA"].ToString(),
                        };

                        await LoadStopnjeAsync(model, atr.AtributId);
                        segment.Atributi.Add(atr);
                    }
                }
            }
        }
    }
}
