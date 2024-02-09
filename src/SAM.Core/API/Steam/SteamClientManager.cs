using System;
using System.Reflection;
using log4net;
using SAM.API;

namespace SAM.Core;

public static class SteamClientManager
{
	private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

	private static bool _isInitialized;

	public static uint AppId
	{
		get; private set;
	}
	public static string CurrentLanguage
	{
		get; private set;
	}
	public static Client Default
	{
		get; private set;
	}

	public static void Init (uint appId)
	{
		if (_isInitialized)
		{
			throw new SAMInitializationException($"The Steam {nameof(Client)} has already been initialized.");
		}

		try
		{
			Default = new();
			Default.Initialize(appId);

			AppId = appId;
			CurrentLanguage = Default.SteamApps008.GetCurrentGameLanguage();

			_isInitialized = true;
		}
		catch (Exception e)
		{
			var message = $"An error occurred attempting to initialize the Steam client with app ID '{appId}'. {e.Message}";
			log.Error(message, e);

			throw new SAMInitializationException(message, e);
		}
	}
}
