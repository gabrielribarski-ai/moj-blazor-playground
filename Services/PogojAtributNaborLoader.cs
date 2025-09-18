using System.Data;
using Oracle.ManagedDataAccess.Client;
using IzracunInvalidnostiBlazor.Models;  // Atribut, StopnjaDeficita
using Microsoft.Extensions.Configuration;

namespace IzracunInvalidnostiBlazor.Services
{
    public class PogojAtributNaborLoader
    {
        private readonly IConfiguration _config;
        public List<Atribut>? Seznam { get; private set; }

        public PogojAtributNaborLoader(IConfiguration config)
        {
            _config = config;
        }

        public async Task<List<Atribut>> LoadForPogojAsync(int pogojId)
        {
            Seznam = new List<Atribut>();

            using var conn = new OracleConnection(_config.GetConnectionString("APL_INVALIDNOST"));
            await conn.OpenAsync();

            const string sql = @"
SELECT
    PA.POGOJ_ID AS PogojId,
    pa.ID                       AS Atribut_Id,
    pa.SEGMENT_ID               AS Segment_Id,
    atr.STANDARD      AS Standard,
    atr.TIP_MERITVE              AS Tip_Meritve,
    atr.ENOTA                    AS Enota,
    atr.opis as ATRIBUT_OPIS,
    pas.ZAP_ST                  AS Zap_St,
    pas.OBMOCJE_OPERATOR_1      AS Obmocje_Operator_1,
    pas.OBMOCJE_NUM             AS Obmocje_Num,
    pas.STOPNJA_NUM                 AS STOPNJA_NUM
FROM B1_POGOJ_ATRIBUT pa
LEFT JOIN B1_ATRIBUTI atr ON pa.SEGMENT_ID=atr.SEGMENT_ID
LEFT JOIN B1_POGOJ_ATRIBUT_STOPNJA pas
    ON pas.POGOJ_ATRIBUT_ID = pa.ID
WHERE 
pa.POGOJ_ID = :pogojId
  --AND atr.AKTIVNO = 'Y'
ORDER BY pa.SEGMENT_ID, pa.ID, pas.ZAP_ST";

            using var cmd = new OracleCommand(sql, conn)
            {
                BindByName = true
            };
            cmd.Parameters.Add("pogojId", OracleDbType.Int32).Value = pogojId;

            var dt = new DataTable();
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                dt.Load(reader, LoadOption.PreserveChanges);
            }

            // začnemo graditi Atribut objekte
            var lookup = new Dictionary<string, Atribut>();

            foreach (DataRow dr in dt.Rows)
            {
                var aid = dr["Atribut_Id"].ToString()!;
                if (!lookup.TryGetValue(aid, out var atr))
                {
                    // prvič vidimo ta atribut → kreiramo nov objekt
                    atr = new Atribut
                    {
                        AtributId = aid,
                        Opis = dr["ATRIBUT_OPIS"].ToString(),
                        SegmentId = dr["Segment_Id"].ToString(),
                        StandardnaVrednost = dr["Standard"] as decimal?,
                        TipMeritve = dr["Tip_Meritve"].ToString()!,
                        Enota = dr["Enota"].ToString()!
                    };
                    lookup[aid] = atr;
                    Seznam.Add(atr);
                }

                // če je v tej vrsti tudi definicija stopnje, jo dodamo
                if (dr["zap_st"] != DBNull.Value)
                {
                    var stopnja = new StopnjaDeficita
                    {
                        ZapSt = Convert.ToInt32(dr["Zap_St"]),
                        Operator = dr["Obmocje_Operator_1"].ToString()!,
                        ObmocjeNum = Convert.ToDecimal(dr["Obmocje_Num"]),
                        //MaxProcent = Convert.ToDecimal(dr["MaxProcent"]),
                        Stopnja = dr["STOPNJA_NUM"].ToString()!
                    };
                    // privzeto predvidevamo, da si v Atributu dodal List<StopnjaDeficita> Stopnje { get; set; }
                    atr.Stopnje.Add(stopnja);
                }
            }

            return Seznam;
        }
    }
}
