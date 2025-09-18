public class UserSessionState
{
    public Guid InstanceId { get; } = Guid.NewGuid();
    public string IzbraniPogojId { get; set; }
    public string UporabnikId { get; set; } // opcijsko, če imaš login
    public DateTime CasZacetka { get; set; } = DateTime.Now;
}