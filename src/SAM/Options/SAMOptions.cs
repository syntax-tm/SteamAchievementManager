using CommandLine;

namespace SAM;

//[Verb(VERB, true, HelpText = "Options for performing various SAM tasks")]
public class SAMOptions
{
    //public const string VERB = "manage";

    //[Option('a', "appid", HelpText = "A Steam app id")]
    [Value(0, HelpText = "A Steam app id")]
    public uint AppId { get; set; }

    [Option('r', "reset", Default = false)]
    public bool Reset { get; set; }
    
    [Option('o', "offline", Default = false)]
    public bool OfflineMode { get; set; }
}
