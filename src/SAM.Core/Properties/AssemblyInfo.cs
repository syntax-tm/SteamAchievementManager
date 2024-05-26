using System.Runtime.InteropServices;
using System.Windows.Markup;

[assembly: ComVisible(false)]

[assembly: Guid("bda4a1c9-8b64-447f-8b99-3b72c1feeb44")]

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config", Watch = false)]

[assembly: XmlnsPrefix(@"urn:sam.core", "core")]
[assembly: XmlnsDefinition(@"urn:sam.core", "SAM.Core")]
[assembly: XmlnsDefinition(@"urn:sam.core", "SAM.Core.Converters")]
[assembly: XmlnsDefinition(@"urn:sam.core", "SAM.Core.Settings")]
[assembly: XmlnsDefinition(@"urn:sam.core", "SAM.Core.Stats")]
