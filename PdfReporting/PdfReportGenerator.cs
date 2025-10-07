using System.IO;
using System.Linq;
using IzracunInvalidnostiBlazor;
using IzracunInvalidnostiBlazor.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public static class PdfReportHelper
{

    public class ReportStatus
    {
        public bool ReportCompleted { get; set; }
        public byte[]? FileBytes { get; set; }
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
    }

    public record ColumnDef(
    string Header,
    Func<Atribut, string> GetMeritev,
    Func<DelTelesa, string> GetKoncni,
    Func<DelTelesa, string> GetKorigiran,
    Func<DelTelesa, string> GetPrimerjava,
    Func<DelTelesa, string> GetKomentar
);

    private static List<ColumnDef> GetColumns(DelTelesa del)
    {
        var cols = new List<ColumnDef>();

        if (del.SimetrijaTelesa == SimetrijaEnum.LD)
        {
            cols.Add(new ColumnDef(
                "L",
                atr => atr.Ocena?.VrednostL?.ToString() ?? "-",
                d => d.IzbranDeficitL?.IzracunaniOdstotek?.ToString() ?? "-",
                d => d.KorekcijaOdstotkaL?.ToString() ?? "-",
                d => GetPrimerjava(d, StranLDE.L),
                d => d.GetKomentar(StranLDE.L) ?? ""
            ));

            cols.Add(new ColumnDef(
                "D",
                atr => atr.Ocena?.VrednostD?.ToString() ?? "-",
                d => d.IzbranDeficitD?.IzracunaniOdstotek?.ToString() ?? "-",
                d => d.KorekcijaOdstotkaD?.ToString() ?? "-",
                d => GetPrimerjava(d, StranLDE.D),
                d => d.GetKomentar(StranLDE.D) ?? ""
            ));
        }
        else if (del.SimetrijaTelesa == SimetrijaEnum.E)
        {
            cols.Add(new ColumnDef(
                "E",
                atr => atr.Ocena?.VrednostE?.ToString() ?? "-",
                d => d.IzbranDeficitE?.IzracunaniOdstotek?.ToString() ?? "-",
                d => d.KorekcijaOdstotkaE?.ToString() ?? "-",
                d => GetPrimerjava(d, StranLDE.E),
                d => d.GetKomentar(StranLDE.E) ?? ""
            ));
        }

        // Standard je lahko vedno zraven
        cols.Add(new ColumnDef(
            "Standard",
            atr => atr.StandardnaVrednost?.ToString() ?? "-",
            d => "", // nima smisla pri končnem %
            d => "", // nima smisla pri korigiranem %
            d => "-", // nima smisla pri primerjavi
            d => ""   // nima smisla pri komentarju
        ));

        return cols;
    }



    public static ReportStatus GenerateReport(PrijavljenUporabnik user)
    {


        var status = new ReportStatus
        {
            ReportCompleted = false,
            FileName = "izvid.pdf",
            ContentType = "application/pdf"
        };

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);

                // Header
                page.Header().Column(col =>
                {
                    col.Item().Text($"Izvid za: {user.Ime} {user.Priimek}")
                        .FontSize(18).Bold().AlignCenter();
                    col.Item().Text($"Št. dokumenta: {user.StDokumenta}")
                        .FontSize(12).AlignCenter();
                });

                page.Content().Column(col =>
                {
                    foreach (var ocena in user.OcenaSeznam)
                    {
                        var del = user.OcenjevalniModel.DelTelesaSeznam
                            .FirstOrDefault(x => x.DelTelesaId == ocena.DelTelesaId);
                        if (del == null) continue;

                        var simetrija = user.OcenjevalniModel.DelTelesaSeznam.Select(x => x.SimetrijaTelesa).FirstOrDefault();

                        // Hierarhija
                        col.Item().Text(del.DelTelesaTreePath).Bold().FontSize(14);

                        var columns = GetColumns(del);

                        col.Item().Table(table =>
                        {
                            // Definicija stolpcev
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(3); // opis
                                foreach (var _ in columns)
                                    c.RelativeColumn(1);
                            });

                            // Header
                            table.Header(h =>
                            {
                                h.Cell().Text("Opis meritve").Bold();
                                foreach (var colDef in columns)
                                    h.Cell().Text(colDef.Header).Bold();
                            });

                            // Meritve
                            foreach (var atr in del.Atributi)
                            {
                                table.Cell().Text(atr.Opis);
                                foreach (var colDef in columns)
                                    table.Cell().Text(colDef.GetMeritev(atr));
                            }

                            // Končni %
                            table.Cell().Text("Končni %").Bold();
                            foreach (var colDef in columns)
                                table.Cell().Text(colDef.GetKoncni(del));

                            // Korigiran %
                            table.Cell().Text("Korigiran %").Bold();
                            foreach (var colDef in columns)
                                table.Cell().Text(colDef.GetKorigiran(del));

                            // Način primerjave
                            table.Cell().Text("Način primerjave").Bold();
                            foreach (var colDef in columns)
                                table.Cell().Text(colDef.GetPrimerjava(del));

                            // Komentar
                            table.Cell().Text("Komentar").Bold();
                            foreach (var colDef in columns)
                                table.Cell().Text(colDef.GetKomentar(del));
                        });


                        // Povzetek
                        col.Item().Text($"Končna ocena (povzetek): {ocena.KoncnaOcena?.ToString() ?? "-"} %");
                        col.Item().Text($"Uporabljena korekcija: {(ocena.UporabljenaKorekcija ? "DA" : "NE")}");
                        if (!string.IsNullOrWhiteSpace(ocena.Komentar))
                            col.Item().Text($"Komentar (povzetek): {ocena.Komentar}");

                        col.Item().LineHorizontal(1);
                    }

                    // Skupna ocena pacienta
                    col.Item().Text($"Skupna ocena pacienta: {user.SkupnaOcena?.ToString() ?? "-"} %")
                        .FontSize(16).Bold();
                });
            });
        });

        using var ms = new MemoryStream();
        document.GeneratePdf(ms);
        status.FileBytes = ms.ToArray();
        status.ReportCompleted = true;

        return status;
    }

    private static string GetPrimerjava(DelTelesa del, StranLDE stran)
    {
        if (del.Atributi.Any(a => a.StandardnaVrednost.HasValue))
            return "standard";

        if (del.SimetrijaTelesa == SimetrijaEnum.LD)
            return "nasprotna stran";

        if (stran == StranLDE.E)
            return "brez primerjave";

        return "-";
    }
}
