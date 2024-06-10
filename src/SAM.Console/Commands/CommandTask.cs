using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SAM.Console.Commands;

public enum Command
{
    Manage,
    List,
    Start,
    Help,
    Version
}

[DebuggerDisplay("{ToString()}")]
public class CommandTaskArgument
{
    public string Name { get; set; }
    public object Value { get; set; }
    public bool IsValid => !string.IsNullOrEmpty(Name);

    public CommandTaskArgument(string name)
    {
        Name = name;
    }

    public CommandTaskArgument(string name, object value)
    {
        Name = name;
        Value = value;
    }

    public override string ToString()
    {
        if (Value == null)
        {
            return $"{Name}";
        }

        var value = $"{Value}";

        // only quote it if there's whitespace characters
        if (Regex.IsMatch(value, @"\s", RegexOptions.Singleline))
        {
            value = $"\"{value}\"";
        }

        return $"{Name} {value}";
    }
}

public class CommandTask
{
    private const string EXE_NAME = @"SAM.Console.exe";

    private static Assembly CurrentAssembly => Assembly.GetExecutingAssembly();
    private static string AssemblyLocation => CurrentAssembly.Location;

    public IList<CommandTaskArgument> Arguments { get; set; } = [ ];

    public void AddArgument(string name)
    {
        Arguments.Add(new (name));
    }

    public void AddArgument(string name, object value)
    {
        Arguments.Add(new (name, value));
    }
    
    public void AddArgument(CommandTaskArgument arg)
    {
        Arguments.Add(arg);
    }

    public string Execute()
    {
        if (!Arguments.Any()) throw new InvalidOperationException($"Arguments must be set before executing the command.");

        var args = Arguments.Select(a => a.ToString()).ToList();

        var psi = new ProcessStartInfo(EXE_NAME, args)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };
        var proc = Process.Start(psi);

        proc.WaitForExit();

        var output = proc.StandardOutput.ReadToEnd();

        return output;
    }

    public T ExecuteAs<T>()
    {
        if (!Arguments.Any()) throw new InvalidOperationException($"Arguments must be set before executing the command.");

        var args = Arguments.Select(a => a.ToString()).ToList();

        var psi = new ProcessStartInfo(EXE_NAME, args)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };
        var proc = Process.Start(psi);

        proc.WaitForExit();

        var output = proc.StandardOutput.ReadToEnd();

        var result = JsonConvert.DeserializeObject<T>(output);

        return result;
    }
}
