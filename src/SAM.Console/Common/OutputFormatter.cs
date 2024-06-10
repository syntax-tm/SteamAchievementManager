using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SAM.Console.Commands;
using Spectre.Console;

namespace SAM.Console;

public static class OutputManager
{

    public static string SendResult<T>(T value, SettingsBase settings)
    {
        var type = settings.Format;

        if (type == OutputFormat.Text)
        {
            var table = new Table();

            var props = GetProps(value);

            foreach (var propName in props)
            {
                table.AddColumn(new (propName));
            }

            
        }

        return null;
    }

    public static string SendResults<T>(IList<T> values, SettingsBase settings)
    {
        var type = settings.Format;

        if (type == OutputFormat.Text)
        {
            var table = new Table();

            var props = GetProps(values[0]);

            foreach (var propName in props)
            {
                table.AddColumn(new (propName));
            }

            foreach (var value in values)
            {
                //table.AddRow()
            }
        }

        return null;
    }

    private static List<string> GetProps<T>(T value)
    {
        var props = value.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

        var names = props.Select(p => p.Name).ToList();

        return names;
    }

}
