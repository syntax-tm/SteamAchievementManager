using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using log4net;
using SAM.Core.Storage;

namespace SAM.Core
{
	public static class SteamCdnHelper
	{
		// TODO: Move this to a separate factory or builder class (or rename this)
		private const string GAME_CLIENT_ICON_URI = "https://cdn.cloudflare.steamstatic.com/steamcommunity/public/images/apps/{0}/{1}.ico";
		private const string GAME_ICON_URI = "https://cdn.cloudflare.steamstatic.com/steamcommunity/public/images/apps/{0}/{1}.jpg";
		private const string GAME_LOGO_URI = "https://cdn.cloudflare.steamstatic.com/steamcommunity/public/images/apps/{0}/{1}.jpg";
		private const string GAME_HEADER_URI = "https://cdn.cloudflare.steamstatic.com/steam/apps/{0}/header.jpg";
		private const string GAME_LIBRARY_HERO_URI = "https://cdn.cloudflare.steamstatic.com/steam/apps/{0}/library_hero.jpg";
		private const string GAME_SMALL_CAPSULE_URI = "https://cdn.cloudflare.steamstatic.com/steam/apps/{0}/capsule_231x87.jpg";
		private const string GAME_MEDIUM_CAPSULE_URI = "https://cdn.cloudflare.steamstatic.com/steam/apps/{0}/capsule_467x181.jpg";
		private const string GAME_LARGE_CAPSULE_URI = "https://cdn.cloudflare.steamstatic.com/steam/apps/{0}/capsule_616x353.jpg";
		private const string GAME_ACHIEVEMENT_URI = "http://steamcdn-a.akamaihd.net/steamcommunity/public/images/apps/{0}/{1}";

		private static readonly ILog log = LogManager.GetLogger(nameof(SteamCdnHelper));

		public static Image DownloadImage (uint id, SteamImageType type, string file = null)
		{
			try
			{
				var url = type switch
				{
					SteamImageType.ClientIcon => string.Format(GAME_CLIENT_ICON_URI, id, file),
					SteamImageType.Icon => string.Format(GAME_ICON_URI, id, file),
					SteamImageType.Logo => string.Format(GAME_LOGO_URI, id, file),
					SteamImageType.Header => string.Format(GAME_HEADER_URI, id),
					SteamImageType.LibraryHero => string.Format(GAME_LIBRARY_HERO_URI, id),
					SteamImageType.SmallCapsule => string.Format(GAME_SMALL_CAPSULE_URI, id),
					SteamImageType.MediumCapsule => string.Format(GAME_MEDIUM_CAPSULE_URI, id),
					SteamImageType.LargeCapsule => string.Format(GAME_LARGE_CAPSULE_URI, id),
					SteamImageType.AchievementIcon => string.Format(GAME_ACHIEVEMENT_URI, id, file),
					_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
				};

				var fileName = Path.GetFileName(url);
				var cacheKey = CacheKeys.CreateAppImageCacheKey(id, fileName);

				var img = Task.Run(() => WebManager.DownloadImageAsync(url, cacheKey));

				return img.Result;
			}
			catch (Exception e)
			{
				log.Error($"An error occurred downloading the {type} image for app id '{id}'.", e);

				throw;
			}
		}
	}
}
