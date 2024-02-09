#pragma warning disable CA1305

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;
using SAM.Core.Extensions;

namespace SAM.Core;

public static class SAMHelper
{
	private const string SAM_MANAGER_EXE = @"SAM.Manager.exe";
	private const string SAM_PICKER_EXE = @"SAM.exe";
	private const string STEAM_PROCESS_NAME = @"Steam";

	private const string PICKER_PROCESS_REGEX = @"^SAM(?:\.exe)?$";
	private const string MANAGER_PROCESS_REGEX = @"^SAM\.Manager(?:\.exe)?$";

	private static readonly ILog log = LogManager.GetLogger(nameof(SAMHelper));

	public static void VerifySteamProcess ()
	{
		if (IsSteamRunning())
		{
			return;
		}

		//  TODO: Change the error message to indicate that Steam needs to be started
		throw new SAMInitializationException(@"Steam process is not currently running.");
	}

	public static bool IsSteamRunning ()
	{
		var processes = Process.GetProcessesByName(STEAM_PROCESS_NAME);
		return processes.Length != 0;
	}

	public static bool IsPickerRunning ()
	{
		var processes = Process.GetProcesses();
		return processes.Any(p => Regex.IsMatch(p.ProcessName, PICKER_PROCESS_REGEX));
	}

	public static Process OpenPicker ()
	{
		if (!File.Exists(SAM_PICKER_EXE))
		{
			throw new FileNotFoundException($"Unable to start '{SAM_PICKER_EXE}' because it does not exist.", SAM_PICKER_EXE);
		}

		var proc = Process.Start(SAM_PICKER_EXE);

		proc.SetActive();

		return proc;
	}

	public static Process OpenManager (uint appId)
	{
		if (appId == default)
		{
			throw new ArgumentException($"App id {appId} is not valid.", nameof(appId));
		}

		if (!File.Exists(SAM_MANAGER_EXE))
		{
			throw new FileNotFoundException($"Unable to start '{SAM_MANAGER_EXE}' because it does not exist.", SAM_MANAGER_EXE);
		}

		var proc = Process.Start(SAM_MANAGER_EXE, appId.ToString());

		proc.SetActive();

		return proc;
	}

	public static void CloseAllManagers ()
	{
		try
		{
			foreach (var proc in Process.GetProcesses())
			{
				if (!Regex.IsMatch(proc.ProcessName, MANAGER_PROCESS_REGEX))
					{
					continue;
				}

				log.Info($"Found SAM Manager process with process ID {proc.Id}.");

				proc.Kill();
			}
		}
		catch (Exception e)
		{
			var message = $"An error occurred attempting to stop the running SAM Manager processes. {e.Message}";
			log.Error(message, e);
		}
	}
}
