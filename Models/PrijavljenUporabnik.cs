using IzracunInvalidnostiBlazor.Models;

public class PrijavljenUporabnik
{
    public string UpIme { get; set; } = string.Empty;
    public string Ime { get; set; } = string.Empty;
    public string Priimek { get; set; } = string.Empty;
    public DateTime CasPrijave { get; set; } = DateTime.Now;
    public string StDokumenta { get; set; } 

    public OcenjevalniModel OcenjevalniModel { get; set; } = new();
}
