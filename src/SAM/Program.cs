using System;
using System.IO.Pipes;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Windows;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Newtonsoft.Json;
using SAM.Serialization;
using PipeStream = SAM.Serialization.PipeStream;
using SplashScreen = Windows.ApplicationModel.Activation.SplashScreen;

namespace SAM;

public partial class Program
{
    [Serializable]
    public class SerializableProtocolActivatedEventArgs : SerializableActivatedEventArgs, IProtocolActivatedEventArgs
    {
        public Uri Uri { get; set; }
    }

    private const string APP_ID = @"70c22004-1764-45ec-aec2-077c50465935";
    private const string NAMED_PIPE_SERVER_NAME = @".";
    private static readonly int connectionTimeout = TimeSpan.FromSeconds(3).Milliseconds;
    //private static readonly BinaryFormatter formatter = new BinaryFormatter { Binder = new CustomBinder() };
    private static readonly JsonSerializer formatter = new ();
    private static Encoding unicodeEncoding = Encoding.Unicode;

    private static readonly JsonSerializerSettings settings = new ()
    {
        Formatting = Formatting.None,
        TypeNameHandling = TypeNameHandling.Objects,
        StringEscapeHandling = StringEscapeHandling.EscapeNonAscii
    };

    private static App _app;

    [STAThread]
    public static void Main(string[] args)
    {
        var activatedEventArgs = AppInstance.GetActivatedEventArgs();
        //var app = new App();
        //app.InitializeComponent();
        //app.OnActivated(AppInstance.GetActivatedEventArgs());

        using var mutex = new Mutex(false, APP_ID);

        if (mutex.WaitOne(0, false))
        {
            var pipeThread = new Thread(CreateNamedPipeServer) { IsBackground = true };
            pipeThread.Start();

            var app = new App();
            app.InitializeComponent();

            if (activatedEventArgs != null)
                app.OnProtocolActivated(activatedEventArgs);

            app.Run();
        }
        else if (activatedEventArgs != null)
        {
            // another instance is already running
            using var pipeClient = new NamedPipeClientStream(NAMED_PIPE_SERVER_NAME, APP_ID, PipeDirection.Out);

            try
            {
                pipeClient.Connect(connectionTimeout);

                var serializedArgs = JsonConvert.SerializeObject(activatedEventArgs, settings);

                var writer = new PipeStream(pipeClient);
                writer.WriteString(serializedArgs);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }

    private static void CreateNamedPipeServer()
    {
        using var pipeServer = new NamedPipeServerStream(APP_ID, PipeDirection.In, 1, PipeTransmissionMode.Message);

        while (true)
        {
            pipeServer.WaitForConnection();
            IActivatedEventArgs activatedEventArgs;

            do
            {
                var reader = new PipeStream(pipeServer);
                var text = reader.ReadString();
                activatedEventArgs = JsonConvert.DeserializeObject<IActivatedEventArgs>(text);
            }
            while (!pipeServer.IsMessageComplete);

            if (activatedEventArgs != null)
                _app.Dispatcher
                    .BeginInvoke(new Action(() => _app.OnProtocolActivated(activatedEventArgs)));

            pipeServer.Disconnect();
        }
    }
