using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace SAM.Settings;

[DebuggerDisplay("{ToString()}")]
public class SteamAppSettings
{
    // NOTE: increment this whenever new properties are added
    // after loading the settings, if the version between the saved
    // data is the current version, then we don't need to refresh
    // everything and save for no reason
    private const int CURRENT_VERSION = 3;

    public static int CurrentVersion => CURRENT_VERSION;
    public int? Version { get; set; }

    // indicates what properties were available the last time it was
    // saved in order to avoid unnecessary refreshing
    [JsonIgnore]
    public bool IsValid => Version == CURRENT_VERSION;

    [JsonIgnore]
    public bool ImagesLoaded => AreImagesLoaded();

    public uint AppId { get; set; }
    public string Name { get; set; }
    public bool IsFavorite { get; set; }
    public bool IsHidden { get; set; }
    public Uri Header { get; set; }
    public bool IsAnimatedHeader { get; set; }
    public Uri Capsule { get; set; }
    public bool IsAnimatedCapsule { get; set; }
    public string Group { get; set; }
    public ulong? GroupSortIndex { get; set; }

    public SteamAppSettings()
    {

    }

    public SteamAppSettings(SteamApp app)
    {
        AppId = app.Id;
        Name = app.Name;
        IsFavorite = app.IsFavorite;
        IsHidden = app.IsHidden;
        Header = app.Header;
        IsAnimatedHeader = app.IsAnimatedHeader;
        Capsule = app.Capsule;
        IsAnimatedCapsule = app.IsAnimatedCapsule;
        Group = app.Group;
        GroupSortIndex = app.GroupSortIndex;
        Version = CURRENT_VERSION;
    }

    public static SteamAppSettings Create(uint id, string name)
    {
        var group = AppGroupHelper.GetGroup(name);
        return new ()
        {
            AppId = id,
            Name = name,
            Group = group.Name,
            GroupSortIndex = group.SortIndex
        };
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append($"{AppId}");

        var attributes = new List<string>();

        if (IsFavorite) attributes.Add("Favorite");
        if (IsHidden) attributes.Add("Hidden");

        if (!attributes.Any()) return sb.ToString();

        sb.Append(" (");
        sb.Append(string.Join(", ", attributes));
        sb.Append(")");

        return sb.ToString();
    }

    private bool AreImagesLoaded()
    {
        return Header != null && Capsule != null;
    }
}
