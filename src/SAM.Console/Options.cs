using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

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
    Json = 0,
    Text,
    Csv,
    Xml,
    Yml
}

public abstract class OptionsBase
{
    [Option('f', "format", Default = OutputFormat.Json, HelpText = "The format of the results")]
    public OutputFormat Format { get; set; }

    [Option('o', "outfile", HelpText = "The file to save the results to.")]
    public string OutFile { get; set; }

    [Option('s', "simple", HelpText = "If set will only show output using plain (non-ANSI) text.")]
    public bool SimpleOutput { get; set; }
}

[Verb("manage", true, HelpText = "List available apps, achievements, stats, and more.")]
public class ManageOptions : OptionsBase
{
    [Option('a', "app", Default = 0, HelpText = "The app's unique Steam ID.")]
    [Value(0, Min = 0, MetaName = "app", HelpText = "The app's unique Steam ID.")]
    public uint AppId { get; set; }
}

[Verb("list", false, ["print", "show"], HelpText = "List available apps, achievements, stats, and more.")]
public class ListOptions : OptionsBase
{
    [Option('t', "target", Default = ListTarget.All, HelpText = $"The target type to display")]
    public ListTarget Target { get; set; }
}

[Verb("export", false, ["save"], HelpText = "Export apps, stats, settings, and more.")]
public class ExportOptions : OptionsBase
{
    [Option('t', "target", Default = ListTarget.All, HelpText = $"The information to export")]
    public ExportTarget Target { get; set; }
}

[Verb("start", false, ["run", "launch"], HelpText = "Start Steam, Steam Console, installed app, nad more.")]
public class StartOptions : OptionsBase
{
    [Option('t', "target", Default = StartTarget.SAM, HelpText = $"The target to start")]
    public StartTarget Target { get; set; }
}
