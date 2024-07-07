using CommandLine;

namespace SAM;

public class SAMOptions
{
    [Value(0, HelpText = "A Steam app id")]
    public uint AppId { get; set; }

    [Option('r', "reset", Default = false)]
    public bool Reset { get; set; }
    
    [Option('o', "offline", Default = false)]
    public bool OfflineMode { get; set; }

    [Option('g', "grid", Default = false)]
    public bool GridView { get; set; }

    [Option('t', "tile", Default = false)]
    public bool TileView { get; set; }
}
