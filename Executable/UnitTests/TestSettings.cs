using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Microsoft.Win32;
using System.IO;

namespace PixelWhimsy
{
    public static partial class Settings
    {
        [TestFixture]
        public class Test
        {
            /// --------------------------------------------------------------------------
            /// <summary>
            /// Test encoding and decoding of registration keys
            /// </summary>
            /// --------------------------------------------------------------------------
            [Test]
            public void TestSettings()
            {
                string testKey = @"software\Niftibits\PixelWhimsytest";

                Registry.LocalMachine.DeleteSubKey(testKey, false);

                try
                {
                    Settings.InitSettings(testKey);
                    Assert.AreEqual(null, Settings.ReportErrors, "\nReportErrors should be null");
                    Assert.AreEqual(null, Settings.Id, "\nShould have no id");
                    Assert.AreEqual(false, Settings.Registered, "\nShould not be registered");
                    Assert.AreEqual(PixelCount.Medium, Settings.PixelCount);
                    Assert.AreEqual(true, Settings.KidSafe);
                    Assert.AreEqual(true, Settings.PlayableScreensaver);
                    Assert.AreEqual(true, Settings.MuteScreenSaverVolume);
                    Assert.AreEqual(true, Settings.ShowSettings);
                    Assert.AreEqual(false, Settings.Windowed);
                    Assert.AreEqual(1.0, Settings.Volume);
                    Assert.AreEqual("Qq", Settings.ExitCode);
                    Assert.AreEqual("To exit the program, Press 'Qq'", Settings.ExitHint);

                    Settings.ReportErrors = false;
                    Settings.Id = "foo";
                    Settings.Registered = true;
                    Settings.PixelCount = PixelCount.Low;
                    Settings.KidSafe = false;
                    Settings.PlayableScreensaver = false;
                    Settings.MuteScreenSaverVolume = false;
                    Settings.ShowSettings = false;
                    Settings.Windowed = true;
                    Settings.Volume = 1.5;
                    Settings.ExitCode = "zZ";
                    Settings.ExitHint = "Jabba says zZ";

                    Settings.InitSettings(testKey);
                    Assert.AreEqual(null, Settings.ReportErrors, "\nReportErrors should be null");
                    Assert.AreEqual(null, Settings.Id, "\nShould have no id");
                    Assert.AreEqual(false, Settings.Registered, "\nShould not be registered");
                    Assert.AreEqual(PixelCount.Medium, Settings.PixelCount);
                    Assert.AreEqual(true, Settings.KidSafe);
                    Assert.AreEqual(true, Settings.PlayableScreensaver);
                    Assert.AreEqual(true, Settings.MuteScreenSaverVolume);
                    Assert.AreEqual(true, Settings.ShowSettings);
                    Assert.AreEqual(false, Settings.Windowed);
                    Assert.AreEqual(1.0, Settings.Volume);
                    Assert.AreEqual("Qq", Settings.ExitCode);
                    Assert.AreEqual("To exit the program, Press 'Qq'", Settings.ExitHint);

                    Settings.ReportErrors = false;
                    Settings.Id = "foo";
                    Settings.Registered = true;
                    Settings.PixelCount = PixelCount.Low;
                    Settings.KidSafe = false;
                    Settings.PlayableScreensaver = false;
                    Settings.MuteScreenSaverVolume = false;
                    Settings.ShowSettings = false;
                    Settings.Windowed = true;
                    Settings.Volume = 1.5;
                    Settings.ExitCode = "zZ";
                    Settings.ExitHint = "Jabba says zZ";

                    Settings.SaveSettings();
                    Settings.InitSettings(testKey);
                    Assert.AreEqual(false, Settings.ReportErrors, "\nReportErrors");
                    Assert.AreEqual("foo", Settings.Id, "\nId");
                    Assert.AreEqual(true, Settings.Registered, "\nShould be registered");
                    Assert.AreEqual(PixelCount.Low, Settings.PixelCount);
                    Assert.AreEqual(false, Settings.KidSafe);
                    Assert.AreEqual(false, Settings.PlayableScreensaver);
                    Assert.AreEqual(false, Settings.MuteScreenSaverVolume);
                    Assert.AreEqual(false, Settings.ShowSettings);
                    Assert.AreEqual(true, Settings.Windowed);
                    Assert.AreEqual(1.5, Settings.Volume);
                    Assert.AreEqual("zZ", Settings.ExitCode);
                    Assert.AreEqual("Jabba says zZ", Settings.ExitHint);

                    Settings.ReportErrors = null;
                    Settings.Id = null;
                    Settings.Registered = false;
                    Settings.KidSafe = false;
                    Settings.PlayableScreensaver = false;
                    Settings.MuteScreenSaverVolume = false;
                    Settings.ShowSettings = false;
                    Settings.Windowed = true;
                    Settings.Volume = 1.5;
                    Settings.ExitCode = "zZ";
                    Settings.ExitHint = "Jabba says zZ";

                    Settings.SaveSettings();
                    Registry.LocalMachine.DeleteSubKeyTree(testKey);
                    Settings.InitSettings(testKey);
                    Assert.AreEqual(null, Settings.ReportErrors, "\nReportErrors");
                    Assert.AreEqual(null, Settings.Id, "\nId");
                    Assert.AreEqual(false, Settings.Registered, "\nShould not be registered");
                    Assert.AreEqual(PixelCount.Medium, Settings.PixelCount);
                    Assert.AreEqual(true, Settings.KidSafe);
                    Assert.AreEqual(true, Settings.PlayableScreensaver);
                    Assert.AreEqual(true, Settings.MuteScreenSaverVolume);
                    Assert.AreEqual(true, Settings.ShowSettings);
                    Assert.AreEqual(false, Settings.Windowed);
                    Assert.AreEqual(1.0, Settings.Volume);
                    Assert.AreEqual("Qq", Settings.ExitCode);
                    Assert.AreEqual("To exit the program, Press 'Qq'", Settings.ExitHint);

                }
                finally
                {
                    Registry.LocalMachine.DeleteSubKey(testKey, false);
                }
            }
        }
    }
}
