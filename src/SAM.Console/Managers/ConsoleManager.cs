using System.Collections.Generic;
using SAM.Core.AppInfo;
using SAM.Core.Interfaces;
using Spectre.Console;

namespace SAM.Console.Managers;

public static class ConsoleManager
{
    public static T ShowMenu<T>(IPrompt<T> prompt)
    {
        T result = default;

        AnsiConsole.AlternateScreen(() =>
        {
            result = AnsiConsole.Prompt(prompt);
        });

        return result;
    }
}
