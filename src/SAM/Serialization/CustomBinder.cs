using System;
using System.Runtime.Serialization;
using Windows.ApplicationModel.Activation;
using Windows.Storage;

namespace SAM.Serialization;

public class CustomBinder : SerializationBinder
{
    public override Type BindToType(string assemblyName, string typeName)
    {
        if (typeName == typeof(ActivationKind).FullName)
            return typeof(ActivationKind);

        if (typeName == typeof(ApplicationExecutionState).FullName)
            return typeof(ApplicationExecutionState);

        if (typeName == typeof(FileAttributes).FullName)
            return typeof(FileAttributes);

        return Type.GetType(typeName);
    }
}
