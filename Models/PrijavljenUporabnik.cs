using CustomTypeExtensions;
using IzracunInvalidnostiBlazor;
using IzracunInvalidnostiBlazor.Models;
using System.ComponentModel.DataAnnotations.Schema;

public class OcenaDelTelesa
{
    public string DelTelesaId { get; set; }
    public decimal? KoncnaOcena { get; set; }
    public bool UporabljenaKorekcija { get; set; }
    public string? Komentar { get; set; }
}



/*
public enum FazaOcenjevanjaEnum
{
    NiOcene,
    Ocenjevanje,
    OcenjevanjePotrjeno,
    DeficitiIzracunani,
    DeficitiIzbrani,
    Urejanje,
    Vpogled,
    Zakljuceno
}
*/

public class PrijavljenUporabnik
{
    public string UpIme { get; set; } = string.Empty;
    public string Ime { get; set; } = string.Empty;
    public string Priimek { get; set; } = string.Empty;
    public DateTime CasPrijave { get; set; } = DateTime.Now;
    public string StDokumenta { get; set; }

    public string ImePacienta { get; set; }
    public string PriimekPacienta { get; set; }

    public Pogoj IzbranPogoj { get; set; }
    public OcenjevalniModel OcenjevalniModel { get; set; } = new();
    public GlobalnaFaza GlobalnaFaza { get; private set; } = GlobalnaFaza.Ocenjevanje;

    public decimal? KoncnaOcena { get; set; }
    public bool? UporabljenaKorekcija { get; set; }
    public string? KoncniKomentar { get; set; }

    public void ZamenjajFazo(GlobalnaFaza novaFaza)
    {
        GlobalnaFaza = novaFaza;

        if (novaFaza.In(GlobalnaFaza.OcenjevanjePotrjeno, GlobalnaFaza.OcenjevanjeZakljuceno))
        {
            // zakleni vse ocenjene dele telesa
            foreach (var ocena in OcenaSeznam)
            {
                var del = OcenjevalniModel.DelTelesaSeznam
                    .FirstOrDefault(x => x.DelTelesaId == ocena.DelTelesaId);

                if (del != null)
                {
                    del.SegmentnaFaza = SegmentnaFaza.Zaklenjeno;
                }
            }
        }
        else if (novaFaza == GlobalnaFaza.Ocenjevanje)
        {
            // če gremo nazaj v Ocenjevanje, odklene vse prej zaklenjene segmente
            foreach (var ocena in OcenaSeznam)
            {
                var del = OcenjevalniModel.DelTelesaSeznam
                    .FirstOrDefault(x => x.DelTelesaId == ocena.DelTelesaId);

                if (del != null && del.SegmentnaFaza == SegmentnaFaza.Zaklenjeno)
                {
                    del.SegmentnaFaza = SegmentnaFaza.DeficitiIzracunani;
                }
            }
        }
    }



    public List<OcenaDelTelesa> OcenaSeznam { get; set; } = new(); // seznam ocenjenih delov telesa 

    public async Task SetIzbranPogoj(string pogojId)
    {
        IzbranPogoj = new Pogoj();
        IzbranPogoj = OcenjevalniModel.PogojSeznam.Where(x => x.PogojId == pogojId).First();
        // najprej kaže na root element
        await OcenjevalniModel.SetTrenutniDelTelesa(GetRoot().DelTelesaId);
    }

    /*
    public async Task VpogledVOcenjeniDelTelesa(string ocenjeniDelTelesaId)
    {
        // preveri, ali imamo oceno za ta ID
        var ocena = OcenaSeznam.FirstOrDefault(x => x.DelTelesaId == ocenjeniDelTelesaId);
        if (ocena == null) return;

        // poišči pravi DelTelesa v modelu
        var del = OcenjevalniModel.DelTelesaSeznam
            .FirstOrDefault(x => x.DelTelesaId == ocenjeniDelTelesaId);

        if (del != null)
        {
            OcenjevalniModel.TrenutniDelTelesa = del;
        }
    }
    */

    public async Task VpogledVOcenjeniDelTelesa(string ocenjeniDelTelesaId)
    {
        var del = OcenjevalniModel.DelTelesaSeznam
            .FirstOrDefault(x => x.DelTelesaId == ocenjeniDelTelesaId);

        if (del != null)
            OcenjevalniModel.TrenutniDelTelesa = del;
    }



    public decimal? SkupnaOcena
    {
        get
        {
            if (OcenaSeznam == null || OcenaSeznam.Count == 0)
                return null;

            var vsota = OcenaSeznam
                .Where(o => o.KoncnaOcena.HasValue)
                .Sum(o => o.KoncnaOcena.Value);

            return Math.Min(vsota, 100m);
        }
    }

    public void OdstraniOceno(string delTelesaId)
    {
        var ocena = OcenaSeznam.FirstOrDefault(x => x.DelTelesaId == delTelesaId);
        if (ocena != null)
        {
            OcenaSeznam.Remove(ocena);
            // opcijsko: tudi v modelu resetiraj fazo segmenta
            var del = OcenjevalniModel.DelTelesaSeznam
                .FirstOrDefault(x => x.DelTelesaId == delTelesaId);
            if (del != null && del.SegmentnaFaza == SegmentnaFaza.Zaklenjeno)
            {
                del.SegmentnaFaza = SegmentnaFaza.NiOcene;
            }
        }
    }

    public void DodajMedOcenjene(DelTelesa delTelesa)
    {
        var existing = OcenaSeznam.FirstOrDefault(s => s.DelTelesaId == delTelesa.DelTelesaId);
        if (existing != null) OcenaSeznam.Remove(existing);

        OcenaSeznam.Add(new OcenaDelTelesa
        {
            DelTelesaId = delTelesa.DelTelesaId,
            KoncnaOcena = delTelesa.SkupniOdstotek,  
            UporabljenaKorekcija = delTelesa.KorekcijaOdstotkaL.HasValue
                                || delTelesa.KorekcijaOdstotkaD.HasValue
                                || delTelesa.KorekcijaOdstotkaE.HasValue,
            Komentar = delTelesa.GetKomentar(StranLDE.L)
                    ?? delTelesa.GetKomentar(StranLDE.D)
                    ?? delTelesa.GetKomentar(StranLDE.E)
        });
    }





    public DelTelesa GetRoot()
    {
        var s = this.OcenjevalniModel.DelTelesaSeznam
            .Where(s => s.NadrejeniId == null)
            .First();
        return s;
    }
}
