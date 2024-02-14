using log4net;

namespace SAM.Core;

public class SteamLibraryManager
{
	private static readonly ILog log = LogManager.GetLogger(nameof(SteamLibraryManager));

	private static readonly object syncLock = new();
	private static SteamLibraryManager _instance;

	public static bool IsInitialized
	{
		get; private set;
	}

	public SteamLibrary Library
	{
		get; set;
	}

	public static SteamLibraryManager Default
	{
		get
		{
			if (_instance != null)
			{
				return _instance;
			}

			lock (syncLock)
			{
				_instance = new();
			}

			return _instance;
		}
	}

	public static SteamLibrary DefaultLibrary => Default.Library;

	public static void Init ()
	{
		if (IsInitialized)
		{
			throw new InvalidOperationException("Steam library is already initialized.");
		}

		try
		{
			var library = new SteamLibrary();
			library.Refresh();

			Default.Library = library;

			IsInitialized = true;
		}
		catch (Exception e)
		{
			var message = $"An error occurred attempting to initialize the Steam library. {e.Message}";
			log.Error(message, e);

			throw new SAMInitializationException(message, e);
		}
	}
}
