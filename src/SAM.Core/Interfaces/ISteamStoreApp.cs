using System.Collections.Generic;

namespace SAM.Core.Interfaces;

public interface ISteamStoreApp
{
    string Type { get; set; }
    string Name { get; set; }
    int SteamAppid { get; set; }
    string RequiredAge { get; set; }
    bool IsFree { get; set; }
    string ControllerSupport { get; set; }
    List<uint> Dlc { get; set; }
    List<SteamStoreApp> DlcInfo { get; set; }
    string DetailedDescription { get; set; }
    string AboutTheGame { get; set; }
    string ShortDescription { get; set; }
    string SupportedLanguages { get; set; }
    string Reviews { get; set; }
    string HeaderImage { get; set; }
    object Website { get; set; }
    object PcRequirements { get; set; }
    object MacRequirements { get; set; }
    object LinuxRequirements { get; set; }
    string LegalNotice { get; set; }
    List<string> Developers { get; set; }
    List<string> Publishers { get; set; }
    PriceOverview PriceOverview { get; set; }
    List<int> Packages { get; set; }
    List<PackageGroup> PackageGroups { get; set; }
    Platforms Platforms { get; set; }
    List<Category> Categories { get; set; }
    List<Genre> Genres { get; set; }
    List<Screenshot> Screenshots { get; set; }
    List<SteamStoreMovie> Movies { get; set; }
    Recommendations Recommendations { get; set; }
    Achievements Achievements { get; set; }
    ReleaseDate ReleaseDate { get; set; }
    SupportInfo SupportInfo { get; set; }
    string Background { get; set; }
    ContentDescriptors ContentDescriptors { get; set; }
}

public interface IPcRequirements
{
    string Minimum { get; set; }
    string Recommended { get; set; }
}

public interface IPriceOverview
{
    string Currency { get; set; }
    int Initial { get; set; }
    int Final { get; set; }
    int DiscountPercent { get; set; }
    string InitialFormatted { get; set; }
    string FinalFormatted { get; set; }
}

public interface ISub
{
    int PackageId { get; set; }
    string PercentSavingsText { get; set; }
    int PercentSavings { get; set; }
    string OptionText { get; set; }
    string OptionDescription { get; set; }
    string CanGetFreeLicense { get; set; }
    bool IsFreeLicense { get; set; }
    int PriceInCentsWithDiscount { get; set; }
}

public interface IPackageGroup
{
    string Name { get; set; }
    string Title { get; set; }
    string Description { get; set; }
    string SelectionText { get; set; }
    string SaveText { get; set; }
    string DisplayType { get; set; }
    string IsRecurringSubscription { get; set; }
    List<Sub> Subs { get; set; }
}

public interface IPlatforms
{
    bool Windows { get; set; }
    bool Mac { get; set; }
    bool Linux { get; set; }
}

public interface ICategory
{
    int Id { get; set; }
    string Description { get; set; }
}

public interface IGenre
{
    string Id { get; set; }
    string Description { get; set; }
}

public interface IScreenshot
{
    int Id { get; set; }
    string PathThumbnail { get; set; }
    string PathFull { get; set; }
}

public interface ISteamStoreMovieInfo
{
    string Default { get; set; }
    string Max { get; set; }
}

public interface ISteamStoreMovie
{
    int Id { get; set; }
    string Name { get; set; }
    string Thumbnail { get; set; }
    SteamStoreMovieInfo WebM { get; set; }
    SteamStoreMovieInfo MP4 { get; set; }
    bool Highlight { get; set; }
}

public interface IRecommendations
{
    int Total { get; set; }
}

public interface IHighlighted
{
    string Name { get; set; }
    string Path { get; set; }
}

public interface IAchievements
{
    int Total { get; set; }
    List<Highlighted> Highlighted { get; set; }
}

public interface IReleaseDate
{
    bool ComingSoon { get; set; }
    string Date { get; set; }
}

public interface ISupportInfo
{
    string Url { get; set; }
    string Email { get; set; }
}

public interface IContentDescriptors
{
    List<object> Ids { get; set; }
    object Notes { get; set; }
}
