using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Markup;

[assembly: ComVisible(false)]

[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config", Watch = false)]

[assembly: XmlnsPrefix(@"urn:sam", "sam")]
[assembly: XmlnsDefinition(@"urn:sam", "SAM")]
[assembly: XmlnsDefinition(@"urn:sam", "SAM.Behaviors")]
[assembly: XmlnsDefinition(@"urn:sam", "SAM.Converters")]
[assembly: XmlnsDefinition(@"urn:sam", "SAM.Controls")]
[assembly: XmlnsDefinition(@"urn:sam", "SAM.Resources")]
[assembly: XmlnsDefinition(@"urn:sam", "SAM.Settings")]
[assembly: XmlnsDefinition(@"urn:sam", "SAM.Stats")]
[assembly: XmlnsDefinition(@"urn:sam", "SAM.Views")]
[assembly: XmlnsDefinition(@"urn:sam", "SAM.ViewModels")]
