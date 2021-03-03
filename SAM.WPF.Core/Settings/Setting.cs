using System;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;

namespace SAM.WPF.Core.Settings
{
    [POCOViewModel(ImplementIDataErrorInfo = true)]
    public class Setting<T> : ViewModelBase, ISetting<T>
        where T : IComparable
    {
        public virtual string Name { get; }
        public virtual string ToolTip { get; set; }
        public virtual T Value { get; set; }
        public virtual T PreviousValue { get; protected set; }
        public virtual T Default { get; protected set; }
        public virtual bool IsModified { get; protected set; }
        public virtual bool IsReadOnly { get; set; }
        public virtual bool AllowEdit { get; set; }
        public virtual EditorType EditorType { get; set; }

        protected Setting(string name, T defaultValue)
        {
            Name = name;
            Default = defaultValue;
        }

        public static Setting<T> Create(string name, T defaultValue)
        {
            return ViewModelSource.Create(() => new Setting<T>(name, defaultValue));
        }

        public void CommitChange()
        {
            PreviousValue = Value;
        }
        
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
}
