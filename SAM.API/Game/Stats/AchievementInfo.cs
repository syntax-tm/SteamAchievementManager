using System.Windows.Forms;

namespace SAM.API.Game.Stats
{
    public class AchievementInfo
    {
        public string Id;
        public bool IsAchieved;
        public int Permission;
        public string IconNormal;
        public string IconLocked;
        public string Name;
        public string Description;
        public ListViewItem Item;

        #region public int ImageIndex;
        public int ImageIndex
        {
            get { return this.Item.ImageIndex; }

            set { this.Item.ImageIndex = value; }
        }
        #endregion
    }
}
