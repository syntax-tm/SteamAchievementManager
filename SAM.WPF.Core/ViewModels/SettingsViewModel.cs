using System;
using System.Collections.Generic;
using System.Text;
using DevExpress.Mvvm.POCO;

namespace SAM.WPF.Core.ViewModels
{
    public class SettingsViewModel
    {

        protected SettingsViewModel()
        {

        }

        public static SettingsViewModel Create()
        {
            return ViewModelSource.Create(() => new SettingsViewModel());
        }

    }
}
