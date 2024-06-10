using SAM.Core;
using SAM.Core.Interfaces;
using Spectre.Console.Cli;
using System.Collections.Generic;
using SAM.API;

namespace SAM.Console.Commands;

internal class ListCommand
{
}

public class ListAppsSettings : SettingsBase
{

}

public class ListAppsCommand : Command<ListAppsSettings>
{
    public override int Execute(CommandContext context, ListAppsSettings settings)
    {
        SteamClientManager.Init(0);

        var apps = SteamworksManager.GetAppList();

        var ownedApps = new List<ISupportedApp>();

        foreach (var app in apps)
        {
            var owned = SteamClientManager.Default.OwnsGame(app.Key);

            if (!owned) continue;

            var title = SteamClientManager.Default.GetAppName(app.Key);

            ownedApps.Add(new SupportedApp(app.Key, app.Value, title));
        }

        // Omitted
        return 0;
    }
}
