using System;
using DevExpress.Mvvm;
using DevExpress.Mvvm.CodeGenerators;
using log4net;
using Newtonsoft.Json;
using SAM.Core;
using SAM.Core.Storage;

namespace SAM;

[GenerateViewModel]
public partial class HomeSettings : BindableBase
{
    private const string NAME = "homeSettings.json";
    
    private static readonly ILog log = LogManager.GetLogger(typeof(HomeSettings));
    private static readonly CacheKey cacheKey = new (NAME, CacheKeyType.Settings);

    [GenerateProperty(OnChangedMethod=nameof(Save))] private LibraryView _view;
    [GenerateProperty] private LibraryGridSettings _gridSettings = new ();
    [GenerateProperty] private LibraryTileSettings _tileSettings = new ();

    [JsonIgnore]
    public bool IsGridView
    {
        get => GetProperty(() => IsGridView);
        set => SetProperty(() => IsGridView, value);
    }
    
    [JsonIgnore]
    public bool IsTileView
    {
        get => GetProperty(() => IsTileView);
        set => SetProperty(() => IsTileView, value);
    }

    public static HomeSettings Load()
    {
        try
        {
            var settings = new HomeSettings();
            CacheManager.TryPopulateObject(cacheKey, settings);
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
        try
        {
            CacheManager.CacheObject(cacheKey, this);
        }
        catch (Exception e)
        {
            log.Error($"An error occurred attempting to save the {nameof(HomeSettings)}. {e.Message}", e);
        }
    }

}
