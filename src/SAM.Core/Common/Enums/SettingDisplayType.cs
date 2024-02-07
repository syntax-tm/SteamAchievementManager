using System.ComponentModel;

namespace SAM.Core
{
	[DefaultValue(None)]
	public enum EditorType
	{
		None = 0,
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

	[DefaultValue(Default)]
	public enum TextEditorType
	{
		Default = 0,
		FileName,
		DirectoryName,
		Email,
		Url
	}
}
