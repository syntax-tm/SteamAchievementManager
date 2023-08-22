using System.Windows;
using DevExpress.Mvvm.POCO;

namespace SAM.WPF.ViewModels
{
    public class MainWindowViewModel
    {
        public virtual string Title { get; set; } = "Steam Achievement Manager";
        public virtual int Width { get; set; } = 1024;
        public virtual int Height { get; set; } = 768;
        public virtual int X { get; set; }
        public virtual int Y { get; set; }
        public virtual WindowState WindowState { get; set; }
        public virtual HomeViewModel HomeVm { get; set; }

        protected MainWindowViewModel()
        {
        }

        public static MainWindowViewModel Create()
        {
            return ViewModelSource.Create(() => new MainWindowViewModel());
        }
    }
}