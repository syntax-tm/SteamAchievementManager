using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SAM.WPF.Core
{
    [DefaultValue(6)]
    public enum ViewColumns : int
    {
        Min = 3,
        Max = 8
    }
}
