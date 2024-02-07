using System.Windows.Media;
using System.Xml;
using DevExpress.Mvvm.CodeGenerators;
using log4net;
using SAM.Core.Extensions;

namespace SAM.Core
{
	// TODO: add support for any steam user (currently only supports the current steam user)
	[GenerateViewModel]
	public partial class SteamUser
	{
		private const string PROFILE_URL_FORMAT = @"https://steamcommunity.com/profiles/{0}";
		private const string PROFILE_XML_URL_FORMAT = @"https://steamcommunity.com/profiles/{0}?xml=1";

		private static readonly ILog log = LogManager.GetLogger(nameof(SteamUser));

		[GenerateProperty] private ulong steamId64;
		[GenerateProperty] private string steamId;
		[GenerateProperty] private int playerLevel;
		[GenerateProperty] private string customUrl;
		[GenerateProperty] private decimal recentHoursPlayed;
		[GenerateProperty] private string headline;
		[GenerateProperty] private string location;
		[GenerateProperty] private string displayLocation;
		[GenerateProperty] private string memberSince;
		[GenerateProperty] private string realName;
		[GenerateProperty] private string profileUrl;
		[GenerateProperty] private string avatarIcon;
		[GenerateProperty] private string avatarMedium;
		[GenerateProperty] private string avatarFull;
		[GenerateProperty] private bool vacBanned;
		[GenerateProperty] private bool isLimitedAccount;

		[GenerateProperty] private ImageSource avatar;

		public SteamUser ()
		{
			RefreshProfile();
		}

		private void RefreshProfile ()
		{
			var client = SteamClientManager.Default;

			SteamId64 = client.SteamUser.GetSteamId();

			// TODO: need a way to get player level for friends
			PlayerLevel = client.SteamUser.GetPlayerSteamLevel();

			ProfileUrl = string.Format(PROFILE_URL_FORMAT, SteamId64);

			var xmlFeedUrl = string.Format(PROFILE_XML_URL_FORMAT, SteamId64);

			using var reader = new XmlTextReader(xmlFeedUrl);
			var doc = new XmlDocument();
			doc.Load(reader);

			SteamId = doc.GetValue(@"//steamID");

			AvatarIcon = doc.GetValue(@"//avatarIcon");
			AvatarMedium = doc.GetValue(@"//avatarMedium");
			AvatarFull = doc.GetValue(@"//avatarFull");

			Avatar = ImageHelper.CreateSource(AvatarFull);

			CustomUrl = doc.GetValue(@"//customUrl");
			MemberSince = doc.GetValue(@"//memberSince");
			Headline = doc.GetValue(@"//Headline");
			Location = doc.GetValue(@"//location");

			DisplayLocation = !string.IsNullOrEmpty(Location) ? LocationHelper.GetShortLocation(Location) : string.Empty;

			RealName = doc.GetValue(@"//realname");

			// TODO: no idea what this is for since mine and everyone i checked was empty
			//var steamRating = GetValue(doc, @"//steamRating");

			VacBanned = doc.GetValueAsBool(@"//vacBanned");
			IsLimitedAccount = doc.GetValueAsBool(@"//isLimitedAccount");
			RecentHoursPlayed = doc.GetValueAsDecimal(@"//hoursPlayed2Wk");

			log.Debug($"Finished loading steam user {SteamId} ({SteamId64}) user profile.");
		}
	}
}
