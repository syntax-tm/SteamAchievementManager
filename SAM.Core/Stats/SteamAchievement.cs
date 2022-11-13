﻿using System.Diagnostics;
using System.Drawing;
using System.Windows.Input;
using DevExpress.Mvvm;
using SAM.API.Stats;

namespace SAM.Core.Stats
{
    [DebuggerDisplay("{Name} ({Id})")]
    public class SteamAchievement : BindableBase
    {
        private readonly bool _isLoading = true;

        public uint GameId { get; }
        public string Id => AchievementInfo.Id;
        public string IconLockedName => AchievementInfo.IconLocked;
        public string IconNormalName => AchievementInfo.IconNormal;
        public string Name => AchievementInfo.Name;
        public string Description => AchievementDefinition.IsHidden && !IsAchieved ? @"Hidden" : AchievementInfo.Description;
        public string FullDescription => AchievementInfo.Description;
        public int Permission => AchievementInfo.Permission;
        public bool OriginalLockState
        {
            get => GetProperty(() => OriginalLockState);
            set => SetProperty(() => OriginalLockState, value);
        }
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
        public bool IsSelected
        {
            get => GetProperty(() => IsSelected);
            set => SetProperty(() => IsSelected, value);
        }
        public bool IsAchieved
        {
            get => GetProperty(() => IsAchieved);
            set => SetProperty(() => IsAchieved, value, OnIsAchievedChanged);
        }
        
        public AchievementInfo AchievementInfo { get; }
        public AchievementDefinition AchievementDefinition { get; }

        public ICommand UnlockCommand => new DelegateCommand(Unlock);
        public ICommand LockCommand => new DelegateCommand(Lock);
        public ICommand ResetCommand => new DelegateCommand(Reset);
        public ICommand InvertCommand => new DelegateCommand(Invert);

        public SteamAchievement()
        {

        }

        public SteamAchievement(uint gameId, AchievementInfo info, AchievementDefinition definition)
        {
            GameId = gameId;
            AchievementInfo = info;
            AchievementDefinition = definition;
            
            OriginalLockState = info.IsAchieved;
            IsAchieved = info.IsAchieved;

            DownloadIcons();

            _isLoading = false;
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

        public void CommitChanges()
        {
            if (!IsModified) return;

            OriginalLockState = IsAchieved;
            IsModified = false;
        }

        public override string ToString()
        {
            return $"{Name} ({Id})";
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
            if (_isLoading) return;

            RefreshImage();

            IsModified = IsAchieved != OriginalLockState;
        }

    }
}
