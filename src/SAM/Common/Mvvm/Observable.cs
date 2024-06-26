﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SAM.Mvvm
{
    public abstract class Observable : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            Debug.Assert(!string.IsNullOrEmpty(propertyName));

            if (EqualityComparer<T>.Default.Equals(field,value))
                return false;

            field = value;

            RaisePropertyChanged(propertyName);

            return true;
        }

        public void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new (propertyName));
        }
    }
}
