using Spectre.Console.Cli;
using System.ComponentModel;

namespace SAM.Console.Commands;

public abstract class SettingsBase : CommandSettings
{
    [Description("The format of the results")]
    [CommandOption("-f|--format <FORMAT>")]
    [DefaultValue(OutputFormat.Text)]
    public OutputFormat Format { get; set; }

    [Description("The file to save the results to")]
    [CommandOption("-o|--outfile <FILE_NAME>")]
    public string OutFile { get; set; }
}
