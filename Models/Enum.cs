using System.ComponentModel;

namespace IzracunInvalidnostiBlazor.Models
{
    public enum TipMeritveEnum { NUM, BOOL };

    public enum StranLDE { L, D, E }
    public enum Enum
    {
        [Description("L <=> S")]
        LS, //LD
        [Description("D <=> S")]
        DS, //LD
        [Description("L <=> D")]
        LD, //LD
        [Description("D <=> L")]
        DL,//LD
        [Description("E <=> S")]
        ES //E
    }
}
