#pragma warning disable CA1707

using SAM.Core;
using Xunit;
using Xunit.Abstractions;

namespace SAM.UnitTests;

public class SteamworksManagerTests (ITestOutputHelper testOutputHelper)
{
	private readonly ITestOutputHelper _output = testOutputHelper;

	[Theory(DisplayName = "Steamworks App")]
	[InlineData(287290)]
	[InlineData(254700)]
	[InlineData(952060)]
	public void SteamworksManager_GetAppInfo_Succeeds (uint appId)
	{
		var appData = SteamworksManager.GetAppInfo(appId);

		Assert.NotNull(appData);
		Assert.NotEmpty(appData.Name);

		_output.WriteLine($"App {appId} is '{appData.Name}'.");
	}

	[Theory(DisplayName = "Steamworks App (w/ DLC)")]
	[InlineData(287290)]
	[InlineData(952060)]
	public void SteamworksManager_GetAppInfo_WithDLC_Succeeds (uint appId)
	{
		var appData = SteamworksManager.GetAppInfo(appId, true);

		Assert.NotNull(appData);
		Assert.NotEmpty(appData.Name);
		Assert.NotEmpty(appData.DlcInfo);
	}

	[Theory(DisplayName = "Steamworks App (w/o DLC)")]
	[InlineData(254700)]
	public void SteamworksManager_GetAppInfo_NoDLC_Succeeds (uint appId)
	{
		var appData = SteamworksManager.GetAppInfo(appId, true);

		Assert.NotNull(appData);
		Assert.NotEmpty(appData.Name);
		Assert.Empty(appData.DlcInfo);
	}

	[Fact(DisplayName = "Steamworks App List")]
	public void SteamworksManager_GetAppList_NotEmpty ()
	{
		var apps = SteamworksManager.GetAppList();

		Assert.NotEmpty(apps);
	}
}
