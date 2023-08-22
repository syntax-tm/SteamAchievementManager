using System.ComponentModel;

namespace SAM.WPF.Core
{
    public enum GameInfoType
    {
        [Description("normal")]
        Normal,
        [Description("demo")]
        Demo,
        [Description("mod")]
        Mod,
        [Description("junk")]
        Junk,
        [Description("tool")]
        Tool
    }
}
