using System;
using System.ComponentModel.DataAnnotations;
using DevExpress.Mvvm;
using DevExpress.Mvvm.CodeGenerators;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SAM.Core.Messages;
using SAM.Core.Storage;
using SAM.Core;
using SAM.Extensions;

namespace SAM;

[GenerateViewModel(ImplementISupportParentViewModel = true)]
[MetadataType(typeof(LibraryTileSettingsMetaData))]
public partial class LibraryTileSettings : ILibrarySettings
{
    private const string NAME = "libraryTileSettings.json";
    
    private static readonly ILog log = LogManager.GetLogger(typeof(LibraryTileSettings));
    private static readonly CacheKey cacheKey = new (NAME, CacheKeyType.Settings);

    [GenerateProperty(OnChangedMethod = nameof(Changed))]
    private bool _showInstallPath;
    
    [GenerateProperty(OnChangedMethod = nameof(Changed))]
    private bool _showHidden;

    [GenerateProperty(OnChangedMethod = nameof(Changed))]
    private bool _showFavoritesOnly;

    [GenerateProperty(OnChangedMethod = nameof(Changed))]
    private bool _enableGrouping = true;
    
    [GenerateProperty(OnChangedMethod = nameof(Changed))]
    private bool _showImages = true;

    [GenerateProperty(OnChangedMethod = nameof(Changed))]
    private bool _localImagesOnly = true;

    [GenerateProperty(OnChangedMethod = nameof(Changed))]
    private TileSize _tileSize = TileSize.M;

    [JsonIgnore]
    public bool Loaded { get; private set; }
    
    [JsonConstructor]
    private LibraryTileSettings()
    {

    }

    [GenerateCommand]
    public void ToggleShowHidden()
    {
        ShowHidden = !ShowHidden;
    }

    [GenerateCommand]
    public void ToggleShowFavoritesOnly()
    {
        ShowFavoritesOnly = !ShowFavoritesOnly;
    }

    [GenerateCommand]
    public void ToggleGrouping()
    {
        EnableGrouping = !EnableGrouping;
    }

    protected virtual void Changed()
    {
        if (!Loaded) return;

        Messenger.Default.SendAction(EntityType.HomeSettings, ActionType.Changed);
    }

    public static LibraryTileSettings Load()
    {
        try
        {
            var settings = new LibraryTileSettings();
            CacheManager.TryPopulateObject(cacheKey, settings);
            settings.Loaded = true;
            return settings;
        }
        catch (Exception e)
        {
            log.Error($"An error occurred attempting to load the {nameof(LibraryTileSettings)}. {e.Message}", e);
            throw;
        }
    }

    public void Save()
    {
        try
        {
            CacheManager.CacheObject(cacheKey, this);

            log.Info($"Saved {nameof(LibraryTileSettings)}.");
        }
        catch (Exception e)
        {
            log.Error($"An error occurred attempting to save the {nameof(LibraryTileSettings)}. {e.Message}", e);
        }
    }
}

public class LibraryTileSettingsMetaData
{
    [JsonProperty]
    public bool ShowImages { get; set; }

    [JsonProperty]
    public bool LocalImagesOnly { get; set; }

    [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
    public TileSize TileSize { get; set; }

    [JsonProperty]
    public bool ShowHidden { get; set; }

    [JsonProperty]
    public bool ShowFavoritesOnly { get; set; }

    [JsonProperty]
    public bool EnableGrouping { get; set; }
}
