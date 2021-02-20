using System.Diagnostics;
using DevExpress.Mvvm.POCO;
using log4net;
using SAM.WPF.Core;
using SAM.WPF.Core.API;

namespace SAM.WPF.ViewModels
{
    public class HomeViewModel
    {

        protected readonly ILog log = LogManager.GetLogger(nameof(HomeViewModel));
        
        public virtual SteamApp SelectedItem { get; set; }
        public virtual SteamLibrary Library { get; set; }

        protected HomeViewModel()
        {
            //Library = SteamLibrary.Create();
            //Library.Refresh(true);

            Library = SteamLibraryManager.Default.Library;
        }

        public static HomeViewModel Create()
        {
            return ViewModelSource.Create(() => new HomeViewModel());
        }

        public void Loaded()
        {

        }

        public void ManageApp()
        {
            if (SelectedItem == null) return;

            Process.Start("SAM.WPF.Manager.exe", SelectedItem.Id.ToString());
        }

        public void ViewOnSteamDB()
        {
            if (SelectedItem == null) return;

            BrowserHelper.ViewOnSteamDB(SelectedItem.Id);
        }

        public void ViewOnSteam()
        {
            if (SelectedItem == null) return;

            BrowserHelper.ViewOnSteamStore(SelectedItem.Id);
        }

        public void ViewOnSteamCardExchange()
        {
            if (SelectedItem == null) return;

            BrowserHelper.ViewOnSteamCardExchange(SelectedItem.Id);
        }

        public void ViewOnPCGW()
        {
            if (SelectedItem == null) return;

            BrowserHelper.ViewOnPCGW(SelectedItem.Id);
        }

        public void CopySteamID()
        {
            if (SelectedItem == null) return;

            TextCopy.ClipboardService.SetText(SelectedItem.Id.ToString());
        }
    }
}
