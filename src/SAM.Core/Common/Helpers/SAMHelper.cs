using ABI.System;
using log4net;
using SAM.API;
using SAM.Core.Extensions;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SAM.Core;

public static partial class SAMHelper
{
    private const string SAM_EXE = @"SAM.exe";
    private const string STEAM_PROCESS_NAME = @"Steam";

    private const string SAM_PROCESS_REGEX = @"^SAM(?:\.exe)?$";
    [GeneratedRegex(SAM_PROCESS_REGEX, RegexOptions.IgnoreCase)]
    private static partial Regex SAMProcessRegex();

    private const string SAM_PICKER_URI = @"sam://";
    private const string SAM_MANAGER_URI_FORMAT = @"sam://manage/{0}";

    private static readonly ILog log = LogManager.GetLogger(nameof(SAMHelper));

    public static void LaunchSteamClient()
    {
        var path = Steam.GetClientPath();

        if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));

        if (!File.Exists(path))
        {
            var message = $"The Steam application does not exist or is not accessible at '{path}'.";
            throw new FileNotFoundException(message, path);
        }

        var psi = new ProcessStartInfo(path)
        {
            UseShellExecute = true
        };

        Process.Start(psi);
    }

    public static void VerifySteamProcess()
    {
        if (IsSteamRunning()) return;

        //  TODO: Change the error message to indicate that Steam needs to be started
        throw new SAMInitializationException(@"Steam process is not currently running.");
    }

    public static bool IsSteamRunning()
    {
        var processes = Process.GetProcessesByName(STEAM_PROCESS_NAME);
        return processes.Length > 0;
    }

    public static bool IsPickerRunning()
    {
        var processes = Process.GetProcesses();
        return processes.Any(p => SAMProcessRegex().IsMatch(p.ProcessName));
    }

    public static Process OpenPicker()
    {
        Process proc;

        if (ApplicationContext.Default.IsUwpApp)
        {
            var uri = SAM_PICKER_URI;
            var psi = new ProcessStartInfo(uri)
            {
                UseShellExecute = true
            };

            proc = Process.Start(psi);
        }
        else
        {
            if (!File.Exists(SAM_EXE))
            {
                throw new FileNotFoundException($"Unable to start '{SAM_EXE}' because it does not exist.", SAM_EXE);
            }

            proc = Process.Start(SAM_EXE);
        }

        proc.SetActive();

        return proc;
    }

    public static Process OpenManager(uint appId)
    {
        if (appId == 0) throw new ArgumentException($"App id {appId} is not valid.", nameof(appId));

        Process proc;

        if (ApplicationContext.Default.IsUwpApp)
        {
            // running as uwp app, launch via uri
            var uri = string.Format(SAM_MANAGER_URI_FORMAT, appId);
            var psi = new ProcessStartInfo(uri)
            {
                UseShellExecute = true
            };

            proc = Process.Start(psi);
        }
        else
        {
            // not running in UWP container, launch exe normally
            if (!File.Exists(SAM_EXE))
            {
                throw new FileNotFoundException($"Unable to start '{SAM_EXE}' because it does not exist.", SAM_EXE);
            }

            string[] args = ["manage", $"{appId}"];
            var psi = new ProcessStartInfo(SAM_EXE, args);
            proc = Process.Start(psi);
        }

        proc.SetActive();

        return proc;
    }

    public static Process UnlockAll(uint appId)
    {
        if (appId == 0) throw new ArgumentException($"App id {appId} is not valid.", nameof(appId));

        Process proc;

        if (ApplicationContext.Default.IsUwpApp)
        {
            var uri = $"sam://manage/{appId}/unlock-all";
            var psi = new ProcessStartInfo(uri)
            {
                UseShellExecute = true
            };

            proc = Process.Start(psi);
        }
        else
        {
            if (!File.Exists(SAM_EXE))
            {
                throw new FileNotFoundException($"Unable to start '{SAM_EXE}' because it does not exist.", SAM_EXE);
            }

            string[] args = ["manage", $"{appId}", "-u"];
            var psi = new ProcessStartInfo(SAM_EXE, args)
            {
                CreateNoWindow = true
            };
            proc = Process.Start(psi);
        }

        return proc;
    }
        
    public static void CloseAllManagers()
    {
        try
        {
            foreach (var proc in Process.GetProcesses())
            {
                if (!SAMProcessRegex().IsMatch(proc.ProcessName)) continue;

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
