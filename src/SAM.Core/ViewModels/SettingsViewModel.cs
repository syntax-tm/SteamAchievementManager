using DevExpress.Mvvm.POCO;

namespace SAM.Core.ViewModels
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
