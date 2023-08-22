using System.Diagnostics;
using System.Drawing;
using DevExpress.Mvvm;
using SAM.Game.Stats;
using SAM.WPF.Core.API.Steam;
using SAM.WPF.Core.Extensions;

namespace SAM.WPF.Core.Stats
{
    [DebuggerDisplay("{Name} ({Id})")]
    public class SteamAchievement : BindableBase
    {

        public uint GameId { get; }
        public string Id => AchievementInfo.Id;
        public string IconLockedName => AchievementInfo.IconLocked;
        public string IconNormalName => AchievementInfo.IconNormal;
        public string Name => AchievementInfo.Name;
        public string Description => AchievementInfo.Description;
        public int Permission => AchievementInfo.Permission;
        public bool OriginalLockState => AchievementInfo.IsAchieved;
        
        public Image Image
        {
            get => GetProperty(() => Image);
            set => SetProperty(() => Image, value);
        }
        public Image LockedImage
        {
            get => GetProperty(() => LockedImage);
            set => SetProperty(() => LockedImage, value);
        }
        public Image NormalImage
        {
            get => GetProperty(() => NormalImage);
            set => SetProperty(() => NormalImage, value);
        }
        public bool IsModified
        {
            get => GetProperty(() => IsModified);
            set => SetProperty(() => IsModified, value);
        }
        public bool IsAchieved
        {
            get => GetProperty(() => IsAchieved);
            set => SetProperty(() => IsAchieved, value, OnIsAchievedChanged);
        }


        public AchievementInfo AchievementInfo { get;}
        public AchievementDefinition AchievementDefinition { get; }

        public SteamAchievement()
        {

        }

        public SteamAchievement(uint gameId, AchievementInfo info, AchievementDefinition definition)
        {
            GameId = gameId;
            AchievementInfo = info;
            AchievementDefinition = definition;

            IsAchieved = info.IsAchieved;

            DownloadIcons();
        }

        public void Unlock()
        {
            if (IsAchieved) return;

            IsAchieved = true;
            IsModified = true;
        }

        public void Lock()
        {
            if (!IsAchieved) return;

            IsAchieved = false;
            IsModified = true;
        }

        public void Reset()
        {
            IsAchieved = OriginalLockState;
            IsModified = false;
        }

        public void Invert()
        {
            IsAchieved = !IsAchieved;
        }

        private void RefreshImage()
        {
            Image = IsAchieved ? NormalImage : LockedImage;
        }

        private void DownloadIcons()
        {
            LockedImage = SteamCdnHelper.DownloadImage(GameId, SteamImageType.AchievementIcon, IconLockedName);
            NormalImage = SteamCdnHelper.DownloadImage(GameId, SteamImageType.AchievementIcon, IconNormalName);

            RefreshImage();
        }

        private void OnIsAchievedChanged()
        {
            RefreshImage();

            IsModified = IsAchieved != OriginalLockState;
        }

    }
}
