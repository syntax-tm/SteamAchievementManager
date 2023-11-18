using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Markup;

[assembly: ComVisible(false)]

[assembly: Guid("bda4a1c9-8b64-447f-8b99-3b72c1feeb44")]

[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config", Watch = false)]

[assembly: XmlnsPrefix(@"urn:sam", "sam")]
[assembly: XmlnsDefinition(@"urn:sam", "SAM.Core")]
[assembly: XmlnsDefinition(@"urn:sam", "SAM.Core.Behaviors")]
[assembly: XmlnsDefinition(@"urn:sam", "SAM.Core.Converters")]
[assembly: XmlnsDefinition(@"urn:sam", "SAM.Core.Controls")]
[assembly: XmlnsDefinition(@"urn:sam", "SAM.Core.Resources")]
[assembly: XmlnsDefinition(@"urn:sam", "SAM.Core.Settings")]
[assembly: XmlnsDefinition(@"urn:sam", "SAM.Core.Stats")]
[assembly: XmlnsDefinition(@"urn:sam", "SAM.Core.Views")]
[assembly: XmlnsDefinition(@"urn:sam", "SAM.Core.ViewModels")]

