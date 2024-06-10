using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using SAM.API;
using SAM.Core.Interfaces;
using ValveKeyValue;

namespace SAM.Core.AppInfo;

public class AppInfoManager
{
    private const string FILE_NAME = @"appinfo.vdf";
    private const string APPCACHE = @"appcache";
    private const uint Magic28 = 0x07_56_44_28;
    private const uint Magic = 0x07_56_44_27;

    private static readonly object syncLock = new ();
    private static AppInfoManager _instance;

    public EUniverse Universe { get; set; }
    public List<IAppInfo> Apps { get; set; } = [ ];
    public Dictionary<AppInfoType, List<IAppInfo>> Categories { get; set; } = [ ];
    
    public List<IAppInfo> Tool => Categories[AppInfoType.Tool];
    public List<IAppInfo> Game => Categories[AppInfoType.Game];
    public List<IAppInfo> Demo => Categories[AppInfoType.Demo];
    public List<IAppInfo> DLC => Categories[AppInfoType.DLC];
    public List<IAppInfo> Application => Categories[AppInfoType.Application];
    public List<IAppInfo> Config => Categories[AppInfoType.Config];
    public List<IAppInfo> Music => Categories[AppInfoType.Music];
    public List<IAppInfo> Video => Categories[AppInfoType.Video];
    public List<IAppInfo> Beta => Categories[AppInfoType.Beta];
    public List<IAppInfo> Unknown => Categories[AppInfoType.Unknown];

    public AppInfoManager()
    {
        var installPath = Steam.GetInstallPath();
        var appInfoVdfPath = Path.Join(installPath, APPCACHE, FILE_NAME);

        if (!File.Exists(appInfoVdfPath)) throw new FileNotFoundException($"The '{FILE_NAME}' does not exist.", FILE_NAME);
        
        Init();

        Read(appInfoVdfPath);
    }

    public AppInfoManager([NotNull] string file)
    {
        if (string.IsNullOrEmpty(file)) throw new ArgumentNullException(nameof(file));
        if (!File.Exists(file)) throw new FileNotFoundException($"File '{file}' does not exist.", file);

        Init();

        Read(file);
    }

    public static AppInfoManager Default
    {
        get
        {
            if (_instance != null) return _instance;

            lock (syncLock)
            {
                _instance = new ();
            }

            return _instance;
        }
    }

    /// <summary>
    /// Opens and reads the given filename.
    /// </summary>
    /// <param name="filename">The file to open and read.</param>
    public void Read(string filename)
    {
        using var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        Read(fs);
    }

    /// <summary>
    /// Reads the given <see cref="Stream"/>.
    /// </summary>
    /// <param name="input">The input <see cref="Stream"/> to read from.</param>
    public void Read(Stream input)
    {
        using var reader = new BinaryReader(input);
        var magic = reader.ReadUInt32();

        if (magic is not Magic and not Magic28)
        {
            throw new InvalidDataException($"Unknown magic header: {magic:X}");
        }
        Universe = (EUniverse) reader.ReadUInt32();

        var deserializer = KVSerializer.Create(KVSerializationFormat.KeyValues1Binary);

        while (true)
        {
            var appid = reader.ReadUInt32();

            if (appid == 0)
            {
                break;
            }

            reader.ReadUInt32(); // size until the end of Data

            var app = new AppInfo
            {
                AppId = appid,
                InfoState = reader.ReadUInt32(),
                LastUpdated = DateTimeOffset.FromUnixTimeSeconds(reader.ReadUInt32()).DateTime,
                Token = reader.ReadUInt64(),
                Hash = new (reader.ReadBytes(20)),
                ChangeNumber = reader.ReadUInt32(),
            };

            if (magic == Magic28)
            {
                app.BinaryDataHash = new (reader.ReadBytes(20));
            }

            app.Data = deserializer.Deserialize(input);

            var cat = app.AppInfoType;

            Categories[cat].Add(app);

            Apps.Add(app);
        }
    }

    private void Init()
    {
        var categories = Enum.GetValues<AppInfoType>();

        foreach (var cat in categories)
        {
            Categories[cat] = [ ];
        }
    }
}
