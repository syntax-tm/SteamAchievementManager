#pragma warning disable CA1305

using System;
using System.ComponentModel;
using System.Windows.Data;
using DevExpress.Mvvm.CodeGenerators;
using log4net;
using SAM.Core.Converters;
using SAM.Core.Extensions;

namespace SAM.Core.ViewModels;

[GenerateViewModel]
public partial class HomeViewModel
{
	// ReSharper disable once InconsistentNaming
	protected readonly ILog Log = LogManager.GetLogger(nameof(HomeViewModel));

	private CollectionViewSource _itemsViewSource;
	private bool _loading = true;

	[GenerateProperty] private bool enableGrouping;
	[GenerateProperty] private string filterText;
	[GenerateProperty] private string filterNormal;
	[GenerateProperty] private string filterDemos;
	[GenerateProperty] private string filterMods;
	[GenerateProperty] private bool filterJunk;
	[GenerateProperty] private bool showHidden;
	[GenerateProperty] private bool filterFavorites;
	[GenerateProperty] private string filterTool;
	[GenerateProperty] private int tileWidth = 100;
	[GenerateProperty] private ICollectionView itemsView;

	public SteamApp SelectedItem
	{
		get => (SteamApp) ItemsView!.CurrentItem;
		set => ItemsView!.MoveCurrentTo(value);
	}
	[GenerateProperty] private SteamLibrary library;

	public HomeViewModel ()
	{
		Refresh();
	}

	private void ItemsViewSourceOnFilter (object sender, FilterEventArgs e)
	{
		if (e.Item is not SteamApp app)
			{
			throw new ArgumentException(nameof(e.Item));
		}

		var hasNameFilter = !string.IsNullOrWhiteSpace(FilterText);
		var isNameMatch = !hasNameFilter || app.Name.ContainsIgnoreCase(FilterText) || app.Id.ToString().Contains(FilterText);
		var isJunkFiltered = !FilterJunk || app.IsJunk;
		var isHiddenFiltered = ShowHidden || !app.IsHidden;
		var isNonFavoriteFiltered = !FilterFavorites || app.IsFavorite;

		e.Accepted = isNameMatch && isJunkFiltered && isHiddenFiltered && isNonFavoriteFiltered;
	}

	[GenerateCommand]
	public static void Loaded ()
	{
	}

	[GenerateCommand]
	public void Refresh (bool force = false)
	{
		_loading = true;

		if (force)
		{
			SteamLibraryManager.DefaultLibrary.Refresh();
		}

		Library = SteamLibraryManager.DefaultLibrary;

		_itemsViewSource = new()
		{
			Source = Library.Items
		};
		ItemsView = _itemsViewSource.View;

		using (_itemsViewSource.DeferRefresh())
		{
			_itemsViewSource.Filter += ItemsViewSourceOnFilter;

			_itemsViewSource.SortDescriptions.Clear();
			_itemsViewSource.SortDescriptions.Add(new(nameof(SteamApp.Name), ListSortDirection.Ascending));

			_itemsViewSource.LiveFilteringProperties.Add(nameof(SteamApp.IsHidden));
			_itemsViewSource.LiveFilteringProperties.Add(nameof(SteamApp.IsFavorite));

			_itemsViewSource.IsLiveFilteringRequested = true;
			_itemsViewSource.IsLiveSortingRequested = true;
			_itemsViewSource.IsLiveGroupingRequested = false;
		}

		_loading = false;
	}

	protected void OnFilterTextChanged ()
	{
		if (_loading)
			{
			return;
		}

		ItemsView!.Refresh();
	}

	protected void OnEnableGroupingChanged ()
	{
		if (_loading)
			{
			return;
		}

		using (_itemsViewSource.DeferRefresh())
		{
			_itemsViewSource.GroupDescriptions.Clear();
			_itemsViewSource.IsLiveGroupingRequested = EnableGrouping;

			if (EnableGrouping)
			{
				ItemsView!.GroupDescriptions.Add(new PropertyGroupDescription(nameof(SteamApp.Name), new StringToGroupConverter()));
			}
		}
	}
}
