using System;

namespace SAM.Core;

[Flags]
public enum SteamImageType
{
    /// <summary>
    /// {APPID}.jpg
    /// </summary>
    GridLandscape = 1,
    /// <summary>
    /// {APPID}p.jpg
    /// </summary>
    GridPortrait = 2,
    /// <summary>
    /// {APPID}_icon.jpg
    /// </summary>
    GridIcon = 4,
    /// <summary>
    /// {APPID}_hero.jpg
    /// </summary>
    GridHero = 8,
    Grid = GridLandscape | GridPortrait | GridIcon | GridHero,
    ClientIcon,
    /// <summary>
    /// {APPID}_icon.jpg
    /// </summary>
    Icon,
    /// <summary>
    /// logo.png
    /// </summary>
    Logo,
    /// <summary>
    /// library_header.jpg
    /// </summary>
    Header,
    /// <summary>
    /// library_600x900.jpg
    /// </summary>
    Library,
    /// <summary>
    /// library_hero.jpg
    /// </summary>
    LibraryHero,
    /// <summary>
    /// library_header.jpg
    /// </summary>
    LibraryHeader,
    /// <summary>
    /// library_hero_blur.jpg
    /// </summary>
    LibraryHeroBlur,
    /// <summary>
    /// capsule_231x87.jpg
    /// </summary>
    SmallCapsule,
    /// <summary>
    /// capsule_467x181.jpg
    /// </summary>
    MediumCapsule,
    /// <summary>
    /// capsule_616x353.jpg
    /// </summary>
    LargeCapsule,
    AchievementIcon,
}
