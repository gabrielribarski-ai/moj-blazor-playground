using IzracunInvalidnostiBlazor.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace IzracunInvalidnostiBlazor
{
    public class AtributOcena
    {

        public int StopnjaIdx { get; set; } = -1;
        public decimal? VrednostL { get; set; }= 0;
        public decimal? VrednostD { get; set; } = 0;

        public decimal? VrednostE { get; set; } = 0;

        public bool VrednostL_Bool { get; set; } = false;
        public bool VrednostD_Bool { get; set; } = false;

        public bool VrednostE_Bool { get; set; } = false;
    }
}
