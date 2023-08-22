using System;
using System.Collections.Generic;
using System.Text;

namespace SAM.WPF.Core.Settings
{
    public class ManagerSettings
    {

        /// <summary>
        /// Whether or not to show the description for achievements marked "hidden" that are
        /// not unlocked.
        /// </summary>
        public bool ShowHidden { get; set; }
        public bool GroupAchievements { get; set; }

    }
}
