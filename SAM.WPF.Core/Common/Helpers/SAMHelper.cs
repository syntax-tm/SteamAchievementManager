using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;
using SAM.WPF.Core.Extensions;

namespace SAM.WPF.Core
{
    public static class SAMHelper
    {

        private const string CLASSIC_MANAGER_EXE = @"SAM.Manager.exe";
        private const string WPF_MANAGER_EXE = @"SAM.WPF.Manager.exe";
        private const string CLASSIC_PICKER_EXE = @"SAM.Picker.exe";
        private const string WPF_PICKER_EXE = @"SAM.WPF.exe";
        private const string STEAM_PROCESS_NAME = @"Steam";

        private const string PICKER_PROCESS_REGEX = @"^SAM(?:\.WPF)?(?:\.Picker)?(?:\.exe)?$";
        private const string MANAGER_PROCESS_REGEX = @"^SAM(?:\.WPF)?\.Manager(?:\.exe)?$";

        private static readonly ILog log = LogManager.GetLogger(nameof(SAMHelper));

        public static bool IsSteamRunning()
        {
            var processes = Process.GetProcessesByName(STEAM_PROCESS_NAME);
            return processes.Any();
        }

        public static bool IsPickerRunning()
        {
            var processes = Process.GetProcesses();
            return processes.Any(p => Regex.IsMatch(p.ProcessName, PICKER_PROCESS_REGEX));
        }

        public static Process OpenPicker(bool useClassicPicker = false)
        {
            var pickerExe = GetPickerExe(useClassicPicker);

            if (!File.Exists(pickerExe))
            {
                log.Warn($"The SAM Picker '{pickerExe}' does not exist.");
                
                // try to fall back to the other manager
                var otherPicker = GetManagerExe(!useClassicPicker);
                
                if (!File.Exists(otherPicker))
                {
                    throw new FileNotFoundException($"Unable to start '{pickerExe}' because it does not exist.", pickerExe);
                }

                pickerExe = otherPicker;
            }

            var proc = Process.Start(pickerExe);

            proc.SetActive();

            return proc;
        }

        public static Process OpenManager(uint appId, bool useClassicManager = false)
        {
            if (appId == default) throw new ArgumentException($"{appId} is not a valid app ID.", nameof(appId));

            var managerExe = GetManagerExe(useClassicManager);

            if (!File.Exists(managerExe))
            {
                log.Warn($"The SAM Manager '{managerExe}' does not exist.");

                // try to fall back to the other manager
                var otherManager = GetManagerExe(!useClassicManager);

                if (!File.Exists(otherManager))
                {
                    throw new FileNotFoundException($"Unable to start '{managerExe}' because it does not exist.", managerExe);
                }

                managerExe = otherManager;
            }

            var proc = Process.Start(managerExe, appId.ToString());

            proc.SetActive();

            return proc;
        }
        
        public static void CloseAllManagers()
        {
            try
            {
                foreach (var proc in Process.GetProcesses())
                {
                    if (!Regex.IsMatch(proc.ProcessName, MANAGER_PROCESS_REGEX)) continue;

                    log.Info($"Found SAM Manager process with process ID {proc.Id}.");

                    proc.Kill();
                }
            }
            catch (Exception e)
            {
                var message = $"An error occurred attempting to close the SAM Managers. {e.Message}";
                log.Error(message, e);
            }
        }

        private static string GetPickerExe(bool useClassicPicker = false)
        {
            return useClassicPicker
                ? CLASSIC_PICKER_EXE
                : WPF_PICKER_EXE;
        }

        private static string GetManagerExe(bool useClassicManager = false)
        {
            return useClassicManager
                ? CLASSIC_MANAGER_EXE
                : WPF_MANAGER_EXE;
        }


    }
}
