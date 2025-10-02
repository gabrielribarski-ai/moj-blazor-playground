using CustomTypeExtensions;
using IzracunInvalidnostiBlazor.Models;
using System.Text.Json;


    public static class DeficitCalculator
    {
        public static void IzracunajMozneDeficite(Segment segment, Action<string>? logAction = null)
        {
            // 1) agregati
            segment.IzmerjeniDeficit = new();
            segment.IzmerjeniDeficit.GibljivostSkupajL = segment.Atributi.Sum(atr => atr.Ocena?.VrednostL ?? 0m);
            segment.IzmerjeniDeficit.GibljivostSkupajD = segment.Atributi.Sum(atr => atr.Ocena?.VrednostD ?? 0m);
            segment.IzmerjeniDeficit.GibljivostSkupajE = segment.Atributi.Sum(atr => atr.Ocena?.VrednostE ?? 0m);
            segment.IzmerjeniDeficit.StandardSkupaj = segment.Atributi.Sum(atr => atr.StandardnaVrednost ?? 0m);

            // 2) pripravi nabor kandidatov
            segment.MozniDeficitNabor ??= new List<MozniDeficit>();
            segment.MozniDeficitNabor.Clear();

            // 3) vse stopnje
            var vseStopnje = segment.Atributi.SelectMany(a => a.Stopnje).ToList();

            // 4) glede na simetrijo
            if (segment.SimetrijaTelesa == SimetrijaTelesaEnum.LD)
            {
            AddIfValid(segment, IzracunInvalidnostiBlazor.Models.Enum.LS, segment.IzmerjeniDeficit.GibljivostSkupajL, segment.IzmerjeniDeficit.StandardSkupaj, StranLDE.L, vseStopnje);
            AddIfValid(segment, IzracunInvalidnostiBlazor.Models.Enum.DS, segment.IzmerjeniDeficit.GibljivostSkupajD, segment.IzmerjeniDeficit.StandardSkupaj, StranLDE.D, vseStopnje);
            AddIfValid(segment, IzracunInvalidnostiBlazor.Models.Enum.LD, segment.IzmerjeniDeficit.GibljivostSkupajL, segment.IzmerjeniDeficit.GibljivostSkupajD, StranLDE.L, vseStopnje);
            AddIfValid(segment, IzracunInvalidnostiBlazor.Models.Enum.DL, segment.IzmerjeniDeficit.GibljivostSkupajD, segment.IzmerjeniDeficit.GibljivostSkupajL, StranLDE.D, vseStopnje);
            }
            else if (segment.SimetrijaTelesa == SimetrijaTelesaEnum.E)
            {
            AddIfValid(segment, IzracunInvalidnostiBlazor.Models.Enum.ES, segment.IzmerjeniDeficit.GibljivostSkupajE, segment.IzmerjeniDeficit.StandardSkupaj, StranLDE.E, vseStopnje);
            }

            // 5) izloči duplikate
            segment.MozniDeficitNabor = segment.MozniDeficitNabor
                .GroupBy(d => new { d.StranLDE, Percent = d.IzracunaniOdstotek ?? -1m, d.MoznaPrimerjava })
                .Select(g => g.First())
                .OrderBy(d => d.StranLDE)
                .ThenBy(d => d.IzracunaniOdstotek)
                .ToList();

            segment.FazaOcenjevanja = FazaOcenjevanjaEnum.DeficitiIzracunani;

            if (logAction != null)
            {
                var json = JsonSerializer.Serialize(segment);
                logAction($"DeficitCalculator -> Segment {json}");
            }
        }

        private static void AddIfValid(Segment segment, IzracunInvalidnostiBlazor.Models.Enum tip, decimal dejanska, decimal referenca, StranLDE stran, List<StopnjaDeficita> vseStopnje)
        {
            if (referenca == 0m) return;

            var deficit = CalcDeficit(dejanska, referenca);
            if (deficit >= 100m) return;

            var odstotek = IzracunajInvalidnost(deficit, vseStopnje);

            segment.MozniDeficitNabor.Add(new MozniDeficit
            {
                MoznaPrimerjava = tip,
                Deficit = deficit,
                StranLDE = stran,
                IzbiraOpis = $"{tip.GetDescription()}: {odstotek}",
                IzracunaniOdstotek = odstotek
            });
        }

        private static decimal CalcDeficit(decimal dejanska, decimal referenca)
        {
            if (referenca == 0m) return 0m;
            var razlika = Math.Abs(referenca - dejanska);
            return Math.Round((razlika / referenca) * 100m, 1);
        }

        private static decimal? IzracunajInvalidnost(decimal deficitPercent, List<StopnjaDeficita> stopnje)
        {
            if (stopnje == null || stopnje.Count == 0) return null;

            var coef = deficitPercent / 100m;

            var relativne = stopnje
                .Where(s => s.OdstotekFR == OdstotekFR.R && s.ObmocjeNum.HasValue)
                .OrderBy(s => s.ObmocjeNum.Value)
                .ToArray();

            if (relativne.Length > 0)
            {
                if (coef <= relativne[0].ObmocjeNum) return relativne[0].StopnjaNum;

                var nextIndex = Array.FindIndex(relativne, s => coef <= s.ObmocjeNum);
                if (nextIndex >= 0)
                {
                    var prev = relativne[nextIndex - 1];
                    var next = relativne[nextIndex];
                    var span = next.ObmocjeNum.Value - prev.ObmocjeNum.Value;
                    var t = span == 0 ? 1 : (coef - prev.ObmocjeNum.Value) / span;
                    return Math.Round(prev.StopnjaNum + t * (next.StopnjaNum - prev.StopnjaNum), 1);
                }

                return relativne[^1].StopnjaNum;
            }

            var fiksne = stopnje
                .Where(s => s.OdstotekFR == OdstotekFR.F)
                .OrderBy(s => s.ZapSt)
                .Select(s => s.StopnjaNum)
                .ToArray();

            if (fiksne.Length == 0) return null;
            if (coef <= 0) return 0;

            var idx = Math.Min((int)(coef * fiksne.Length), fiksne.Length - 1);
            return fiksne[idx];
        }
    }

