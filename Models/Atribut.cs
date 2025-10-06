using IzracunInvalidnostiBlazor.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace IzracunInvalidnostiBlazor
{


    public class Atribut
    {

        public decimal? StandardnaVrednost { get; set; }

        public AtributOcena Ocena { get; set; } = new AtributOcena();   

        public List<StopnjaDeficita> Stopnje { get; set; } = new();

        public string AtributId { get; set; }
        public string DelTelesaId { get; set; }
        public string Opis { get; set; } = string.Empty;
        public  TipMeritveEnum TipMeritve { get; set; } = TipMeritveEnum.NUM;
        public string Enota { get; set; } = string.Empty;

    }
}

