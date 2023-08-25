using System.Reflection;
using SAM.Core.Behaviors;

namespace SAM.Core.ViewModels
{
    public class MainWindowViewModelBase
    {
        private const string TITLE_BASE = "Steam Achievement Manager";

        public virtual string Title { get; protected set; } = TITLE_BASE;
        public virtual string SubTitle { get; set; }
        public virtual WindowSettings Config { get; set; }

        protected MainWindowViewModelBase()
        {

        }
        
        protected void OnSubTitleChanged()
        {
            if (string.IsNullOrWhiteSpace(SubTitle))
            {
                Title = TITLE_BASE;
                return;
            }

            Title = $"{TITLE_BASE} | {SubTitle}";
        }
    }
}
