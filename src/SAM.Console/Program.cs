using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using log4net;
using Newtonsoft.Json;
using SAM.Console.Commands;
using SAM.Core;
using SAM.Core.AppInfo;
using SAM.Core.Interfaces;
using Spectre.Console.Cli;
using Terminal.Gui;
using ValveKeyValue;
using Attribute = Terminal.Gui.Attribute;

namespace SAM.Console
{
    public static class Program
    {
        private static readonly ILog log = LogManager.GetLogger(nameof(Program));

        private static Label AppHeader { get; set; }
        private static MenuBar Menu { get; set; }
        private static Window MainWindow { get; set; }
        private static StatusBar StatusBar { get; set; }
        private static StatusItem UserStatusItem { get; set; }

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Application.Init();

                AddMenu();

                MainWindow = new Window("MainWindow")
                {
                    X = 0,
                    Y = 1,
                    Width = Dim.Fill(),
                    Height = Dim.Fill() - 1,
                    Border = new Border()
                    {
                        BorderStyle = BorderStyle.Double
                    },
                    ColorScheme = new ()
                    {
                        Normal = Attribute.Make(Color.DarkGray, Color.Black)
                    }
                };

                var menuOptions = new List<string>()
                {
                    "Manage",
                    "List",
                    "Start",
                    "Help",
                    "Version"
                };

                BuildMenu(menuOptions, MainMenuSelectionChanged);
                
                AddStatusBar();

                // Add both menu and win in a single call
                Application.Top.Add(AppHeader, Menu, MainWindow, StatusBar);
                Application.Run();
                Application.Shutdown();

                return;
            }

            var app = new CommandApp();

            // https://spectreconsole.net/cli/commandapp
            app.Configure(config =>
            {
                config.AddCommand<ManageCommand>("manage");

                //config.AddBranch<ListCommand>("manage", add =>
                //{
                //    add.AddCommand<ListAppsCommand>("apps");
                //    add.AddCommand<AddReferenceCommand>("reference");
                //});
            });

            app.Run(args);

