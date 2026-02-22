using System;
using System.Collections.Generic;
using System.Text;
using Windows.ApplicationModel.Activation;

namespace SAM.Serialization;

[Serializable]
public class SerializableProtocolActivatedEventArgs : SerializableActivatedEventArgs, IProtocolActivatedEventArgs
{
    public Uri Uri { get; set; }
}
