using System;
using System.Diagnostics;
using System.IO;
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

        private static readonly ILog log = LogManager.GetLogger(nameof(SAMHelper));

        public static void OpenPicker(bool useClassicPicker = false)
        {
            var pickerExe = GetPickerExe(useClassicPicker);

            if (!File.Exists(pickerExe))
            {
                throw new FileNotFoundException($"Unable to start '{pickerExe}' because it does not exist.", pickerExe);
            }

            Process.Start(pickerExe);
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
            var managerRegex = @"^SAM(?:\.WPF)?\.Manager(?:\.exe)?$";

            foreach (var proc in Process.GetProcesses())
            {
                if (!Regex.IsMatch(proc.ProcessName, managerRegex)) continue;

                proc.Kill();
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
