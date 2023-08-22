using System.Collections.Generic;
using Newtonsoft.Json;

namespace SAM.WPF.Core.API.Steam
{
    public class PcRequirements
    {

        [JsonProperty("minimum")]
        public string Minimum { get; set; }

        [JsonProperty("recommended")]
        public string Recommended { get; set; }
    }

    public class PriceOverview
    {

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("initial")]
        public int Initial { get; set; }

        [JsonProperty("final")]
        public int Final { get; set; }

        [JsonProperty("discount_percent")]
        public int DiscountPercent { get; set; }

        [JsonProperty("initial_formatted")]
        public string InitialFormatted { get; set; }

        [JsonProperty("final_formatted")]
        public string FinalFormatted { get; set; }
    }

    public class Sub
    {

        [JsonProperty("packageid")]
        public int PackageId { get; set; }

        [JsonProperty("percent_savings_text")]
        public string PercentSavingsText { get; set; }

        [JsonProperty("percent_savings")]
        public int PercentSavings { get; set; }

        [JsonProperty("option_text")]
        public string OptionText { get; set; }

        [JsonProperty("option_description")]
        public string OptionDescription { get; set; }

        [JsonProperty("can_get_free_license")]
        public string CanGetFreeLicense { get; set; }

        [JsonProperty("is_free_license")]
        public bool IsFreeLicense { get; set; }

        [JsonProperty("price_in_cents_with_discount")]
        public int PriceInCentsWithDiscount { get; set; }
    }

    public class PackageGroup
    {

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("selection_text")]
        public string SelectionText { get; set; }

        [JsonProperty("save_text")]
        public string SaveText { get; set; }

        [JsonProperty("display_type")]
        public string DisplayType { get; set; }

        [JsonProperty("is_recurring_subscription")]
        public string IsRecurringSubscription { get; set; }

        [JsonProperty("subs")]
        public IList<Sub> Subs { get; set; } = new List<Sub>();
    }

    public class Platforms
    {

        [JsonProperty("windows")]
        public bool Windows { get; set; }

        [JsonProperty("mac")]
        public bool Mac { get; set; }

        [JsonProperty("linux")]
        public bool Linux { get; set; }
    }

    public class Category
    {

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }

    public class Genre
    {

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }

    public class Screenshot
    {

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("path_thumbnail")]
        public string PathThumbnail { get; set; }

        [JsonProperty("path_full")]
        public string PathFull { get; set; }
    }

    public class Webm
    {

        [JsonProperty("480")]
        public string Default { get; set; }

        [JsonProperty("max")]
        public string Max { get; set; }
    }

    public class Mp4
    {

        [JsonProperty("480")]
        public string Default { get; set; }

        [JsonProperty("max")]
        public string Max { get; set; }
    }

    public class Movy
    {

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("thumbnail")]
        public string Thumbnail { get; set; }

        [JsonProperty("webm")]
        public Webm Webm { get; set; }

        [JsonProperty("mp4")]
        public Mp4 Mp4 { get; set; }

        [JsonProperty("highlight")]
        public bool Highlight { get; set; }
    }

    public class Recommendations
    {

        [JsonProperty("total")]
        public int Total { get; set; }
    }

    public class Highlighted
    {

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }
    }

    public class Achievements
    {

        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("highlighted")]
        public IList<Highlighted> Highlighted { get; set; } = new List<Highlighted>();
    }

    public class ReleaseDate
    {

        [JsonProperty("coming_soon")]
        public bool ComingSoon { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; }
    }

    public class SupportInfo
    {

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
    }

    public class ContentDescriptors
    {

        [JsonProperty("ids")]
        public IList<object> Ids { get; set; }

        [JsonProperty("notes")]
        public object Notes { get; set; }
    }

    public class SteamStoreApp
    {

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("steam_appid")]
        public int SteamAppid { get; set; }

        [JsonProperty("required_age")]
        public string RequiredAge { get; set; }

        [JsonProperty("is_free")]
        public bool IsFree { get; set; }

        [JsonProperty("controller_support")]
        public string ControllerSupport { get; set; }

        [JsonProperty("dlc")]
        public IList<uint> Dlc { get; set; } = new List<uint>();

        public IList<SteamStoreApp> DlcInfo { get; set; } = new List<SteamStoreApp>();

        [JsonProperty("detailed_description")]
        public string DetailedDescription { get; set; }

        [JsonProperty("about_the_game")]
        public string AboutTheGame { get; set; }

        [JsonProperty("short_description")]
        public string ShortDescription { get; set; }

        [JsonProperty("supported_languages")]
        public string SupportedLanguages { get; set; }

        [JsonProperty("reviews")]
        public string Reviews { get; set; }

        [JsonProperty("header_image")]
        public string HeaderImage { get; set; }

        [JsonProperty("website")]
        public object Website { get; set; }

        [JsonProperty("pc_requirements")]
        public object PcRequirements { get; set; }

        [JsonProperty("mac_requirements")]
        public object MacRequirements { get; set; }

        [JsonProperty("linux_requirements")]
        public object LinuxRequirements { get; set; }

        [JsonProperty("legal_notice")]
        public string LegalNotice { get; set; }

        [JsonProperty("developers")]
        public IList<string> Developers { get; set; }

        [JsonProperty("publishers")]
        public IList<string> Publishers { get; set; }

        [JsonProperty("price_overview")]
        public PriceOverview PriceOverview { get; set; }

        [JsonProperty("packages")]
        public IList<int> Packages { get; set; }

        [JsonProperty("package_groups")]
        public IList<PackageGroup> PackageGroups { get; set; }

        [JsonProperty("platforms")]
        public Platforms Platforms { get; set; }

        [JsonProperty("categories")]
        public IList<Category> Categories { get; set; }

        [JsonProperty("genres")]
        public IList<Genre> Genres { get; set; }

        [JsonProperty("screenshots")]
        public IList<Screenshot> Screenshots { get; set; }

        [JsonProperty("movies")]
        public IList<Movy> Movies { get; set; }

        [JsonProperty("recommendations")]
        public Recommendations Recommendations { get; set; }

        [JsonProperty("achievements")]
        public Achievements Achievements { get; set; }

        [JsonProperty("release_date")]
        public ReleaseDate ReleaseDate { get; set; }

        [JsonProperty("support_info")]
        public SupportInfo SupportInfo { get; set; }

        [JsonProperty("background")]
        public string Background { get; set; }

        [JsonProperty("content_descriptors")]
        public ContentDescriptors ContentDescriptors { get; set; }
    }
}
