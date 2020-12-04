using SAM.WPF.Core.API;
using SAM.WPF.Core.API.Steam;
using Xunit;
using Xunit.Abstractions;

namespace SAM.UnitTests
{
    public class SteamworksManagerTests
    {
        private readonly ITestOutputHelper _output;

        public SteamworksManagerTests(ITestOutputHelper testOutputHelper)
        {
            _output = testOutputHelper;
        }

        [Theory(DisplayName = "Steamworks App")]
        [InlineData(287290)]
        [InlineData(254700)]
        [InlineData(952060)]
        public void TestApp(uint appId)
        {
            var appData = SteamworksManager.GetAppInfo(appId);

            Assert.NotNull(appData);
            Assert.NotEmpty(appData.Name);

            _output.WriteLine($"App {appId} is '{appData.Name}'.");
        }

        [Theory(DisplayName = "Steamworks App (w/ DLC)")]
        [InlineData(287290)]
        [InlineData(952060)]
        public void TestAppWithDlc(uint appId)
        {
            var appData = SteamworksManager.GetAppInfo(appId, true);

            Assert.NotNull(appData);
            Assert.NotEmpty(appData.Name);
            Assert.NotEmpty(appData.DlcInfo);
        }

        [Theory(DisplayName = "Steamworks App (w/o DLC)")]
        [InlineData(254700)]
        public void TestAppWithNoDlc(uint appId)
        {
            var appData = SteamworksManager.GetAppInfo(appId, true);

            Assert.NotNull(appData);
            Assert.NotEmpty(appData.Name);
            Assert.Empty(appData.DlcInfo);
        }

        [Fact(DisplayName = "Steamworks App List")]
        public void TestAppList()
        {
            var apps = SteamworksManager.GetAppList();

            Assert.NotEmpty(apps);
        }
    }
}
