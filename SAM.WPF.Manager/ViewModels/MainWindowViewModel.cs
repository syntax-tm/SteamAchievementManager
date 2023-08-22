using DevExpress.Mvvm.POCO;
using System.Windows;

namespace SAM.WPF.Manager.ViewModels
{
    public class MainWindowViewModel
    {

        public virtual string Title { get; set; } = "Steam Achievement Manager";
        public virtual int Width { get; set; } = 1024;
        public virtual int Height { get; set; } = 768;
        public virtual int X { get; set; }
        public virtual int Y { get; set; }
        public virtual WindowState WindowState { get; set; }

        protected MainWindowViewModel()
        {

        }
        
        public static MainWindowViewModel Create()
        {
            return ViewModelSource.Create(() => new MainWindowViewModel());
        }

    }
}
