using System;
using System.Collections.Generic;
using System.Text;

namespace SAM.WPF.Core
{
    public enum EditorType
    {
        TextBox,
        TextArea,
        CheckBox,
        RadioButtons,
        NumericUpDown,
        Color,
        Theme,
        ComboBox,
        ListBox,
        PropertyGrid,
        DataGrid
    }

    public enum TextEditorType
    {
        Default = 0,
        FileName,
        DirectoryName,
        Email,
        Url
    }
}
