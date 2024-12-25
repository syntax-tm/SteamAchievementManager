using CommandLine;

namespace SAM;

[Verb("select", true, ["open", "library", "apps"], HelpText = "Opens the library view for selecting apps")]
public class SelectOptions
{
    [Option('r', "reset", Default = false)]
    public bool Reset { get; set; }

    [Option('g', "grid", Default = false)]
    public bool GridView { get; set; }

    [Option('t', "tile", Default = false)]
    public bool TileView { get; set; }
}

[Verb("manage", false, HelpText = "Opens the manager for the specified app")]
public class ManageOptions
{
    public const char UnlockAllShort = 'u';

    [Value(0, HelpText = "A Steam app id", MetaName = "AppId")]
    public uint AppId { get; set; }
    
    [Option(UnlockAllShort, "unlock-all", Default = false)]
    public bool UnlockAll { get; set; }
}
