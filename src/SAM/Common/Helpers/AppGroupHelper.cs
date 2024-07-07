namespace SAM;

public static class AppGroupHelper
{
    private const string FAVORITES_GROUP = "Favorites";
    private const string HIDDEN_GROUP = "Hidden";
    private const string MISC_GROUP = "Misc.";
    private const string NUMBER_GROUP = "#";

    public record AppGroup(string Name, ulong SortIndex);

    public static AppGroup GetGroup(string name)
    {
        var groupName = GetGroupTitle(name);
        var sortIndex = GetGroupSortIndex(groupName);

        return new (groupName, sortIndex);
    }

    public static AppGroup GetGroup(SteamApp app)
    {
        var groupName = GetGroupTitle(app);
        var sortIndex = GetGroupSortIndex(groupName);

        return new (groupName, sortIndex);
    }
    
    private static string GetGroupTitle(SteamApp app)
    {
        return GetGroupTitle(app.Name, app.IsFavorite, app.IsHidden);
    }

    private static string GetGroupTitle(string name, bool isFavorite = false, bool isHidden = false)
    {
        if (isFavorite) return FAVORITES_GROUP;
        if (isHidden) return HIDDEN_GROUP;
        if (string.IsNullOrEmpty(name)) return MISC_GROUP;

        var firstChar = name.ToUpperInvariant()[0];

        if (char.IsDigit(firstChar)) return NUMBER_GROUP;

        return $"{firstChar}";
    }

    private static ulong GetGroupSortIndex(string group)
    {
        return group switch
        {
            FAVORITES_GROUP => 0,
            HIDDEN_GROUP    => int.MaxValue,
            MISC_GROUP      => uint.MaxValue,
            NUMBER_GROUP    => 1,
            _               => group![0]
        };
    }
}
