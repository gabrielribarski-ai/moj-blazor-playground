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
    atr.ID                       AS AtributId,
    atr.SEGMENT_ID               AS SegmentId,
    atr.STANDARDNA_VREDNOST      AS StandardnaVrednost,
    atr.TIP_MERITVE              AS TipMeritve,
    atr.ENOTA                    AS Enota,
    stop.ZAP_ST                  AS ZapSt,
    stop.OBMOCJE_OPERATOR_1      AS ObmocjeOperator,
    stop.OBMOCJE_NUM             AS ObmocjeNum,
    stop.MAX_PROCENT             AS MaxProcent,
    stop.STOPNJA                 AS Stopnja
FROM B1_POGOJ_ATRIBUT atr
LEFT JOIN B1_POGOJ_ATRIBUT_STOPNJA stop
    ON stop.POGOJ_ATRIBUT_ID = atr.ID
WHERE atr.POGOJ_ID = :pogojId
  AND atr.AKTIVNO = 'Y'
ORDER BY atr.SEGMENT_ID, atr.ID, stop.ZAP_ST";

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
                var aid = dr["AtributId"].ToString()!;
                if (!lookup.TryGetValue(aid, out var atr))
                {
                    // prvič vidimo ta atribut → kreiramo nov objekt
                    atr = new Atribut
                    {
                        AtributId = aid,
                        SegmentId = dr["SegmentId"].ToString()!,
                        StandardnaVrednost = dr["StandardnaVrednost"] as decimal?,
                        TipMeritve = dr["TipMeritve"].ToString()!,
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
                        ZapSt = Convert.ToInt32(dr["ZapSt"]),
                        Operator = dr["ObmocjeOperator"].ToString()!,
                        ObmocjeNum = Convert.ToDecimal(dr["ObmocjeNum"]),
                        MaxProcent = Convert.ToDecimal(dr["MaxProcent"]),
                        Stopnja = dr["Stopnja"].ToString()!
                    };
                    // privzeto predvidevamo, da si v Atributu dodal List<StopnjaDeficita> Stopnje { get; set; }
                    atr.Stopnje.Add(stopnja);
                }
            }

            return Seznam;
        }
    }
}
