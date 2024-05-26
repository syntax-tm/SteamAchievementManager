using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;
using SAM.Core.Extensions;

namespace SAM.Core
{
    public static class SAMHelper
    {
        private const string SAM_EXE = @"SAM.exe";
        private const string STEAM_PROCESS_NAME = @"Steam";

        private const string SAM_PROCESS_REGEX = @"^SAM(?:\.exe)?$";

        private static readonly ILog log = LogManager.GetLogger(nameof(SAMHelper));

        public static void VerifySteamProcess()
        {
            if (IsSteamRunning()) return;

            //  TODO: Change the error message to indicate that Steam needs to be started
            throw new SAMInitializationException(@"Steam process is not currently running.");
        }

        public static bool IsSteamRunning()
        {
            var processes = Process.GetProcessesByName(STEAM_PROCESS_NAME);
            return processes.Any();
        }

        public static bool IsPickerRunning()
        {
            var processes = Process.GetProcesses();
            return processes.Any(p => Regex.IsMatch(p.ProcessName, SAM_PROCESS_REGEX));
        }

        public static Process OpenPicker()
        {
            if (!File.Exists(SAM_EXE))
            {
                throw new FileNotFoundException($"Unable to start '{SAM_EXE}' because it does not exist.", SAM_EXE);
            }

            var proc = Process.Start(SAM_EXE);

            proc.SetActive();

            return proc;
        }

        public static Process OpenManager(uint appId)
        {
            if (appId == default) throw new ArgumentException($"App id {appId} is not valid.", nameof(appId));
            
            if (!File.Exists(SAM_EXE))
            {
                throw new FileNotFoundException($"Unable to start '{SAM_EXE}' because it does not exist.", SAM_EXE);
            }

            var proc = Process.Start(SAM_EXE, appId.ToString());

            proc.SetActive();

            return proc;
        }
        
        public static void CloseAllManagers()
        {
            try
            {
                foreach (var proc in Process.GetProcesses())
                {
                    if (!Regex.IsMatch(proc.ProcessName, SAM_PROCESS_REGEX)) continue;

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
}
