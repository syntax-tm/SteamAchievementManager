using System.Xml;
using DevExpress.Mvvm.POCO;
using log4net;
using SAM.Core.Extensions;

namespace SAM.Core
{
    // TODO: add support for any steam user (currently only supports the current steam user)
    public class SteamUser
    {
        private const string PROFILE_URL_FORMAT = @"https://steamcommunity.com/profiles/{0}";
        private const string PROFILE_XML_URL_FORMAT = @"https://steamcommunity.com/profiles/{0}?xml=1";

        private static readonly ILog log = LogManager.GetLogger(nameof(SteamUser));

        public virtual ulong SteamId64 { get; set; }
        public virtual string SteamId { get; set; }
        public virtual int PlayerLevel { get; set; }
        public virtual string CustomUrl { get; set; }
        public virtual decimal RecentHoursPlayed { get; set; }
        public virtual string Headline { get; set; }
        public virtual string Location { get; set; }
        public virtual string MemberSince { get; set; }
        public virtual string RealName { get; set; }
        public virtual string ProfileUrl { get; set; }
        public virtual string AvatarIcon { get; set; }
        public virtual string AvatarMedium { get; set; }
        public virtual string AvatarFull { get; set; }
        public virtual bool VACBanned { get; set; }
        public virtual bool IsLimitedAccount { get; set; }

        protected SteamUser()
        {
            RefreshProfile();
        }

        public static SteamUser Create()
        {
            return ViewModelSource.Create(() => new SteamUser());
        }

        private void RefreshProfile()
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
            
            AvatarIcon = doc.GetValue(@"//avatarIcon");
            AvatarMedium = doc.GetValue(@"//avatarMedium");
            AvatarFull = doc.GetValue(@"//avatarFull");
            
            CustomUrl = doc.GetValue(@"//customUrl");
            MemberSince = doc.GetValue(@"//memberSince");
            Headline = doc.GetValue(@"//Headline");
            Location = doc.GetValue(@"//location");
            RealName = doc.GetValue(@"//realname");
            
            // TODO: no idea what this is for since min and everyone i checked was empty
            //var steamRating = GetValue(doc, @"//steamRating");
            
            VACBanned = doc.GetValueAsBool(@"//vacBanned");
            IsLimitedAccount = doc.GetValueAsBool(@"//isLimitedAccount");
            RecentHoursPlayed = doc.GetValueAsDecimal(@"//hoursPlayed2Wk");

            log.Debug($"Finished loading steam user {SteamId} ({SteamId64}) user profile.");
        }
    }
}
