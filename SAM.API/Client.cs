﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SAM.API.Types;
using SAM.API.Wrappers;

namespace SAM.API
{
    public class Client : IDisposable
    {
        private readonly List<ICallback> _Callbacks = new();

        private bool _isDisposed;
        private int _pipe;

        private bool _RunningCallbacks;
        private int _user;
        public SteamApps001 SteamApps001;
        public SteamApps008 SteamApps008;
        public SteamClient018 SteamClient;
        public SteamUser012 SteamUser;
        public SteamUserStats007 SteamUserStats;
        public SteamUtils005 SteamUtils;

        public void Initialize(long appId)
        {
            if (string.IsNullOrEmpty(Steam.GetInstallPath())) throw new ClientInitializeException(ClientInitFailure.GetInstallPath, "failed to get Steam install path");

            if (appId != 0) Environment.SetEnvironmentVariable(@"SteamAppId", appId.ToString(CultureInfo.InvariantCulture));

            if (Steam.Load() == false) throw new ClientInitializeException(ClientInitFailure.Load, "failed to load SteamClient");

            SteamClient = Steam.CreateInterface<SteamClient018>(nameof(SteamClient018));
            if (SteamClient == null) throw new ClientInitializeException(ClientInitFailure.CreateSteamClient, "failed to create ISteamClient018");

            _pipe = SteamClient.CreateSteamPipe();
            if (_pipe == 0) throw new ClientInitializeException(ClientInitFailure.CreateSteamPipe, "failed to create pipe");

            _user = SteamClient.ConnectToGlobalUser(_pipe);
            if (_user == 0) throw new ClientInitializeException(ClientInitFailure.ConnectToGlobalUser, "failed to connect to global user");

            SteamUtils = SteamClient.GetSteamUtils004(_pipe);
            if (appId > 0 && SteamUtils.GetAppId() != (uint)appId) throw new ClientInitializeException(ClientInitFailure.AppIdMismatch, "appID mismatch");

            SteamUser = SteamClient.GetSteamUser012(_user, _pipe);
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
            _Callbacks.Add(callback);
            return callback;
        }

        public void RunCallbacks(bool server)
        {
            if (_RunningCallbacks) return;

            _RunningCallbacks = true;

            while (Steam.GetCallback(_pipe, out var message, out _))
            {
                var callbackId = message.Id;

                foreach (var callback in _Callbacks.Where(
                    candidate => candidate.Id == callbackId &&
                        candidate.IsServer == server))
                {
                    callback.Run(message.ParamPointer);
                }

                Steam.FreeLastCallback(_pipe);
            }

            _RunningCallbacks = false;
        }
    }
}
