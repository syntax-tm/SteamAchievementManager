using System;
using System.ComponentModel.DataAnnotations;
using DevExpress.Mvvm;
using DevExpress.Mvvm.CodeGenerators;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SAM.Core;
using SAM.Core.Storage;

namespace SAM;

[GenerateViewModel]
[MetadataType(typeof(HomeSettingsMetaData))]
public partial class HomeSettings : BindableBase
{
    private const string NAME = "homeSettings.json";
    
    private static readonly ILog log = LogManager.GetLogger(typeof(HomeSettings));
    private static readonly CacheKey cacheKey = new (NAME, CacheKeyType.Settings);

    [GenerateProperty(OnChangedMethod=nameof(Save))]
    private LibraryView _view;

    [GenerateProperty]
    private LibraryGridSettings _gridSettings;

    [GenerateProperty]
    private LibraryTileSettings _tileSettings;

    public bool IsGridView
    {
        get => GetProperty(() => IsGridView);
        set => SetProperty(() => IsGridView, value);
    }
    
    public bool IsTileView
    {
        get => GetProperty(() => IsTileView);
        set => SetProperty(() => IsTileView, value);
    }
    
    [JsonIgnore]
    public bool Loaded { get; private set; }

    private HomeSettings()
    {

    }

    public static HomeSettings Load()
    {
        try
        {
            // TODO: this is ugly but unfortunately the partial classes don't play well with serialization
            var settings = new HomeSettings();
            CacheManager.TryPopulateObject(cacheKey, settings);
            settings.GridSettings = LibraryGridSettings.Load();
            settings.TileSettings = LibraryTileSettings.Load();
            settings.Loaded = true;
            return settings;
        }
        catch (Exception e)
        {
            log.Error($"An error occurred attempting to load the {nameof(HomeSettings)}. {e.Message}", e);
            throw;
        }
    }

    public void Save()
    {
        if (!Loaded) return;

        try
        {
            CacheManager.CacheObject(cacheKey, this);

            TileSettings!.Save();
            GridSettings!.Save();

            log.Info($"Saved {nameof(HomeSettings)}.");
        }
        catch (Exception e)
        {
            log.Error($"An error occurred attempting to save the {nameof(HomeSettings)}. {e.Message}", e);
        }
    }
}

public class HomeSettingsMetaData
{
    [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
    public LibraryView View { get; set; }

    [JsonProperty]
    public LibraryGridSettings GridSettings { get; set; }

    [JsonProperty]
    public LibraryTileSettings TileSettings { get; set; }
    
    [JsonIgnore]
    public bool IsGridView { get; set; }
    
    [JsonIgnore]
    public bool IsTileView { get; set; }
}

