using Oracle.ManagedDataAccess.Client;
using System.Data;


namespace IzracunInvalidnostiBlazor;
public class PogojDBLoader 
{
    private readonly IConfiguration _config;
    public List<Pogoj>? Seznam;


    public PogojDBLoader(IConfiguration config)
    {
        _config = config;
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
                Sifra = dr[1].ToString(),
                Opis = dr[2].ToString()
            });
        }

    }



}
