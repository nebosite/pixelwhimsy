using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using DirectVarmint;
using System.Threading;
using System.IO;
using Microsoft.Win32;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace PixelWhimsy
{
    public delegate void FrameDriver(int frameRate);
    public delegate void ExceptionLogger(Exception e);

    static partial class Program
    {
        static bool running = true;
        static List<Slate> slates = new List<Slate>();
        static MySystemHandler systemHandler;
        static Thread mainThread;

        #region Main
        /// --------------------------------------------------------------------------
        /// <summary>
        /// Main
        /// </summary>
        /// --------------------------------------------------------------------------
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
#if DEBUG
            GlobalState.Debugging = true;
#endif

            if (args.Length > 0)
            {
                string mainArgument = args[0].ToLower();

                if (mainArgument == "/setup")
                {
                    try
                    {
                        string pics, data;
                        Utilities.SetupDataFolders(out pics, out data);
                        //SetRegistryKeyPermissions(GlobalState.SettingsKeyName, "Users");
                    }
                    catch (Exception)
                    {
                    }
                }
                else if (mainArgument.StartsWith("/c"))
                {
                    new SettingsForm().ShowDialog();
                }
                else if (mainArgument.StartsWith("/s"))
                {
                    GlobalState.RunningAsScreenSaver = true;
                    RunProgram(null);
                }
                else if (mainArgument.StartsWith("/p"))
                {
                    IntPtr hwnd;
                    Form tempForm;

                    if (args.Length < 2)
                    {
                        tempForm = new Form();
                        tempForm.Show();
                        hwnd = tempForm.Handle;
                    }
                    else
                    {
                        hwnd = new IntPtr(int.Parse(args[1]));
                    }

                    GlobalState.RunningAsScreenSaver = true;
                    GlobalState.RunningInPreview = true;
                    RunPreview(hwnd);
                }
                else
                {
                    MessageBox.Show("Unknown argument: " + mainArgument);
                }
            }
            else
            {
                RunProgram(null);
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Run a conventional preview because managed DX doesn't know what to 
        /// do with an hwnd
        /// </summary>
        /// --------------------------------------------------------------------------
        private static void RunPreview(IntPtr hwnd)
        {
            Graphics gMain = Graphics.FromHwnd(hwnd);
            RECT windowRect;
            User32.GetWindowRect(hwnd, out windowRect);
            int w = windowRect.right - windowRect.left;
            int h = windowRect.bottom - windowRect.top;
            Image backBuffer = new Bitmap(w, h);
            Graphics g = Graphics.FromImage(backBuffer);
            Brush ballBrush = new SolidBrush(Color.Green);
            Brush hiliteBrush = new SolidBrush(Color.LightGreen);

            Image image = Bitmap.FromStream(DVTools.GetStream("PixelWhimsy.bitmaps.transparent-logo.png"));
            int x = (w - image.Width) / 2;
            int y = (h - image.Height) / 2;
           
            ImageAttributes ia = new ImageAttributes();
            ia.SetColorKey( Color.Black, Color.Black);

            double bx, by, xm, ym;
            bx = w / 2;
            by = h / 2;
            xm = Utilities.DRand(10) - 5;
            ym = Utilities.DRand(10) - 5;

            while (User32.IsWindow(hwnd) && User32.IsWindowVisible(hwnd))
            {
                g.Clear(Color.Black);

                g.FillEllipse(ballBrush, (int)bx, (int)by, 20, 20);
                g.FillEllipse(hiliteBrush, (int)bx+4, (int)by+4, 3, 3);
                bx += xm;
                by += ym;
                if (bx > w - 20 || bx < 0) xm = -xm;
                if (by > h - 20 || by < 0) ym = -ym;

                g.DrawImage(image, new Rectangle(x, y, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, ia);
                gMain.DrawImageUnscaled(backBuffer, 0, 0);
                Application.DoEvents();
                Thread.Sleep(20);
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Run PixelWhimsy
        /// </summary>
        /// --------------------------------------------------------------------------
        static void RunProgram(Control displayWnd)
        {
            HiPerfTimer hpTimer = new HiPerfTimer();
            hpTimer.Start();
            int frameRate = 60;
            mainThread = Thread.CurrentThread;

            Screen[] screens = Screen.AllScreens;
            Screen screenToUse = screens[0];
            foreach (Screen screen in screens)
            {
                if (screen.Primary)
                {
                    screenToUse = screen;
                    break;
                }
            }

            GetPrivacySettings();


            // Prompt for registration if we aren't registered
            if (!Settings.Registered && !GlobalState.RunningAsScreenSaver)
            {
                RegistrationForm regForm = new RegistrationForm();
                regForm.Show();
                regForm.Left = screens[0].Bounds.Width / 2 - regForm.Width / 2;
                regForm.Top = screens[0].Bounds.Height / 2 - regForm.Height / 2;
                Application.Run(regForm);
                if (regForm.Registered)
                {
                    Settings.Registered = true;
                }

                Settings.SaveSettings();
            }

            if (Settings.Expired) return;


            if (!GlobalState.RunningAsScreenSaver && Settings.ShowSettings)
            {
                SettingsForm settingsForm = new SettingsForm();
                settingsForm.ShowDialog();
            }

            // Grab the keyboard and mouse inputs for the whole system
            systemHandler = new MySystemHandler(Settings.KidSafe);
            try
            {
                int slateId = 0;

                AddSlate(slateId++, displayWnd, screenToUse);

                MediaBag.Initialize(slates[0]);

                slates[0].isPrimaryWindow = true;

                if (!GlobalState.RunningInPreview)
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(WatchDog), slates.Count);
                }

                if(!GlobalState.RunningInPreview) systemHandler.SetHooks();
                RunWithErrorHandling(DrivePixelWhimsy, new WaitCallback(Utilities.ReportExceptionWorker), frameRate, 3, 30000);

                if (!GlobalState.RunningAsScreenSaver)
                {
                    try
                    {
                        File.WriteAllText("data.xml", slates[0].SerializeLog());
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception e)
            {
                Utilities.LogException(e);

                systemHandler.ClearHooks();
                running = false;
                Thread.Sleep(250);

                if ((bool)Settings.ReportErrors)
                {
                    if(slates.Count > 0) Utilities.ReportException(e, slates[0].SerializeLog());
                }

                if (e.ToString().Contains("D3DERR_"))
                {
                    MessageBox.Show("PixelWhimsy has encountered a problem with DirectX.  Please install DirectX version 9 or later.  If version 9 or later is already installed, it may not be installed correctly or your current video card may not support Direct3D.");
                }
                else
                {
                    if (GlobalState.Debugging)
                    {
                        MessageBox.Show(e.ToString());
                    }
                    MessageBox.Show("Oops!  I had a problem that I couldn't fix, so I'll have to quit now.  Press [Enter] to close the applicaton.");
                }
            }
            finally
            {
                systemHandler.ClearHooks();
                running = false;
                if (slates.Count > 0)
                {
                    slates[0].AbortThreadedRender();
                    slates.Clear();
                }
            }

        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Set read permissions to a registry key
        /// </summary>
        /// --------------------------------------------------------------------------
        public static void SetRegistryKeyPermissions(string keyName, string userName)
        {
            //// get a handle to the key       
            //IntPtr hKey;
            //if (Microsoft.Win32.Security.Win32.RegOpenKey(Win32Consts.HKEY_LOCAL_MACHINE, keyName, out hKey) != Microsoft.Win32.Security.Win32.SUCCESS) { throw new Exception("failed to access registry key " + keyName); } try
            //{
            //    // get the security descriptor             
            //    using (SecurityDescriptor sd = SecurityDescriptor.GetRegistryKeySecurity(hKey, SECURITY_INFORMATION.DACL_SECURITY_INFORMATION))
            //    {
            //        // give the service user standard read access.                   
            //        Dacl acl = sd.Dacl; Sid serviceUserSID = new Sid(userName); acl.RemoveAces(serviceUserSID); AccessType aclAccessType = AccessType.GENERIC_ALL; acl.AddAce(new AceAccessAllowed(serviceUserSID, aclAccessType, AceFlags.OBJECT_INHERIT_ACE | AceFlags.CONTAINER_INHERIT_ACE));
            //        // set the values                   
            //        sd.SetDacl(acl); sd.SetRegistryKeySecurity(hKey, SECURITY_INFORMATION.DACL_SECURITY_INFORMATION);
            //    }
            //}
            //finally { Microsoft.Win32.Security.Win32.RegCloseKey(hKey); }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Determine the optimum resolution
        /// </summary>
        /// --------------------------------------------------------------------------
        private static void GetResolution(PixelCount pixelCount, out int resx, out int resy)
        {
            switch (pixelCount)
            {
                case PixelCount.High: resx = 800; resy = 600; break;
                case PixelCount.Medium: resx = 640; resy = 480; break;
                default: resx = 400; resy = 300; break;
            }
        }

        //private static void GetResolution(int width, int height, PixelCount pixelCount, out int resx, out int resy, out int multiplier)
        //{
            //int maxPixelCount = 500000;
            //int minPixelCount = 300000;
            //if (pixelCount == PixelCount.Low)
            //{
            //    maxPixelCount = 180000;
            //    minPixelCount = 120000;
            //}
            //if (pixelCount == PixelCount.Medium)
            //{
            //    maxPixelCount = 300000;
            //    minPixelCount = 180000;
            //}

            //int minN = (int)Math.Sqrt(minPixelCount / 12);
            //int maxN = (int)Math.Sqrt(maxPixelCount / 12);

            //for (multiplier = 3; multiplier > 0; multiplier--)
            //{
            //    for (int i = maxN; i >= minN; i--)
            //    {
            //        resx = i * 4;
            //        resy = i * 3;
            //        int clearanceX = width - resx * multiplier;
            //        int clearanceY = height - resy * multiplier;
            //        double coverage = (resx * resy * multiplier * multiplier) / (double)(width * height);
            //        if (clearanceX >= 150 && clearanceY >= 150) return;
            //    }
            //}

            //resx = 400;
            //resy = 300;
            //multiplier = 1;
        //    return;
        //}

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Drive the Program
        /// </summary>
        /// --------------------------------------------------------------------------
        static void DrivePixelWhimsy(int framesPerSecond)
        {
            foreach(Slate slate in slates)
            {
                slate.Reset();
            }

            DVTools.DriveApplication(framesPerSecond);
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// As the program runs, safely catch errors and try again.
        /// </summary>
        /// --------------------------------------------------------------------------
        private static void RunWithErrorHandling(FrameDriver driver, WaitCallback logger, int frameRate, int maxErrors, int errorPeriodInMilliseconds)
        {
            bool keepGoing = true;
            List<DateTime> errorTimes = new List<DateTime>();

            while (keepGoing)
            {
                try
                {
                    driver(frameRate);
                }
                catch (Exception e)
                {
                    Utilities.LogException(e);

                    if (e is ThreadAbortException)
                    {
                        e = new ApplicationException("Hung State Aborted. (Non Fatal)" + Environment.NewLine + slates[0].AnimationSummary(), e);
                    }

                    errorTimes.Add(DateTime.Now);
                    while (errorTimes.Count > maxErrors)
                    {
                        errorTimes.RemoveAt(0);
                    }

                    if (errorTimes.Count >= maxErrors)
                    {
                        if ((bool)Settings.ReportErrors)
                        {
                            ThreadPool.QueueUserWorkItem(logger, e);
                        }

                        TimeSpan span = errorTimes[maxErrors - 1] - errorTimes[0];
                        if (span.TotalMilliseconds < errorPeriodInMilliseconds)
                        {
                            throw new ApplicationException("Too many errors");
                        }
                    }

                    if (e is TooManyExceptions)
                    {
                        if ((bool)Settings.ReportErrors)
                        {
                            ThreadPool.QueueUserWorkItem(logger, e);
                        }
                        throw new ApplicationException("Too many errors");
                    }
                    
                    continue;
                }
                finally
                {

                }
                keepGoing = false;
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Load privacy settings from the registry, or get them from the user.
        /// </summary>
        /// --------------------------------------------------------------------------
        private static void GetPrivacySettings()
        {
            if (Settings.Id == null)
            {
                Settings.Id = Guid.NewGuid().ToString();
                Settings.SaveSettings();
            }

            if (Settings.ReportErrors == null)
            {
                PrivacyForm privacyForm = new PrivacyForm();
                privacyForm.ShowDialog();
                Settings.ReportErrors = privacyForm.ReportErrors;
                Settings.SaveSettings();
            }
        }


        /// --------------------------------------------------------------------------
        /// <summary>
        /// Logic goes here to check for an update on the internet
        /// </summary>
        /// --------------------------------------------------------------------------
        private static bool DoUpdateCheck()
        {

            if (Utilities.NeedsNewVersion(AssemblyConstants.Version, Utilities.GetCurrentVersion("http://version.pixelwhimsy.com/CurrentVersion.xml?v=" + AssemblyConstants.Version)))
            {
                DialogResult result = MessageBox.Show("There is a newer version of PixelWhimsy" + (char)0x99 +
                    " available on the internet. Would you like to download it?", "New Version!", MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {
                    return true;   
                }
            }

            return false;
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <param name="slateId"></param>
        /// <param name="newX"></param>
        /// <param name="newY"></param>
        /// <returns></returns>
        /// --------------------------------------------------------------------------
        private static int AddSlate(int slateId, Control displayWnd, Screen screenToUse)
        {
            int resx, resy;
            int screenWidth = screenToUse.Bounds.Width;
            int screenHeight = screenToUse.Bounds.Height;

            GetResolution( Settings.PixelCount, out resx, out resy);
            if (!GlobalState.RunningAsScreenSaver && Settings.Windowed)
            {
                screenWidth = resx + GlobalState.ToolBarBorder;
                screenHeight = resy + GlobalState.ToolBarBorder;

                if (Settings.PixelCount == PixelCount.Low)
                {
                    screenWidth = resx * 2 + GlobalState.ToolBarBorder;
                    screenHeight = resy * 2 + GlobalState.ToolBarBorder;
                }
            }


            GlobalState.resolutionX = resx;
            GlobalState.resolutionY = resy;

            int newX = screenToUse.Bounds.X;
            int newY = screenToUse.Bounds.Y;
            int width = screenWidth;
            int height = screenHeight;

            //if (GlobalState.Debugging)
            //{
            //    GlobalState.resolutionX = 400 + 1;// DateTime.Now.Second;
            //    GlobalState.resolutionY = 300 + 1;// DateTime.Now.Second;
            //    multiplier = 1;
            //    newX += screenToUse.Bounds.Width - GlobalState.resolutionX - 300;
            //    newY += screenToUse.Bounds.Height - GlobalState.resolutionY - 300;
            //    width = GlobalState.resolutionX + 300;
            //    height = GlobalState.resolutionY + 300;
            //}


            Slate newSlate = new Slate(slateId);;
            //newSlate.SizeMultiplier = multiplier;
            if (!GlobalState.RunningInPreview)
            {
                if (!GlobalState.RunningAsScreenSaver && Settings.Windowed)
                {
                    newX = (screenToUse.Bounds.Width - screenWidth) / 2;
                    newY = (screenToUse.Bounds.Height - screenHeight) / 2;
                    newSlate.FormBorderStyle = FormBorderStyle.Sizable;
                    newSlate.ControlBox = true;
                    newSlate.MinimizeBox = true;
                    newSlate.ShowIcon = true;
                    if (!Settings.KidSafe) newSlate.TopMost = false;
                }

                newSlate.PlaceWindow(newX, newY, width, height);
                newSlate.Show();
                newSlate.SetDesktopLocation(newX, newY);
                Application.DoEvents();
            }

            Debug.WriteLine("Initializing: " + displayWnd + " " + slateId);
            newSlate.Initialize(displayWnd);
            slates.Add(newSlate);
            Debug.WriteLine("Registering: " + newSlate.ToString() + " " + slateId);

            systemHandler.AddKeyHandlers(newSlate, newSlate.KeyUpHandler, newSlate.KeyDownHandler);
            systemHandler.AddMouseHandlers(newSlate, newSlate.Slate_MouseMove, newSlate.Slate_MouseUp, newSlate.Slate_MouseDown);
            return slateId;
        }

        #endregion

        #region Window killer
        const int WM_CLOSE = 0x10;
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Worker Method that watches for problems and fixes them
        /// </summary>
        /// <param name="state"></param>
        /// --------------------------------------------------------------------------
        private static void WatchDog(object state)
        {
            int numSlates = (int)state;
            DateTime[] hungStart = new DateTime[numSlates];
            for (int i = 0; i < slates.Count; i++)
            {
                hungStart[i] = DateTime.Now;
            }
            uint[] lastFrame = new uint[numSlates];

            while (running)
            {
                // Handle hung slates
                for (int i = 0; i < slates.Count; i++)
                {
                    if (slates[i].Frame != lastFrame[i])
                    {
                        hungStart[i] = DateTime.Now;
                        lastFrame[i] = slates[i].DVFrame;
                    }
                    else
                    {
                        TimeSpan span = DateTime.Now - hungStart[i];
                        if (span.TotalSeconds > 15)
                        {
                            mainThread.Abort();  
                        }
                    }
                }

                if (Settings.KidSafe)
                {
                    // Handle task manager window
                    IntPtr hwnd = FindWindow(null, "Windows Task Manager");
                    if (hwnd.ToInt32() != 0)
                    {
                        if (!GlobalState.Debugging)
                        {
                            SendMessage(hwnd, WM_CLOSE, new IntPtr(0), new IntPtr(0));
                            if (slates != null && slates.Count > 0) slates[0].SignalPasswordHint();
                        }
                    }

                    // Handle  sticky keys windows (press shift 5 times fast)
                    hwnd = FindWindow(null, "Sticky Keys");
                    if (hwnd.ToInt32() != 0)
                    {
                        SendMessage(hwnd, WM_CLOSE, new IntPtr(0), new IntPtr(0));
                    }
                }

                Thread.Sleep(10);
            }
        }

        #endregion
    }
}