            // ReSharper disable once InconsistentNaming
        }

        private static void MainMenuSelectionChanged(ListViewItemEventArgs args)
        {
            var selected = args.Value as string;

            switch (selected)
            {
                case "Manage":
                    var apps = AppInfoManager.Default.Game;

                    var appList = apps.Select(a => a.ToString()).Order().ToList();

                    BuildMenu(appList, eventArgs =>
                    {
                        MessageBox.Query("Selection Changed", $"You selected '{eventArgs.Value}'.", "OK");
                    });

                    break;
                default:
                    MessageBox.Query("Selection Changed", $"You selected '{selected}'.", "OK");
                    break;
            }
        }

        private static void BuildMenu(IList items, Action<ListViewItemEventArgs> selectionChanged)
        {
            ClearMainView();

            var keyCollection = items as IEnumerable<object>;

            var lb = new ListView();
            lb.Height = Dim.Fill();
            lb.Width = Dim.Fill();
            lb.X = Pos.Left(MainWindow) + 1;
            lb.KeystrokeNavigator.Collection = keyCollection;
            lb.KeystrokeNavigator.Comparer = StringComparer.InvariantCultureIgnoreCase;
                
            lb.Source = new ListWrapper(items);
            lb.OpenSelectedItem += selectionChanged;

            MainWindow.Add(lb);

            MainWindow.FocusFirst();
        }

        private static void ClearMainView()
        {
            MainWindow.RemoveAll();
        }

        private static void AddMenu()
        {
            AppHeader = new (" Steam Achievement Manager")
            {
                X = 0,
                Y = 0,
                Height = 1,
                Width = 27,
                ColorScheme = new ()
                {
                    Normal = new (Color.Black, Color.BrightCyan)
                }
            };

            Menu = new MenuBar([
                new ("_File", [
                    new MenuItem("_Quit", "", () => { 
                        Application.RequestStop(); 
                    })
                ]),
                new ("_Manage", [
                    new MenuItem("_Achievements", "", () =>
                    {

                    })
                ])
            ])
            {
                X = 27,
                Y = 0,
                Width = Dim.Fill()
            };
        }

        private static void AddStatusBar()
        {
            StatusBar = new StatusBar()
            {
                Y = Pos.Bottom(MainWindow),
                Height = 1,
                Width = Dim.Fill(),
                Enabled = false,
                TextAlignment = TextAlignment.Right,
                ColorScheme = new()
                {
                    Disabled = Attribute.Make(Color.Black, Color.BrightGreen)
                }
            };

            var username = SteamUserManager.GetActiveUserName();
            var userId = SteamUserManager.GetActiveUser();

            UserStatusItem = new (Key.Unknown, $"{username} ({userId})", () => { });

            StatusBar.Items =
            [
                UserStatusItem
            ];

        }

        //private static void RunShell()
        //{
        //    string selection;

        //    var prompt = new SelectionPrompt<string>()
        //                 .Title("What would you like to do?")
        //                 .PageSize(10)
        //                 .AddChoiceGroup("List", [
        //                     "achievements", "apps", "stats"
        //                 ])
        //                 .AddChoices("manage", "list", "start", "help", "version", "quit");

        //    while (true)
        //    {
        //        AnsiConsole.ResetColors();

        //        AnsiConsole.Background = Color.Aqua;

        //        //var selection = AnsiConsole.Prompt();


        //        AnsiConsole.AlternateScreen(() =>
        //        {
        //            selection = AnsiConsole.Prompt(prompt);
        //        });

        //        if (selection == "quit") { }

        //        if (selection == "manage")
        //        {
        //            var appInfo = AppInfo.Create();
        //            var apps = appInfo.Apps;
        //            var options = new List<string>();

        //            var y = new Dictionary<string, List<string>>();

        //            foreach (var app in apps)
        //            {
        //                var data = app.Data;
        //                var common = data["common"];

        //                if (common == null) continue;

        //                var name = common["name"]?.ToString();
        //                var type = common["type"]?.ToString();

        //                if (string.IsNullOrEmpty(name)) continue;

        //                List<string> typeList;

        //                if (y.ContainsKey(type))
        //                {
        //                    typeList = y[type];
        //                }
        //                else
        //                {
        //                    typeList = [ ];
        //                }

        //                typeList.Add($"{name} ({app.AppID})");

        //                y[type] = typeList;

        //                options.Add($"[{type.ToUpper()}] {name} ({app.AppID})");
        //            }

        //            var json = JsonConvert.SerializeObject(y, Formatting.Indented);

        //            var z = "";
        //        }

        //        // Echo the fruit back to the terminal
        //        AnsiConsole.WriteLine($"I agree. {selection} is tasty!");
        //    }
        //}


        //private static ISupportedApp SelectApp()
        //{
        //    var prompt = new SelectionPrompt<ISupportedApp>()
        //                 .Title("Select an [cyan]app[/]")
        //                 .SearchPlaceholderText("Search...")
        //                 .EnableSearch()
        //                 .UseConverter(a => $"{a.Name} ({a.Id})");
        //}

        //private static void CreateLayout()
        //{
        //    layout = new Layout();
            
        //    const string CHECKBOX_UNCHECKED = "\ud83d\udfaa";
        //    const char CHECKBOX_CHECKED = '\u2713';

        //    layout.SplitRows(
        //        new Layout("Header")
        //            .SplitColumns(
        //               new Layout("Title").Ratio(1),
        //               new Layout("Status")
        //            ),
        //         new Layout("Div"),
        //         new Layout("Middle"),
        //         new Layout("BottomDiv"),
        //         new Layout("StatusBar"));
            
        //    layout["Header"].Size = 1;

        //    layout["Title"].Update(new Markup("[cyan] Steam Achievement Manager Console[/] [grey30]|[/] [cyan dim]v1.0.0[/]"));

        //    var isSteamRunning = !SAMHelper.IsSteamRunning();

        //    var displayText = isSteamRunning
        //        ? $"[gray dim]Status:[/] [green]{CHECKBOX_CHECKED} Steam is running[/]"
        //        : $"[gray dim]Status:[/] [yellow]{CHECKBOX_UNCHECKED}  Steam is NOT running[/]";

        //    layout["Status"].Update(new Markup(displayText).RightJustified());
            
        //    var rule = new Rule();
        //    rule.RuleStyle("gray dim");

        //    layout["Div"].Size = 1;
        //    layout["Div"].Update(rule);
            
        //    layout["BottomDiv"].Size = 1;
        //    layout["BottomDiv"].Update(rule);
            
        //    var userId = SteamUserManager.GetActiveUser();
        //    var userName = SteamUserManager.GetActiveUserName();

        //    layout["StatusBar"].Size = 1;
        //    layout["StatusBar"].Update(new Markup($"[cyan2]{userName}[/] [gray]({userId})[/]").RightJustified());

        //    layout["Middle"].Update(new Panel(new Text("")).Expand().NoBorder());

        //    //layout["LeftBottom"].Update(
        //    //                            new Panel("[blink]PRESS ANY KEY TO QUIT[/]")
        //    //                                .Expand()
        //    //                                .BorderColor(Color.Yellow)
        //    //                                .Padding(0, 0));

        //    //layout["Right"].Update(
        //    //                       new Panel(
        //    //                                 new Table()
        //    //                                     .AddColumns("[blue]Qux[/]", "[green]Corgi[/]")
        //    //                                     .AddRow("9", "8")
        //    //                                     .AddRow("7", "6")
        //    //                                     .Expand())
        //    //                           .Header("A [yellow]Table[/] in a [blue]Panel[/] (Ratio=2)")
        //    //                           .Expand());

        //    //layout["RightRight"].Update(
        //    //                            new Panel("Explicit-size-is-[yellow]3[/]")
        //    //                                .BorderColor(Color.Yellow)
        //    //                                .Padding(0, 0));

        //    //layout["Bottom"].Update(
        //    //                        new Panel(
        //    //                                  new FigletText("Hello World"))
        //    //                            .Header("Some [green]Figlet[/] text")
        //    //                            .Expand());
        //}
    }
}
