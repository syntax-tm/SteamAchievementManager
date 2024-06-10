using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using JetBrains.Annotations;
using Spectre.Console.Cli;

namespace SAM.Console;



//[Verb("list", false, ["print", "show"], HelpText = "List available apps, achievements, stats, and more.")]
//public class ListOptions : OptionsBase
//{
//    [Option('t', "target", Default = ListTarget.All, HelpText = $"The target type to display")]
//    public ListTarget Target { get; set; }
//}

//[Verb("export", false, ["save"], HelpText = "Export apps, stats, settings, and more.")]
//public class ExportOptions : OptionsBase
//{
//    [Option('t', "target", Default = ListTarget.All, HelpText = $"The information to export")]
//    public ExportTarget Target { get; set; }
//}

//[Verb("start", false, ["run", "launch"], HelpText = "Start Steam, Steam Console, installed app, nad more.")]
//public class StartOptions : OptionsBase
//{
//    [Option('t', "target", Default = StartTarget.SAM, HelpText = $"The target to start")]
//    public StartTarget Target { get; set; }
//}
