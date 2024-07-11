using System;
using System.ComponentModel.DataAnnotations;
using DevExpress.Mvvm;
using DevExpress.Mvvm.CodeGenerators;
using log4net;
using Newtonsoft.Json;
using SAM.Core;
using SAM.Core.Messages;
using SAM.Core.Storage;
using SAM.Extensions;

namespace SAM;

[GenerateViewModel(ImplementISupportParentViewModel = true)]
[MetadataType(typeof(LibraryGridSettingsMetaData))]
public partial class LibraryGridSettings : ILibrarySettings
{
    private const string NAME = "libraryGridSettings.json";
    
    private static readonly ILog log = LogManager.GetLogger(typeof(LibraryGridSettings));
    private static readonly CacheKey cacheKey = new (NAME, CacheKeyType.Settings);

    [GenerateProperty(OnChangedMethod = nameof(Changed))]
    private bool _showInstallPath;
    
    [GenerateProperty(OnChangedMethod = nameof(Changed))]
    private bool _showHidden;

    [GenerateProperty(OnChangedMethod = nameof(Changed))]
    private bool _showFavoritesOnly;

    [GenerateProperty(OnChangedMethod = nameof(Changed))]
    private bool _enableGrouping = true;
    
    [JsonIgnore]
    public bool Loaded { get; private set; }

    [JsonConstructor]
    private LibraryGridSettings()
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
        Messenger.Default.SendAction(EntityType.HomeSettings, ActionType.Changed);
    }

    public static LibraryGridSettings Load()
    {
        try
        {
            var settings = new LibraryGridSettings();
            CacheManager.TryPopulateObject(cacheKey, settings);
            settings.Loaded = true;
            return settings;
        }
        catch (Exception e)
        {
            log.Error($"An error occurred attempting to load the {nameof(LibraryGridSettings)}. {e.Message}", e);
            throw;
        }
    }

    public void Save()
    {
        try
        {
            CacheManager.CacheObject(cacheKey, this);

            log.Info($"Saved {nameof(LibraryGridSettings)}.");
        }
        catch (Exception e)
        {
            log.Error($"An error occurred attempting to save the {nameof(LibraryGridSettings)}. {e.Message}", e);
        }
    }
}

public class LibraryGridSettingsMetaData
{
    [JsonProperty]
    public bool ShowInstallPath { get; set; }
    [JsonProperty]
    public bool ShowHidden { get; set; }
    [JsonProperty]
    public bool ShowFavoritesOnly { get; set; }
    [JsonProperty]
    public bool EnableGrouping { get; set; }
}
