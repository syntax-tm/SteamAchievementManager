// See https://aka.ms/new-console-template for more information

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using log4net;
using SAM.Console;

var log = LogManager.GetLogger(nameof(Program));

var helpWriter = new StringWriter();
var parser = new Parser(with =>
{
    //ignore case for enum values
    with.CaseInsensitiveEnumValues = true;
    with.HelpWriter = helpWriter;
});
            
var options = parser.ParseArguments<ManageOptions, ListOptions, StartOptions>(Environment.GetCommandLineArgs())
                    .WithParsed<ManageOptions>(HandleManage)
                    .WithParsed<ListOptions>(opts => opts.ToString())
                    .WithParsed<StartOptions>(opts => opts.ToString())
                    .WithNotParsed(errs => DisplayHelp(errs, helpWriter));

Console.WriteLine("Hello, World!");

// ReSharper disable once InconsistentNaming
static void DisplayHelp(IEnumerable<Error> err, TextWriter helpWriter)
{
    var errors = err.ToList();

    if (errors.IsVersion() || errors.IsHelp())
    {
        Console.WriteLine(helpWriter.ToString());
    }
    else
    {
        Console.Error.WriteLine(helpWriter.ToString());
    }
}

static void HandleManage(ManageOptions options)
{

}
