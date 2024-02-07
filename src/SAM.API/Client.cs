using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using log4net;
using SAM.API.Wrappers;

namespace SAM.API
{
    public class Client : IDisposable
    {
        private static readonly ILog log = LogManager.GetLogger(nameof(Client));

        private readonly List<ICallback> callbacks = [];

        private bool _isDisposed;
        private int _pipe;

        private volatile bool _runningCallbacks;
        private int _user;

        public SteamApps001 SteamApps001 { get; private set; }
        public SteamApps008 SteamApps008 { get; private set; }
        public SteamClient019 SteamClient { get; private set; }
        public SteamUser017 SteamUser { get; private set; }
        public SteamUserStats007 SteamUserStats { get; private set; }
        public SteamUtils005 SteamUtils { get; private set; }

        public void Initialize(long appId)
        {
            if (string.IsNullOrEmpty(Steam.GetInstallPath())) throw new ClientInitializeException(ClientInitFailure.GetInstallPath);

            if (appId != 0) Environment.SetEnvironmentVariable(@"SteamAppId", appId.ToString(CultureInfo.InvariantCulture));

            if (Steam.Load() == false) throw new ClientInitializeException(ClientInitFailure.Load);

            SteamClient = Steam.CreateInterface<SteamClient019>(nameof(SteamClient019));
            if (SteamClient == null) throw new ClientInitializeException(ClientInitFailure.CreateSteamClient);
            
            _pipe = SteamClient.CreateSteamPipe();
            if (_pipe == 0) throw new ClientInitializeException(ClientInitFailure.CreateSteamPipe);

            _user = SteamClient.ConnectToGlobalUser(_pipe);
            if (_user == 0) throw new ClientInitializeException(ClientInitFailure.ConnectToGlobalUser);

            SteamUtils = SteamClient.GetSteamUtils004(_pipe);
            if (appId > 0 && SteamUtils.GetAppId() != (uint)appId) throw new ClientInitializeException(ClientInitFailure.AppIdMismatch);

            SteamUser = SteamClient.GetSteamUser017(_user, _pipe);
            SteamUserStats = SteamClient.GetSteamUserStats006(_user, _pipe);
            SteamApps001 = SteamClient.GetSteamApps001(_user, _pipe);
            SteamApps008 = SteamClient.GetSteamApps008(_user, _pipe);
        }

        ~Client()
        {
            Dispose(false);
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                // dispose of managed resources
            }

            // dispose of unmanaged resources
            if (SteamClient != null && _pipe > 0)
            {
                if (_user > 0)
                {
                    SteamClient.ReleaseUser(_pipe, _user);
                    _user = 0;
                }

                SteamClient.ReleaseSteamPipe(_pipe);
                _pipe = 0;

                SteamClient.ShutdownIfAllPipesClosed();
            }

            SteamClient = null;
            SteamUser = null;
            SteamApps001 = null;
            SteamApps008 = null;
            SteamUserStats = null;
            SteamUtils = null;

            _isDisposed = true;
        }

        public TCallback CreateAndRegisterCallback<TCallback>()
            where TCallback : ICallback, new()
        {
            var callback = new TCallback();
            callbacks.Add(callback);
            return callback;
        }

        public void RunCallbacks(bool server)
        {
            if (_runningCallbacks) return;

            _runningCallbacks = true;

            while (Steam.GetCallback(_pipe, out var message, out _))
            {
                var callbackId = message.Id;
                var messageCallbacks = callbacks.Where(candidate => candidate.Id == callbackId &&
                                                            candidate.IsServer == server);

                foreach (var callback in messageCallbacks)
                {
                    callback.Run(message.ParamPointer);
                }

                _ = Steam.FreeLastCallback(_pipe);
            }

            _runningCallbacks = false;
        }
    }
}
