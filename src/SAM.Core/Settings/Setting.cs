using System;
using DevExpress.Mvvm;
using DevExpress.Mvvm.CodeGenerators;
using Newtonsoft.Json;

namespace SAM.Core.Settings;

[GenerateViewModel(ImplementIDataErrorInfo = true)]
public partial class Setting<T> : ViewModelBase, ISetting<T>
    where T : IComparable
{
    [GenerateProperty] private string _name;
    [GenerateProperty] private string _toolTip;
    [GenerateProperty] private T _value;
    [GenerateProperty] private T _previousValue;
    [GenerateProperty] private T _default;
    [GenerateProperty] private bool _isModified;
    [GenerateProperty] private bool _isReadOnly;
    [GenerateProperty] private bool _allowEdit;
    [GenerateProperty] private EditorType _editorType;

    public Setting(string name, T defaultValue)
    {
        Name = name;
        Default = defaultValue;
    }
        
    [GenerateCommand]
    public void CommitChange()
    {
        PreviousValue = Value;
    }
        
    [GenerateCommand]
    public void Reset()
    {
        Value = PreviousValue;
    }

    public void RestoreDefault(bool showModified = false)
    {
        Value = Default;

        if (!showModified)
        {
            PreviousValue = Value;
        }
    }

    protected void OnValueChanged()
    {
        if (ReferenceEquals(Value, null))
        {
            IsModified = ReferenceEquals(PreviousValue, null);
            return;
        }

        IsModified = Value.Equals(PreviousValue);
    }
}
