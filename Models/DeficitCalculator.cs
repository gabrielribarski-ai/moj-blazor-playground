using CustomTypeExtensions;
using IzracunInvalidnostiBlazor.Models;
using System.Text.Json;


public static class DeficitCalculator
{
    public static void IzracunajMozneDeficite(DelTelesa delTelesa, Action<string>? logAction = null)
    {
        // 1) agregati
        delTelesa.IzmerjeniDeficit = new();
        delTelesa.IzmerjeniDeficit.GibljivostSkupajL = delTelesa.Atributi.Sum(atr => atr.Ocena?.VrednostL ?? 0m);
        delTelesa.IzmerjeniDeficit.GibljivostSkupajD = delTelesa.Atributi.Sum(atr => atr.Ocena?.VrednostD ?? 0m);
        delTelesa.IzmerjeniDeficit.GibljivostSkupajE = delTelesa.Atributi.Sum(atr => atr.Ocena?.VrednostE ?? 0m);
        delTelesa.IzmerjeniDeficit.StandardSkupaj = delTelesa.Atributi.Sum(atr => atr.StandardnaVrednost ?? 0m);

        // 2) pripravi nabor kandidatov
        delTelesa.MozniDeficitSeznam ??= new List<MozniDeficit>();
        delTelesa.MozniDeficitSeznam.Clear();

        // 3) vse stopnje
        var vseStopnje = delTelesa.Atributi
            .SelectMany(a => a.Stopnje)
            .GroupBy(s => s.StopnjaNum)
            .Select(g => g.First())
            .ToList();

        // 4) glede na simetrijo
        if (delTelesa.SimetrijaTelesa == SimetrijaEnum.LD)
        {
            var pari = new[]
            {
                (IzracunInvalidnostiBlazor.Models.Enum.LS, delTelesa.IzmerjeniDeficit.GibljivostSkupajL, delTelesa.IzmerjeniDeficit.StandardSkupaj, StranLDE.L),
                (IzracunInvalidnostiBlazor.Models.Enum.DS, delTelesa.IzmerjeniDeficit.GibljivostSkupajD, delTelesa.IzmerjeniDeficit.StandardSkupaj, StranLDE.D),
                (IzracunInvalidnostiBlazor.Models.Enum.LD, delTelesa.IzmerjeniDeficit.GibljivostSkupajL, delTelesa.IzmerjeniDeficit.GibljivostSkupajD, StranLDE.L),
                (IzracunInvalidnostiBlazor.Models.Enum.DL, delTelesa.IzmerjeniDeficit.GibljivostSkupajD, delTelesa.IzmerjeniDeficit.GibljivostSkupajL, StranLDE.D)
            };

            foreach (var (tip, dejanska, referenca, stran) in pari)
                AddIfValid(delTelesa, tip, dejanska, referenca, stran, vseStopnje);
        }

        // 5) izloči duplikate
        delTelesa.MozniDeficitSeznam = delTelesa.MozniDeficitSeznam
                .GroupBy(d => new { d.StranLDE, Percent = d.IzracunaniOdstotek ?? -1m, d.MoznaPrimerjava })
                .Select(g => g.First())
                .OrderBy(d => d.StranLDE)
                .ThenBy(d => d.IzracunaniOdstotek)
                .ToList();

        // 6) če je za posamezno stran samo en možen deficit, ga izberi
        foreach (var group in delTelesa.MozniDeficitSeznam.GroupBy(d => d.StranLDE))
        {
            if (group.Count() == 1)
            {
                group.First().JeIzbran = true;
            }
        }

        delTelesa.SegmentnaFaza = SegmentnaFaza.DeficitiIzracunani;

        if (logAction != null)
        {
            var json = JsonSerializer.Serialize(delTelesa);
            logAction($"DeficitCalculator -> Segment {json}");
        }
    }


    private static void AddIfValid(DelTelesa segment, IzracunInvalidnostiBlazor.Models.Enum tip, decimal dejanska, decimal referenca, StranLDE stran, List<StopnjaDeficita> vseStopnje)
    {
        if (referenca == 0m) return;

        var deficit = CalcDeficit(dejanska, referenca);
        if (deficit >= 100m) return;

        var odstotek = IzracunajInvalidnost(deficit, vseStopnje);
        if (!odstotek.HasValue || odstotek.Value == 0m) return;

        segment.MozniDeficitSeznam.Add(new MozniDeficit
        {
            MoznaPrimerjava = tip,
            Deficit = deficit,
            StranLDE = stran,
            IzbiraOpis = $"{tip.GetDescription()}: {odstotek.Value} %",
            IzracunaniOdstotek = odstotek.Value
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

