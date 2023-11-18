using DevExpress.Mvvm;

namespace SAM.Core.Settings
{
    public class LibrarySettings : BindableBase
    {
        private const ushort DEFAULT_ITEM_HEIGHT = 100;

        public ushort ItemHeight
        {
            get => GetProperty(() => ItemHeight);
            set => SetProperty(() => ItemHeight, value);
        }

        public LibrarySettings()
        {
            ItemHeight = DEFAULT_ITEM_HEIGHT;
        }
    }
}
