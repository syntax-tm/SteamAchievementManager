using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using J = Newtonsoft.Json.JsonPropertyAttribute;
using R = Newtonsoft.Json.Required;
using N = Newtonsoft.Json.NullValueHandling;

namespace SAM.Core;

public class SteamAppInfo
{
    public AchievementSection Achievements { get; set; }
    public AchievementMap AchievementMap { get; set; }
    public CustomImages CustomImages { get; set; }
    public Descriptions Descriptions { get; set; }
    public UserNews UserNews { get; set; }
    public SocialMedia SocialMedia { get; set; }
    public Associations Associations { get; set; }
    public WorkshopTrendingItems WorkshopTrendingItems { get; set; }
    public Badge Badge { get; set; }
}

public record Section
{
    [J("Type")] public string Type { get; set; }
    [J("Version")] public int Version { get; set; }
}

public record AchievementSection : Section
{
    [J("nAchieved")] public int Achieved { get; set; }
    [J("nTotal")] public int Total { get; set; }
    [J("vecAchievedHidden")] public List<AchievementInfo> AchievedHidden { get; set; } = [ ];
    [J("vecUnachieved")] public List<AchievementInfo> Unachieved { get; set; } = [ ];
    [J("vecHighlight")] public List<AchievementInfo> Highlight { get; set; } = [ ];
}

public record AchievementInfo
{
    [J("strID")] public string ID { get; set; }
    [J("strName")] public string Name { get; set; }
    [J("strDescription")] public string Description { get; set; }
    [J("bAchieved")] public bool Achieved { get; set; }
    [J("rtUnlocked")] public long Unlocked { get; set; }
    [J("strImage")] public string Image { get; set; }
    [J("bHidden")] public bool Hidden { get; set; }
    [J("flMinProgress")] public double MinProgress { get; set; }
    [J("flCurrentProgress")] public double CurrentProgress { get; set; }
    [J("flMaxProgress")] public double MaxProgress { get; set; }
    [J("flAchieved")] public double GlobalAchieved { get; set; }
}

public record AchievementMap : Section
{
    [J("items")] public JToken Items { get; set; }
}

public record CustomImages : Section
{

}

public record Descriptions : Section
{
    [J("strFullDescription")] public string Description { get; set; }
    [J("strSnippet")] public string Snippet { get; set; }
}

public record UserNews([J("items")] List<string> Items) : Section;

public record SocialMedia
{
    [J("items")] public List<SocialMediaItem> Items { get; set; } = [ ];
}

public record SocialMediaItem([J("eType")] string Type, [J("strName")] string Name, [J("strURL")] string URL);

public record Associations
{
    [J("rgDevelopers")] public List<AssociationItem> Developers { get; set; } = [ ];
    [J("rgFranchises")] public List<AssociationItem> Franchises { get; set; } = [ ];
    [J("rgPublishers")] public List<AssociationItem> Publishers { get; set; } = [ ];

    public string Publisher => Publishers?.FirstOrDefault()?.Name;
    public string Developer => Developers?.FirstOrDefault()?.Name;
    public string Franchise => Franchises?.FirstOrDefault()?.Name;
}

public record AssociationItem([J("strName")] string Name, [J("strURL")] string Url);

public record GameActivity : Section
{

}

public record WorkshopTrendingItems : Section
{

}

public record Badge : Section
{
    [J("bMaxed")] public bool? IsMaxed { get; set; }
    [J("dtNextRetry")] public object NextRetry { get; set; }
    [J("nLevel")] public int Level { get; set; }
    [J("nMaxLevel")] public int MaxLevel { get; set; }
    [J("nNextLevelXP")] public int NextLevelXP { get; set; }
    [J("nXP")] public int XP { get; set; }
    [J("strIconURL")] public string IconURL { get; set; }
    [J("strName")] public string Name { get; set; }
    [J("strNextLevelName")] public string NextLevelName { get; set; }
    [J("rgCards")] public List<Card> Cards { get; set; }
}

public record Card([J("nOwned")] int Owned, [J("strArtworkURL")] string ArtworkURL, [J("strMarketHash")] string MarketHash, [J("strName")] string Name, [J("strTitle")] string Title);

public class SteamAppInfoConverter : JsonConverter
{
    public override bool CanConvert(Type t) => t == typeof(SteamAppInfo) || t == typeof(SteamAppInfo);

    public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
    {
        JToken getSection(JArray items, string name)
        {
            var sectionToken = items.SelectToken(@$"$..[?(@ == '{name}')]");
            if (sectionToken == null) return null;

            var parent = sectionToken.Parent;
            var data = parent?.SelectToken(@"$..data") ?? new JObject();
            var version = parent?.SelectToken("$..version")?.Value<int>() ?? 0;

            if (data.Type == JTokenType.Array)
            {
                var jo = new JObject();
                jo["items"] = data;
                data = jo;
            }
            else if (data.Type == JTokenType.String)
            {
                var jo = new JObject();
                jo["items"] = JArray.Parse(data.ToString());
                data = jo;
            }

            data["Type"] = name;
            data["version"] = version;

            // add the version to the data to make life easier
            //data.Append(new JProperty("version", version));

            return data;
        }

        // the json root should just be an array
        if (reader.TokenType != JsonToken.StartArray) return null;

        var items = JArray.Load(reader);

        var achievementSection = getSection(items, "achievements");
        var achievements = achievementSection?.ToObject<AchievementSection>();

        var customimageSection = getSection(items, "customimage");
        var customimage = customimageSection?.ToObject<CustomImages>();

        var socialmediaSection = getSection(items, "socialmedia");
        var socialmedia = socialmediaSection?.ToObject<SocialMedia>();

        var associationSection = getSection(items, "associations");
        var associations = associationSection?.ToObject<Associations>();

        var descriptionSection = getSection(items, "descriptions");
        var descriptions = descriptionSection?.ToObject<Descriptions>();

        var achievementMapSection = getSection(items, "achievementmap");
        var achievementMap = achievementMapSection?.ToObject<AchievementMap>();

        var usernewsSection = getSection(items, "usernews");
        var usernews = usernewsSection?.ToObject<UserNews>();

        var gameactivitySection = getSection(items, "gameactivity");
        var gameactivity = gameactivitySection?.ToObject<GameActivity>();

        var workshopTrendingItemsSection = getSection(items, "workshop_trendy_items");
        var workshowTrendingItems = workshopTrendingItemsSection?.ToObject<WorkshopTrendingItems>();

        var badgeSection = getSection(items, "badge");
        var badge = badgeSection?.ToObject<Badge>();

        return new SteamAppInfo()
        {
            Achievements = achievements,
            CustomImages = customimage,
            SocialMedia = socialmedia,
            Associations = associations,
            Descriptions = descriptions,
            AchievementMap = achievementMap,
            UserNews = usernews,
            WorkshopTrendingItems = workshowTrendingItems,
            Badge = badge
        };
    }

    public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
    {
        throw new NotSupportedException();
    }

    public static readonly SteamAppInfoConverter Singleton = new ();
}
