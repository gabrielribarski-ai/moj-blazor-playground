using System.ComponentModel;

namespace IzracunInvalidnostiBlazor.Models
{

    public enum MoznaPrimerjavaEnum
    {
        [Description("L <=> S")]
        L_primerjava_S, //LD
        [Description("D <=> S")]
        D_primerjava_S, //LD
        [Description("L <=> D")]
        L_primerjava_D, //LD
        [Description("D <=> L")]
        D_primerjava_L,//LD
        [Description("E <=> S")]
        E_primerjava_S //E
    }
}
