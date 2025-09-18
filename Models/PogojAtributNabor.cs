namespace IzracunInvalidnostiBlazor.Models
{
    public class PogojAtributNabor
    {
        public int Id { get; set; }
        public int SegmentId { get; set; }
        public int AtributId { get; set; }
        public string Stran { get; set; } = "E"; // L, D, E
        public decimal Standard { get; set; }
        public string TipMeritve { get; set; } = "NUM"; // NUM, TF, ORD
        public string Enota { get; set; } = "stopinje";
        public List<StopnjaDeficita> Stopnje { get; set; } = new();
    }

    public class StopnjaDeficita
    {
        public int ZapSt { get; set; }
        public string Operator { get; set; } = "<=";
        public decimal ObmocjeNum { get; set; }
        public decimal MaxProcent { get; set; }
        public string Stopnja { get; set; } = "";
    }

}
