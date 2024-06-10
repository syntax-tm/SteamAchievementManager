namespace SAM.Console;

public enum ListTarget
{
    Apps,
    Stats,
    Achievements,
    All
}

public enum ExportTarget
{
    AppList,
    Stats,
    Achievements,
    User,
    Settings,
    All
}

public enum StartTarget
{
    SAM,
    Steam,
    SteamConsole,
    SteamApp
}

public enum OutputFormat
{
    Text = 0,
    Json,
    Csv,
    Xml,
    Yml
}
