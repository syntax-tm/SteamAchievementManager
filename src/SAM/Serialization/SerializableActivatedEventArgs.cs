using System;
using Windows.ApplicationModel.Activation;

namespace SAM.Serialization;

[Serializable]
public class SerializableActivatedEventArgs : IActivatedEventArgs
{
    public ActivationKind Kind { get; set; }

    public ApplicationExecutionState PreviousExecutionState { get; set; }

    [NonSerialized]
    private Windows.ApplicationModel.Activation.SplashScreen _splashScreen;
    public Windows.ApplicationModel.Activation.SplashScreen SplashScreen
    {
        get => _splashScreen;
        set => _splashScreen = value;
    }
}
