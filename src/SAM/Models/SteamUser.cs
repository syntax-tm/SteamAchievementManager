using System;
using System.Drawing;
using System.Windows.Media;
using System.Xml;
using DevExpress.Mvvm.CodeGenerators;
using log4net;
using SAM.API;
using SAM.Core;
using SAM.Core.Extensions;

namespace SAM;

// TODO: add support for any steam user (currently only supports the current steam user)
[GenerateViewModel]
public partial class SteamUser
{
    private const string PROFILE_URL_FORMAT = @"https://steamcommunity.com/profiles/{0}";
    private const string PROFILE_XML_URL_FORMAT = @"https://steamcommunity.com/profiles/{0}?xml=1";

    private static readonly ILog log = LogManager.GetLogger(nameof(SteamUser));
    private readonly Client _client;
    [GenerateProperty] private ulong _steamId64;
    [GenerateProperty] private string _steamId;
    [GenerateProperty] private int _playerLevel;
    [GenerateProperty] private string _customUrl;
    [GenerateProperty] private decimal _recentHoursPlayed;
    [GenerateProperty] private string _headline;
    [GenerateProperty] private string _location;
    [GenerateProperty] private string _displayLocation;
    [GenerateProperty] private string _memberSince;
    [GenerateProperty] private string _realName;
    [GenerateProperty] private string _profileUrl;
    [GenerateProperty] private string _avatarIcon;
    [GenerateProperty] private string _avatarMedium;
    [GenerateProperty] private string _avatarFull;
    [GenerateProperty] private bool _vacBanned;
    [GenerateProperty] private bool _isLimitedAccount;

    [GenerateProperty] private ImageSource _avatar;

    public SteamUser(Client client)
    {
        _client = client;

        RefreshProfile();
    }

    private void RefreshProfile()
    {
        try
        {
            SteamId64 = _client.SteamUser.GetSteamId();

            // TODO: need a way to get player level for friends
            PlayerLevel = _client.SteamUser.GetPlayerSteamLevel();

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
        catch (Exception e)
        {
            var message = $"An error occurred loading user information. {e.Message}";

            log.Error(message, e);
        }
    }
}
