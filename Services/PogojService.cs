using Oracle.ManagedDataAccess.Client;
using System.Data;


namespace IzracunInvalidnostiBlazor;
public class PogojService 
{
    private readonly IConfiguration _config;
    public List<Pogoj>? Seznam;


    public PogojService(IConfiguration config)
    {
        _config = config;
    }
    private readonly SegmentService _segmentService;

    public PogojService(IConfiguration config, SegmentService segmentService)
    {
        _config = config;
        _segmentService = segmentService;
    }

    public async Task PreberiPogoje()
    {
        Seznam = new List<Pogoj>();
        var connectionString = _config.GetConnectionString("APL_INVALIDNOST");
        using var conn = new OracleConnection(connectionString);
        conn.Open();

        using var cmd = new OracleCommand("SELECT ID, SIFRA, OPIS FROM B1_POGOJI", conn);
        using var reader = cmd.ExecuteReader();
        var dt = new DataTable();
        dt.Load(reader);

        foreach (DataRow dr in dt.Rows)
        {
            Seznam.Add(new Pogoj
            {
                PogojId = dr[0].ToString(),
                Koda = dr[1].ToString(),
                Opis = dr[2].ToString()
            });
        }

    }



}
