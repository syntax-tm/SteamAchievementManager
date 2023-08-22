using System.ComponentModel;

namespace SAM.WPF.Core.Settings
{
    public interface ISetting<T> : INotifyPropertyChanged
    {
        string Name { get; }
        string ToolTip { get; set; }
        T Value { get; set; }
        T PreviousValue { get; }
        T Default { get; }
        bool IsModified { get; }
        bool IsReadOnly { get; set; }
        bool AllowEdit { get; set; }
        
        EditorType EditorType { get; set; }

        void CommitChange();
        void RestoreDefault(bool showModified);
        void Reset();
    }
}
