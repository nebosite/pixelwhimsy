using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using DirectVarmint;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace DirectVarmint
{
    public delegate void RenderMethod();
    /// --------------------------------------------------------------------------
    /// <summary>
    /// This is the main factory for accessing an activating DirectVarmint classes
    /// </summary>
    /// --------------------------------------------------------------------------
    public class DVTools
    {
        static List<DVWindow> windows = new List<DVWindow>();
        static List<SoundPlayer> players = new List<SoundPlayer>();
        public static double ActualFramesPerSecond = 0;

        static bool firstCallToDrive = true;
        /// --------------------------------------------------------------------------
        /// <summary>
        /// Drive a windows form that uses DirectVarmint functionality
        /// </summary>
        /// <param name="form">The form to drive</param>
        /// <param name="renderMenthod">The method to call at the beginning of each frame</param>
        /// <param name="framesPerSecond">How often to update the from</param>
        /// --------------------------------------------------------------------------
        public static void DriveApplication(int framesPerSecond)
        {
            HiPerfTimer timer = new HiPerfTimer();
            double frameDelay = 1.0 / framesPerSecond;
            int frame = 0;
            DateTime frameTime = DateTime.Now;

            if (firstCallToDrive && windows.Count == 0)
            {
                firstCallToDrive = false;
                throw new ApplicationException("There are no DVWindows defined.  Make sure your form has created a DVWindow before calling DriveApplication().");
            }

            List<DVWindow> deadWindows = new List<DVWindow>();


            while (windows.Count > 0)
            {

                if(frame % framesPerSecond == 0)
                {
                    TimeSpan span = DateTime.Now - frameTime; 
                    ActualFramesPerSecond = framesPerSecond / span.TotalSeconds;
                    frameTime = DateTime.Now;
                }

                frame++;
                timer.Start();

                deadWindows.Clear();


                // Draw all active windows
                foreach (DVWindow window in windows)
                {
                    if (User32.IsWindow(window.owner) && User32.IsWindowVisible(window.owner))
                    {
                        if(window.AutoRender) window.Render();
                    }
                    else
                    {
                        deadWindows.Add(window);
                    }
                }

                // Remove closed windows
                foreach (DVWindow window in deadWindows)
                {
                    windows.Remove(window);
                }


                // Process regular windows events and wait for the next frame
                Application.DoEvents();

                double seconds = timer.ElapsedSeconds;
                if (seconds < frameDelay)
                {
                    int millisecondsToSleep = (int)((frameDelay - seconds) * 1000);
                    if (millisecondsToSleep > 0)
                        Thread.Sleep(millisecondsToSleep);
                }

                while (timer.ElapsedSeconds < frameDelay)
                {
                    Application.DoEvents();
                    Thread.Sleep(1);
                }
                
            }

            // Drive the sound loop
            foreach (SoundPlayer player in players)
            {
                player.Dispose();
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Create a DVWindow object
        /// </summary>
        /// <param name="windowsControl">The control to map the window object to</param>
        /// <param name="renderMethod">This method is called every frame from DriveApplication()</param>
        /// <param name="d3dRrenderMethod">This method is called so that 3d objects are
        /// rendered between the main buffer and the overlay buffer</param>
        /// <returns></returns>
        /// --------------------------------------------------------------------------
        public static DVWindow CreateDVWindow(Control control, RenderMethod renderMethod)
        {
            return CreateDVWindow(control, renderMethod, PixelBuffer.DEFAULTWIDTH, PixelBuffer.DEFAULTHEIGHT);
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Create a DVWindow object
        /// </summary>
        /// <param name="windowHandle">The control to map the window object to</param>
        /// <param name="renderMethod">This method is called every frame from DriveApplication()</param>
        /// <returns></returns>
        /// --------------------------------------------------------------------------
        public static DVWindow CreateDVWindow(Control control, RenderMethod renderMethod, int pixelBufferWidth, int pixelBufferHeight)
        {
            DVWindow newWindow = new DVWindow(control, renderMethod, pixelBufferWidth, pixelBufferHeight);
            windows.Add(newWindow);
            return newWindow;
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Create a soundplayer object (Each application only needs one)
        /// </summary>
        /// --------------------------------------------------------------------------
        public static SoundPlayer CreateSoundPlayer(Control windowsControl)
        {
            SoundPlayer newPlayer = new SoundPlayer(windowsControl);
            players.Add(newPlayer);
            return newPlayer;
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Gets a stream to a file or embedded resource.
        /// </summary>
        /// <param name="fileOrResourceName">Filename or full resource name (namespace.resourcename)</param>
        /// <returns>A stream handle to the data</returns>
        /// --------------------------------------------------------------------------
        public static Stream GetStream(string fileOrResourceName)
        {
            Stream stream = null;

            if (File.Exists(fileOrResourceName)) stream = File.OpenRead(fileOrResourceName);
            else stream = Assembly.GetEntryAssembly().GetManifestResourceStream(fileOrResourceName);

            if (stream == null) throw new ApplicationException("Cannot find file or resource: " + fileOrResourceName);
            
            return stream;
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Set the transparency bits in the paletter to "solid"
        /// </summary>
        /// <param name="fileOrResourceName">Filename or full resource name (namespace.resourcename)</param>
        /// <returns>A stream handle to the data</returns>
        /// --------------------------------------------------------------------------
        public  static void FixPalette(uint[] palette)
        {
            unchecked
            {
                for (int i = 0; i < palette.Length; i++)
                {
                    palette[i] |= 0xff000000;
                }
            }

        }
    }
}
