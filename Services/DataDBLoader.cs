// file: OcenjevalniModelDBLoader.cs
using CustomTypeExtensions;
using IzracunInvalidnostiBlazor.Models;  // Atribut, StopnjaDeficita
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Data;

namespace IzracunInvalidnostiBlazor.Services
{
    public class DataDBLoader
    {
        private readonly string connStr;

        public DataDBLoader(IConfiguration config)
        {
            connStr = config.GetConnectionString("APL_INVALIDNOST");
        }

        public DataDBLoader()
        {
        }

        public async Task<List<Pogoj>> LoadPogojSeznamAsync()
        {
            List <Pogoj> result=new ();
            //if (model.PogojSeznam?.Any() == true) return; // prepreči dvojni query
            //model.PogojSeznam = new List<Pogoj>();

            await using var conn = new OracleConnection(connStr);
            await conn.OpenAsync();

            const string sql = "SELECT ID, SIFRA, OPIS FROM B1_POGOJI";
            await using var cmd = new OracleCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new Pogoj
                {
                    PogojId = reader["ID"].ToString(),
                    Sifra = reader["SIFRA"].ToString(),
                    Opis = reader["OPIS"].ToString()
                });
            }
            return result;
        }

        public async Task<List<Models.DelTelesa>> LoadSegmentSeznamAsync( )
        {
            var result = new List<Models.DelTelesa>();

            await using var conn = new OracleConnection(connStr);
            await conn.OpenAsync();

            const string sql = "SELECT * FROM VW_B1_DEL_TELESA";
            await using var cmd = new OracleCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            var dt = new DataTable();
            dt.Load(reader);

            foreach (DataRow dr in dt.Rows)
            {
                result.Add(new Models.DelTelesa
                {
                    DelTelesaId = dr["DEL_TELESA_ID"].ToString(),
                    Opis = dr["OPIS"].ToString(),
                    DelTelesaTreePath = dr["DEL_TELESA_Tree_Path"].ToString(),
                    NadrejeniId = string.IsNullOrEmpty(dr["NADREJENI_ID"].ToString()) ? null : dr["NADREJENI_ID"].ToString(),
                    SimetrijaTelesa = dr["LDE"].ToString() == "LD" ? SimetrijaEnum.LD : SimetrijaEnum.E,
                    Atributi = new List<Atribut>(),
                    ImaOcenjevalneAtribute = dr["st_kriterijev"].ToString() != "0",
                });
            }

           return result;
        }

        public async Task PreberiStopnjDBAsync(OcenjevalniModel model, string pogojId)
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
                        var atr = FindAtributById(model,  atribut_id);
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

        public Atribut? FindAtributById(OcenjevalniModel model, string atributId)
        {
            return model.DelTelesaSeznam
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

        public async Task PreberiInPoveziAtributeDBAsync(OcenjevalniModel model)
        {
            await using var conn = new OracleConnection(connStr);
            await conn.OpenAsync();

            const string q = "SELECT ID, DEL_TELESA_ID, STANDARD, ENOTA, TIP_MERITVE, MOZNA_PRIMERJAVA, OPIS from B1_ATRIBUTI";
            await using var cmd = new OracleCommand(q, conn);

            var dt = new DataTable();
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                dt.Load(reader, LoadOption.PreserveChanges);
            }

            foreach (Models.DelTelesa segment in model.DelTelesaSeznam)
            {
                if (segment.ImaOcenjevalneAtribute)
                {
                    var atributiRows = dt.AsEnumerable()
                        .Where(x => x["del_telesa_id"].ToString() == segment.DelTelesaId);

                    segment.Atributi = new List<Atribut>();

                    foreach (DataRow dr in atributiRows)
                    {
                        Atribut atr = new()
                        {
                            AtributId = dr["ID"].ToString(),
                            Opis = dr["OPIS"].ToString(),
                            DelTelesaId = dr["DEL_TELESA_ID"].ToString(),
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
