namespace IzracunInvalidnostiBlazor.Models
{
    public class OcenaDeficita
    {
        public decimal DelezIzgube { get; set; }
        public string Stopnja { get; set; } = "";
        public decimal MaxProcent { get; set; }
    }

    public static class StopnjaEvaluator
    {
        public static OcenaDeficita Izracunaj(
            decimal izmerjeno,
            decimal referenca,
            List<StopnjaDeficita> pasovi
        )
        {
            if (referenca <= 0)
                throw new ArgumentException("Referenca > 0", nameof(referenca));

            var delež = 1m - (izmerjeno / referenca);

            var pas = pasovi
                .Where(p => p.Operator == "<=" && delež <= p.ObmocjeNum)
                .OrderBy(p => p.ZapSt)
                .FirstOrDefault();

            return new OcenaDeficita
            {
                DelezIzgube = Math.Round(delež * 100m, 1),
                Stopnja = pas?.Stopnja ?? "neopredeljeno",
                MaxProcent = pas?.MaxProcent ?? 0m
            };
        }
    }

}
