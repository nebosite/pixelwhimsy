using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;

namespace PixelWhimsy
{
    /// ----------------------------------------------------------------
    /// <summary>
    /// This class manages all application settings
    /// </summary>
    /// ----------------------------------------------------------------
    public static partial class Settings
    {
        private static RegistryKey sourceKey;

        private static bool? checkForUpdates = null;
        private static bool? reportErrors = null;
        private static bool registered;
        private static string id;
        private static PixelCount pixelCount;
        private static bool kidSafe;
        private static bool playableScreensaver;
        private static bool muteScreenSaverVolume;
        private static bool showSettings;
        private static bool windowed;
        private static double volume;
        private static string exitCode;
        private static string exitHint;
        private static double evaluationDays = 0;

        public static bool? ReportErrors { get { return reportErrors; } set { reportErrors = value; } }
        public static bool Registered { get { return registered; } set { registered = value; } }
        public static string Id { get { return id; } set { id = value; } }
        public static PixelCount PixelCount { get { return pixelCount; } set { pixelCount = value; } }
        public static bool PlayableScreensaver { get { return playableScreensaver; } set { playableScreensaver = value; } }
        public static bool MuteScreenSaverVolume { get { return muteScreenSaverVolume; } set { muteScreenSaverVolume = value; } }
        public static bool ShowSettings { get { return showSettings; } set { showSettings = value; } }
        public static bool Windowed { get { return windowed; } set { windowed = value; } }
        public static bool KidSafe { get { return kidSafe; } set { kidSafe = value; } }
        public static double Volume { get { return volume; } set { volume = value; } }
        public static string ExitCode { get { return exitCode; } set { exitCode = value; } }
        public static string ExitHint { get { return exitHint; } set { exitHint = value; } }
        public static double EvaluationDays { get { return evaluationDays; } }
        public static bool Expired { get { return !Registered && (EvaluationDays >= 30 || EvaluationDays < 0); } }

        /// ----------------------------------------------------------------
        /// <summary>
        /// Constructor
        /// </summary>
        /// ----------------------------------------------------------------
        static Settings()
        {
            InitSettings(GlobalState.SettingsKeyName);
        }

        /// ----------------------------------------------------------------
        /// <summary>
        /// Permanently store the current settings
        /// </summary>
        /// ----------------------------------------------------------------
        public static void SaveSettings()
        {
            if (sourceKey != null)
            {
                if (checkForUpdates != null) sourceKey.SetValue("CheckForUpdates", checkForUpdates);
                if (reportErrors != null) sourceKey.SetValue("ReportErrors", reportErrors);
                if (registered) sourceKey.SetValue("candied", 22);
                if (evaluationDays == 0) sourceKey.SetValue("ed", DateTime.Now.Ticks);
                if (id != null) sourceKey.SetValue("id", id);
                sourceKey.SetValue("PixelCount", pixelCount);
                sourceKey.SetValue("PlayableScreensaver", playableScreensaver);
                sourceKey.SetValue("MuteScreensavervolume", muteScreenSaverVolume);
                sourceKey.SetValue("windowed", windowed);
                sourceKey.SetValue("kidsafe", kidSafe);
                sourceKey.SetValue("exitCode", exitCode);
                sourceKey.SetValue("exitHint", exitHint);
                sourceKey.SetValue("showSettings", showSettings);
                sourceKey.SetValue("volume", volume);
            }
        }

        /// ----------------------------------------------------------------
        /// <summary>
        /// Initialize local settings from the registry
        /// </summary>
        /// ----------------------------------------------------------------
        private static void InitSettings(string keyName)
        {
            checkForUpdates = null;
            reportErrors = null;
            registered = false;
            id = null;
            pixelCount = PixelCount.Medium;
            kidSafe = false;
            playableScreensaver = true;
            muteScreenSaverVolume = true;
            showSettings = true;
            windowed = false;
            volume = 1.0;
            exitCode = "Qq";
            exitHint = "To exit the program, Press 'Qq'";

            try
            {
                sourceKey = Registry.LocalMachine.CreateSubKey(keyName);
            }
            catch (UnauthorizedAccessException)
            {
                sourceKey = Registry.CurrentUser.CreateSubKey(keyName);
            }
            catch (Exception)
            {
                sourceKey = null;
            }

            if (sourceKey != null)
            {
                foreach (string name in sourceKey.GetValueNames())
                {
                    switch (name.ToLower())
                    {
                        case "checkforupdates":
                            checkForUpdates = bool.Parse((string)sourceKey.GetValue(name));
                            break;
                        case "reporterrors":
                            reportErrors = bool.Parse((string)sourceKey.GetValue(name));
                            break;
                        case "candied":
                            registered = true;
                            break;
                        case "id":
                            id = (string)sourceKey.GetValue(name);
                            break;
                        case "pixelcount":
                            pixelCount = (PixelCount)Enum.Parse(typeof(PixelCount),(string)sourceKey.GetValue(name));
                            break;
                        case "playablescreensaver":
                            playableScreensaver = bool.Parse((string)sourceKey.GetValue(name));
                            break;
                        case "mutescreensavervolume":
                            muteScreenSaverVolume = bool.Parse((string)sourceKey.GetValue(name));
                            break;
                        case "windowed":
                            windowed = bool.Parse((string)sourceKey.GetValue(name));
                            break;
                        case "kidsafe":
                            kidSafe = bool.Parse((string)sourceKey.GetValue(name));
                            break;
                        case "exitcode":
                            exitCode = (string)sourceKey.GetValue(name);
                            break;
                        case "exithint":
                            exitHint = (string)sourceKey.GetValue(name);
                            break;
                        case "volume":
                            volume = double.Parse((string)sourceKey.GetValue(name));
                            break;
                        case "showsettings":
                            showSettings = bool.Parse((string)sourceKey.GetValue(name));
                            break;
                        case "ed":
                            DateTime regDate = new DateTime(long.Parse((string)sourceKey.GetValue(name)));
                            TimeSpan span = DateTime.Now - regDate;
                            evaluationDays = span.TotalDays;
                            break;
                    }
                }
            }
            else
            {
                checkForUpdates = false;
                reportErrors = false;
                id = Guid.NewGuid().ToString();
            }
        }
    }
}
