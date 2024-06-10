using System.ComponentModel;
using SAM.Core;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SAM.Console.Commands;

public sealed class StartSettings : SettingsBase
{
    [Description("The app's unique Steam ID.")]
    [CommandArgument(0, "[appId]")]
    public uint AppId { get; set; }
}

public sealed class StartCommand : Command<StartSettings>
{
    public override int Execute(CommandContext context, StartSettings settings)
    {
        SteamClientManager.Init(settings.AppId);

        AnsiConsole.MarkupLine($"You are managing [green]{settings.AppId}[/].");

        return 0;
    }
}
