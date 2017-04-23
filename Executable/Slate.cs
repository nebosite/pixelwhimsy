using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using DirectVarmint;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace PixelWhimsy
{
    public enum ScreenMode
    {
        Normal,
        SaveScreen,
        LoadScreen,
        TextEntry
    }

    public partial class Slate : Form
    {
        #region Data
        uint frame, stressModeFrames, mouseFrame;
        HiPerfTimer hpTimer = new HiPerfTimer();
        DVWindow dvWindow = null;
        DVWindow dvWindowBrushes = null;
        DVWindow dvWindowAnimations = null;
        DVWindow dvWindowModulators = null;
        bool expired = false;
        bool anyKeyExits = false;
        bool showStats = true;
        //bool showKeypressData = false;
        bool passWordHint = false;
        public bool isPrimaryWindow = false;
        List<Animation> animationQueue = new List<Animation>();
        PixelBuffer.Sprite colorPickerSprite = null;
        PixelBuffer.Sprite currentScreen = null;
        PixelBuffer.Sprite[] savedScreens = new PixelBuffer.Sprite[16];
        ScreenMode screenMode = ScreenMode.Normal;
        bool mouseMotion;
        bool stressMode = false;
        bool freezeAnimations = false;
        Array possibleKeyValues = Enum.GetValues(typeof(Keys));
        int targetX;
        int targetY;
        int modulator = 0;
        Dictionary<Keys, int> keypressLog = new Dictionary<Keys, int>();
        Dictionary<Type, int> animationLog = new Dictionary<Type, int>();
        int sizeMultiplier = 1;
        int lastAnimationQueueCount = -1;
        BrushType lastBrushType = BrushType.Circle;
        bool insideToolbar = false;
        bool insideExitButton = false;
        DateTime nextRandomBrushTime = DateTime.Now;
        int skipMouseDraws = 0;

        bool colorPicker = false;

        double millisecondsLocalRender;
        double millisecondsGlobalRender;

        public uint Frame { get { return this.frame; } }
        public uint DVFrame { get { return this.dvWindow.RenderFrame; } }

        public int SizeMultiplier { get { return this.sizeMultiplier; } set { this.sizeMultiplier = value; } }

        /// <summary>
        /// A numberic code for helping determine if the state has changed
        /// </summary>
        public int GeneralStateCode
        {
            get
            {
                int state = 0;
                if (freezeAnimations) state += 0x01;
                if (screenMode == ScreenMode.LoadScreen) state += 0x02;
                if (screenMode == ScreenMode.SaveScreen) state += 0x04;
                if (screenMode == ScreenMode.TextEntry) state += 0x08;
                if (GlobalState.PaintingStyle == PaintingStyle.Kaleidoscope) state += 0x10;
                if (GlobalState.PaintingStyle == PaintingStyle.Tile) state += 0x20;
                if (colorPicker) state += 0x40;
                if (GlobalState.RandomBrush) state += 0x80;
                return state;
            }
        }
        #endregion

        #region Rendering

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Use this method to render 3d objects
        /// </summary>
        /// --------------------------------------------------------------------------
        public void My3DRenderMethod()
        {

        }

        public delegate void RenderMethod();
        bool startRender = false;
        DateTime lastGoodRenderTime = DateTime.Now;
        Thread renderThread;
        List<Exception> renderExceptions = new List<Exception>();
        List<DateTime> renderExceptionTimes = new List<DateTime>();
        DateTime lastFpsCheckTime = DateTime.Now;
        int lastGeneralStateCode = 0;

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Start up a thread that handles rendering.
        /// </summary>
        /// --------------------------------------------------------------------------
        public void StartThreadedRender()
        {
            if (renderThread != null) AbortThreadedRender();
            ParameterizedThreadStart threadStart = new ParameterizedThreadStart(MyRenderWorker);
            renderThread = new Thread(threadStart);
            renderThread.Start((RenderMethod)MyThreadedRenderMethod);
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Abort the rendering thread (used in the case of a detected hang).
        /// </summary>
        /// --------------------------------------------------------------------------
        public void AbortThreadedRender()
        {
            lock (this)
            {
                if (renderThread != null)
                {
                    renderThread.Abort();
                    //renderThread.Join(9000);
                    renderThread = null;
                }
            }
        }

        double actualFps = 1;
        /// --------------------------------------------------------------------------
        /// <summary>
        /// Handle Rendering in a safe way and recover from various program errors.
        /// </summary>
        /// --------------------------------------------------------------------------
        public void MyRenderMethod()
        {
            if (frame % 100 == 0) GC.Collect(2);
            if (frame > 1 && isPrimaryWindow && this.labelPleaseWait.Visible)
            {
                this.labelPleaseWait.Hide();

            }

            if (showStats && (frame % 30 == 0))
            {
                TimeSpan checkSpan = DateTime.Now - lastFpsCheckTime;
                actualFps = 30 / checkSpan.TotalSeconds;
                labelFps.Text = "#A: " + animationQueue.Count + "  FPS: " + actualFps.ToString(".0");
                lastFpsCheckTime = DateTime.Now;
            }

            if (GlobalState.EndApplication)
            {
                dvWindow.Close();
                dvWindowAnimations.Close();
                dvWindowBrushes.Close();
                dvWindowModulators.Close();
                AbortThreadedRender();
                Close();
                return;
            }

            lock (renderExceptions)
            {
                while (renderExceptions.Count > 0 && renderExceptionTimes[0] < DateTime.Now.AddSeconds(-30))
                {
                    renderExceptions.RemoveAt(0);
                    renderExceptionTimes.RemoveAt(0);
                }

                if (renderExceptions.Count > 3)
                {
                    AbortThreadedRender();
                    throw new TooManyExceptions("Too many exceptions.", renderExceptions[renderExceptions.Count-1]);
                }
            }

            HandleStressMode();
            HandleScreenSaverActivity();

            startRender = true;
            lastGoodRenderTime = DateTime.Now;
            while (startRender)
            {
                TimeSpan timeSinceLastGoodRender = DateTime.Now - lastGoodRenderTime;
#if !DEBUG
                if (timeSinceLastGoodRender.TotalSeconds > 5)
                {
                    startRender = false;
                    AbortThreadedRender();
                    StopAnimations(null);
                    StartThreadedRender();
                }
#endif
                Thread.Sleep(1);
            }

        }

        int savedExceptions = 0;
        /// --------------------------------------------------------------------------
        /// <summary>
        /// Call the renderer when signalled to do so
        /// </summary>
        /// --------------------------------------------------------------------------
        public void MyRenderWorker(object state)
        {
            RenderMethod renderMethod = (RenderMethod)state;
            while (true)
            {
                if(startRender)
                {
                    try
                    {
                        renderMethod();
                        HandleStressMode();

                        startRender = false;
                    }
                    catch (Exception e)
                    {
                        savedExceptions++;
                        Utilities.LogException(e);  
                        

                        lock (renderExceptions)
                        {
                            renderExceptionTimes.Add(DateTime.Now);
                            renderExceptions.Add(e);
                        }
                        StopAnimations(null);
                    }
                }
                Thread.Sleep(1);
            }
        }

        uint renderBrushFrame = 20;
        uint renderAnimationFrame = 20;
        uint renderModulatorsFrame = 20;

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Render a Frame
        /// </summary>
        /// --------------------------------------------------------------------------
        public void MyThreadedRenderMethod()
        {
            frame++;
            if (frame == 20 && isPrimaryWindow && !GlobalState.RunningInPreview)
            {
                passWordHint = true;
                DrawLogo();
                ShowVersion();
            }

            if (dvWindowAnimations != null && dvWindowAnimations.WindowWasReset)
            {
                dvWindowAnimations.WindowWasReset = false;
                renderAnimationFrame = frame + 60;
            }

            if (dvWindowBrushes != null && dvWindowBrushes.WindowWasReset)
            {
                dvWindowBrushes.WindowWasReset = false;
                renderBrushFrame = frame + 60;
            }

            if (dvWindowModulators != null && dvWindowModulators.WindowWasReset)
            {
                dvWindowModulators.WindowWasReset = false;
                renderModulatorsFrame = frame + 60;
            }

            if (lastAnimationQueueCount != AnimationTypeCount())
            {
                lastAnimationQueueCount = AnimationTypeCount();
                renderAnimationFrame = frame;
                renderBrushFrame = frame;
            }

            if (lastGeneralStateCode != GeneralStateCode || lastBrushType != GlobalState.BrushType)
            {
                lastGeneralStateCode = GeneralStateCode;
                lastBrushType = GlobalState.BrushType;
                renderBrushFrame = frame;
            }

            if (renderBrushFrame == frame)
            {
                renderBrushFrame += (uint)(actualFps * 2);
                RenderBrushToolbar();
            }

            if (renderModulatorsFrame == frame)
            {
                renderModulatorsFrame += (uint)(actualFps * 2);
                RenderModulatorToolbar();
            }

            if (renderAnimationFrame == frame)
            {
                renderAnimationFrame += (uint)(actualFps * 2);
                RenderAnimationToolbar();
            }



            if (GlobalState.EndApplication)
            {
                return;
            }

            ExecuteDelayedActions();

            if (GlobalState.RandomBrush && DateTime.Now > nextRandomBrushTime) SelectRandomBrush();

            List<EventArgs> keyEvents;
            List<EventArgs> mouseUpEvents;
            List<EventArgs> mouseDownEvents;
            List<EventArgs> mouseMoveEvents;
            // Handle any keystrokes
            lock (eventsToDoLockHandle)
            {
                keyEvents = keyboardEventsToDo;
                keyboardEventsToDo = new List<EventArgs>();

                mouseUpEvents = mouseUpEventsToDo;
                mouseUpEventsToDo = new List<EventArgs>();

                mouseDownEvents = mouseDownEventsToDo;
                mouseDownEventsToDo = new List<EventArgs>();

                mouseMoveEvents = mouseMoveEventsToDo;
                mouseMoveEventsToDo = new List<EventArgs>();
            }

            foreach (EventArgs thisEvent in keyEvents)
            {
                Safe_Slate_KeyHandler(null, thisEvent);
            }
            foreach (EventArgs thisEvent in mouseUpEvents)
            {
                Safe_Slate_MouseUp(null, thisEvent);
            }
            foreach (EventArgs thisEvent in mouseDownEvents)
            {
                Safe_Slate_MouseDown(null, thisEvent);
            }
            foreach (EventArgs thisEvent in mouseMoveEvents)
            {
                Safe_Slate_MouseMove(null, thisEvent);
            }
 
            mouseMotion = mouseMoveCounter > 0;
            mouseVelocityX *= 0.8;
            mouseVelocityY *= 0.8;
            if (isPrimaryWindow)
            {
                if (GlobalState.BrushSize > GlobalState.TargetBrushSize) DecreaseBrushSize();
                if (GlobalState.BrushSize < GlobalState.TargetBrushSize) IncreaseBrushSize();
            }


            switch (screenMode)
            {
                case ScreenMode.SaveScreen: RenderScreen_ScreenIO(); break;
                case ScreenMode.LoadScreen: RenderScreen_ScreenIO(); break;
                default: RenderScreen_Normal(); break;
            }
        }

        string currentStressText = null;

        //------------------------------------------------------------	OnDraw()
        //
        List<Point> GetCurve(List<Point> hullPoints, int numPoints)
        {
            List<Point> newPoints = new List<Point>();

            // for each section of curve, draw LOD number of divisions
            for (int i = 0; i != numPoints; ++i)
            {
                // "time" 0-1
                float t = (float)i / numPoints;

                // inverted t
                float it = 1.0f - t;

                // calculate blending functions for cubic bspline
                float b0 = it * it * it / 6.0f;
                float b1 = (3 * t * t * t - 6 * t * t + 4) / 6.0f;
                float b2 = (-3 * t * t * t + 3 * t * t + 3 * t + 1) / 6.0f;
                float b3 = t * t * t / 6.0f;

                // calculate the x,y and z of the curve point
                float x = b0 * hullPoints[0].X +
                          b1 * hullPoints[1].X +
                          b2 * hullPoints[2].X +
                          b3 * hullPoints[3].X;

                float y = b0 * hullPoints[0].Y +
                          b1 * hullPoints[1].Y +
                          b2 * hullPoints[2].Y +
                          b3 * hullPoints[3].Y;

                // specify the point
                newPoints.Add(new Point((int)x, (int)y));
            }

            return newPoints;
        }

        List<Point> pointsToMoveTo = new List<Point>();
        List<Point> pointFrame = new List<Point>();
        /// --------------------------------------------------------------------------
        /// <summary>
        /// In screensaver mode, we want to periodically press a key or something
        /// </summary>
        /// --------------------------------------------------------------------------
        void HandleScreenSaverActivity()
        {
            if (!GlobalState.RunningAsScreenSaver) return;

            if (!Settings.Registered)
            {
                StopAnimations(null);
                StopDelayedActions();
                dvWindow.MainBuffer.Clear(MediaBag.color_DarkGreen);
                string text =
                    "Screen saver mode is \n" +
                    "enabled in the registered version\n" +
                    "of PixelWhimsy.  Please register\n" +
                    "at www.pixelwhimsy.com.";

                DrawCenteredText(dvWindow.MainBuffer, text, MediaBag.color_White);
                return;
            }

            // Mute the screensaver
            if (Settings.MuteScreenSaverVolume) MediaBag.Mute = true;

            // If the user is active, turn the sound back on and skip screensaver activity
            if (Settings.PlayableScreensaver && DateTime.Now < lastActivityTime.AddMinutes(5))
            {
                MediaBag.Mute = false;
                return;
            }

            // Handle artificial mouse motion
            if (pointsToMoveTo.Count == 0)
            {
                while (pointFrame.Count < 4)
                {
                    pointFrame.Add(
                        new Point(Utilities.Rand(dvWindow.MainBuffer.Width + 100) - 50, 
                        Utilities.Rand(dvWindow.MainBuffer.Height + 100)- 50));
                }

                pointsToMoveTo = GetCurve(pointFrame, 20);
                pointFrame.RemoveAt(0);
            }

            Point p = pointsToMoveTo[0];
            pointsToMoveTo.RemoveAt(0);
            lastMouseX = mouseX;
            lastMouseY = mouseY;
            if (leftMouse)
            {
                HandleMouseMove(0, p.X, p.Y);
            }
            else
            {
                mouseX = p.X;
                mouseY = p.Y;
            }

            // Mouse clicks
            if (Utilities.Rand(100) == 0)
            {
                if (leftMouse) HandleMouseUp(MouseButtons.Left);
                else
                {
                    HandleMouseDown(MouseButtons.Left);
                }
            }

            // Keystrokes
            int probability = 50;
            if (GlobalState.RunningInPreview) probability = 300;
            if (Utilities.Rand(1000) > probability) return;

            KeyAction action = (KeyAction)Utilities.Rand((int)KeyAction.NumberOfKeyActions);
            switch(action)
            {
                case KeyAction.ColorCounter: break;
                case KeyAction.ColorPicker: break;
                case KeyAction.FreezeAnimations: break;
                case KeyAction.FunKey: break;
                case KeyAction.Help: break;
                case KeyAction.PasswordHint: break;
                case KeyAction.ScreenLoad: break;
                case KeyAction.ScreenStore: break;
                case KeyAction.ShowVersion: break;
                case KeyAction.StressMode: break;
                case KeyAction.TextEditor: break;
                case KeyAction.WorkingPoints: break;
                case KeyAction.VolumeDown: break;
                case KeyAction.VolumeMute: break;
                case KeyAction.VolumeUp: break;
                default: GenerateKeyPress(action); break;
            }
        }


        /// --------------------------------------------------------------------------
        /// <summary>
        /// In stress mode, we try to crash the application by pressing lots of keys
        /// moving the mouse and doing lots of crazy stuff
        /// </summary>
        /// --------------------------------------------------------------------------
        private void HandleStressMode()
        {
            if (stressMode)
            {
                int r = Utilities.Rand(100);
                if (r < 80)
                {
                    KeyEventArgs eventArgs;
                    eventArgs = new KeyEventArgs((Keys)(possibleKeyValues.GetValue(Utilities.Rand(possibleKeyValues.Length))));

                    if (screenMode == ScreenMode.TextEntry)
                    {
                        if (currentStressText == null) currentStressText = MediaBag.madLib.GetString();

                        char c = '\n';
                        if (currentStressText.Length > 0)
                        {
                            c = currentStressText[0];
                            currentStressText = currentStressText.Substring(1);
                        }
                        else
                        {
                            currentStressText = null;
                            keyPressData[Keys.LShiftKey] = 0;
                            keyPressData[Keys.RShiftKey] = 0;
                        }

                        Keys keyCode = Keys.Space;
                        if (keyReverseTranslations.ContainsKey(char.ToLower(c)))
                        {
                            keyCode = keyReverseTranslations[char.ToLower(c)];
                        }

                        eventArgs = new KeyEventArgs(keyCode);
                        eventArgs.SuppressKeyPress = true;

                        HandleKeys_TextEntry(eventArgs);
                    }
                    else
                    {

                        eventArgs.SuppressKeyPress = true;

                        if (KeyIsPressed(eventArgs.KeyCode)) KeyUpHandler(null, eventArgs);
                        else KeyDownHandler(null, eventArgs);
                    }
                }
                else
                {
                    MouseButtons buttons = MouseButtons.None;
                    int x = Utilities.Rand(this.Width);
                    int y = Utilities.Rand(this.Height);
                    int delta = Utilities.Rand(3) - 1;

                    if (Utilities.Rand(5) == 0)
                    {
                        switch (Utilities.Rand(3))
                        {
                            case 0: buttons = MouseButtons.Left; break;
                            case 1: buttons = MouseButtons.Left; break;
                            case 2: buttons = MouseButtons.Left; break;
                            default: break;
                        }
                    }

                    MouseEventArgs mouseArgs = new MouseEventArgs(buttons, 0, x, y, delta);

                    if(buttons == MouseButtons.None) 
                    {
                        Slate_MouseMove(null, mouseArgs);
                    }
                    else
                    {
                        if((buttons == MouseButtons.Left && leftMouse) ||
                            (buttons == MouseButtons.Right && rightMouse) ||
                            (buttons == MouseButtons.Middle && middleMouse))
                        {
                            Slate_MouseUp(null, mouseArgs);
                        }
                        else
                        {
                            Slate_MouseDown(null, mouseArgs);
                        }
                            
                    }
                }
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// --------------------------------------------------------------------------
        void RenderScreen_ScreenIO()
        {
            AnimatePalette();
            ScreenPickerAnimation();
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Normal Game operation
        /// </summary>
        /// --------------------------------------------------------------------------
        private void RenderScreen_Normal()
        {

            hpTimer.Start();
            if (stressMode) stressModeFrames++;
            LogAnimations();
            AnimatePalette();

            if (mouseMoveCounter > 0) mouseMoveCounter--;

            DrawMouseCursor();


            RenderPressedKeys();
            RenderAnimations();
            Experiment();

            // Certain types of painting happen even when the mouse is still
            //if (GlobalState.BrushType == BrushType.SprayPaint)
            if (skipMouseDraws == 0)
            {
                HandleMouse_Normal();
            }
            else skipMouseDraws--;

            if (drawSoundInstance != null)
            {
                drawSoundFrame--;
                if (drawSoundFrame > 6) drawSoundFrame = 6;
                if (drawSoundFrame < 0) drawSoundFrame = 0;
                drawSoundInstance.Volume = drawSoundFrame * .05;
            }

            if (expired)
            {
                dvWindow.OverlayBuffer.Clear(0);
                dvWindow.MainBuffer.Clear(MediaBag.color_DarkRed);
                string expiredText =
                    "PixelWhimsy" + (char)0x99 + " Version " + AssemblyConstants.Version + "\n\n" +
                    "This pre-release version has expired. Please visit \n" +
                    "www.pixelwhimsy.com for an updated copy.  \n\nPress any key to exit this program.";

                dvWindow.MainBuffer.Print(MediaBag.color_White, MediaBag.font_Status, 10, 10, expiredText);
                anyKeyExits = true;
            }

            ShowDebuggingStuff();

            double duration = hpTimer.ElapsedSeconds;
            millisecondsLocalRender *= .9;
            millisecondsLocalRender += duration * 1000.0 / 10.0;
            millisecondsGlobalRender *= .9;
            millisecondsGlobalRender += dvWindow.RenderMilliseconds / 10.0;
        }

        Dictionary<Type, bool> animationTypeHolder = new Dictionary<Type, bool>();
        /// --------------------------------------------------------------------------
        /// <summary>
        /// Keep track of how many frames animations are active
        /// </summary>
        /// --------------------------------------------------------------------------
        private void LogAnimations()
        {
            if (stressMode) return;
            
            animationTypeHolder.Clear();
            lock (animationQueueLockHandle)
            {

                foreach (Animation animation in animationQueue)
                {
                    animationTypeHolder[animation.GetType()] = true;
                }
            }

            foreach (Type type in animationTypeHolder.Keys)
            {
                if (!animationLog.ContainsKey(type)) animationLog.Add(type, 0);
                animationLog[type]++;
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// --------------------------------------------------------------------------
        void ShowDebuggingStuff()
        {
            if (daysLeft < 1000 && frame < 300)
            {
                string text = "This evaluation copy will expire in " + daysLeft.ToString(".0") + " days.";
                SizeF textSize = MediaBag.font_Status.Measure(text);
                if (frame == 299)
                {
                    dvWindow.OverlayBuffer.DrawFilledRectangle(0, 0, 0, (int)textSize.Width, (int)textSize.Height);
                }
                else
                {
                    dvWindow.OverlayBuffer.DrawFilledRectangle(MediaBag.color_DarkRed, 0, 0, (int)textSize.Width, (int)textSize.Height);
                    dvWindow.OverlayBuffer.Print(MediaBag.color_Yellow, MediaBag.font_Status, 0, 0, text);
                }
            }
#if DEBUG
            if (savedExceptions > 0)
            {
                dvWindow.OverlayBuffer.DrawFilledRectangle(MediaBag.color_Yellow, 2, 2, 100, 15);
                dvWindow.OverlayBuffer.Print(0, MediaBag.font_Status, 2, 2, "Exceptions: " + savedExceptions);
            }
#endif

            // Show brush number
            //dvWindow.OverlayBuffer.DrawFilledRectangle(MediaBag.color_Yellow, 2, 2, 50, 50);
            //dvWindow.OverlayBuffer.Print(MediaBag.color_Blue, MediaBag.font_Text, 10, 10, GlobalState.BrushSize.ToString());

            //dvWindow.OverlayBuffer.DrawFilledRectangle(MediaBag.color_Yellow, 2, 2, 50, 100);
            //if (leftMouse) dvWindow.OverlayBuffer.Print(0, MediaBag.font_Status, 2, 2, "LeftMouse");

            //if (cursorVisible) dvWindow.OverlayBuffer.Print(0, MediaBag.font_Status, 2, 2, "Visible");
            //if (AltIsPressed()) dvWindow.OverlayBuffer.Print(0, MediaBag.font_Status, 2, 12, "Alt");
            //if (CtrlIsPressed()) dvWindow.OverlayBuffer.Print(0, MediaBag.font_Status, 2, 22, "Ctrl");
            //if (ShiftIsPressed()) dvWindow.OverlayBuffer.Print(0, MediaBag.font_Status, 2, 32, "Shift");

            //if (showKeypressData)
            //{
            //    for (int i = 0; i < 120; i++)
            //    {
            //        int x = (i % 12) * 10 + 100;
            //        int y = (i / 12) * 10 + 50;

            //        if (KeyIsPressed((Keys)i))
            //        {
            //            dvWindow.MainBuffer.DrawFilledCircle(Color.White, x, y, 4);
            //        }
            //        else
            //        {
            //            dvWindow.MainBuffer.DrawFilledCircle(Color.DarkGray, x, y, 4);
            //            dvWindow.MainBuffer.DrawCircle(Color.White, x, y, 4);
            //        }
            //    }
            //}
        }


        //Animation.Rain.RainDrop testRainDrop = null;
        //bool didRun = false;

        /// --------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// --------------------------------------------------------------------------
        void Experiment()
        {
            //for (int i = 0; i < dvWindow.Width; i++)
            //{
            //    ushort c = (ushort)(frame + i);
            //    dvWindow.MainBuffer.DrawLine((ushort)c, i, 0, i, 100);
            //}

            //if (testRainDrop == null)
            //{
            //    for (int i = 0; i < 20; i++)
            //    {
            //        dvWindow.MainBuffer.DrawLine(Color.White, 20, 40 + i * 2, 90, 40 + i * 2);
            //    }
            //    for (int i = 0; i < 50; i++)
            //    {
            //        dvWindow.MainBuffer.DrawLine(Color.Blue, 20 + i * 2, 40, 20 + i * 2, 80);
            //    }

            //    testRainDrop = new Animation.Rain.RainDrop(50, 50, 0, 1, 1);
            //    testRainDrop.RenderRun(dvWindow);
            //}

            //if(KeyIsPressed(Keys.D1)) 
            //{
            //    if (!didRun)
            //    {
            //        testRainDrop.RenderRun(dvWindow);
            //        didRun = true;
            //    }

            //}
            //else
            //{
            //    didRun = false;
            //}


            //for (int x = 0; x < 26; x++)
            //{
            //    for (int y = 0; y < 20; y++)
            //    {
            //        ushort c = dvWindow.MainBuffer.GetPixel(x + 37, y + 45);
            //        dvWindow.MainBuffer.DrawFilledRectangle(c, x * 10 + 100, y * 10 + 100, x * 10 + 100 + 9, y * 10 + 100 + 9);
            //    }
            //}

        }

        int lastAnimationCount = -1;
        object animationQueueLockHandle = new object();

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Show any animations in the animation queue
        /// </summary>
        /// --------------------------------------------------------------------------
        private void RenderAnimations()
        {
            lock (animationQueueLockHandle)
            {
                if (lastAnimationCount != animationQueue.Count) freezeAnimations = false;

                if (!colorPicker && !freezeAnimations)
                {
                    for (int i = 0; i < animationQueue.Count; )
                    {
                        animationQueue[i].Render();
                        if (animationQueue[i].IsDone)
                        {
                            animationQueue.RemoveAt(i);
                        }
                        else i++;
                    }
                }

                lastAnimationCount = AnimationTypeCount();
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Show the number of types of active animations
        /// </summary>
        /// --------------------------------------------------------------------------
        int AnimationTypeCount()
        {
            lock (animationQueueLockHandle)
            {
                Dictionary<Type, bool> animationIndex = new Dictionary<Type, bool>();
                foreach (Animation a in animationQueue)
                {
                    animationIndex[a.GetType()] = true;   
                }
                return animationIndex.Count;
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Handle keys that are pressed
        /// </summary>
        /// --------------------------------------------------------------------------
        private void RenderPressedKeys()
        {

            uint framesElapsed = frame - GetKeyPressFrame(Keys.Up);
            if (KeyIsPressed(Keys.Up) && (framesElapsed > 5 || framesElapsed == 1) && textAnimator == null)
            {
                Animation instructions = GetFirstAnimation(typeof(Animation.Instructions));
                if (instructions != null)
                {
                    MediaBag.Play(SoundID.Dot01);
                    mouseY -= 10;
                    if (mouseY >= dvWindow.Height) mouseY = dvWindow.Height-1;
                    instructions.MouseY = mouseY;
                    lastMouseY = mouseY;
                }
                else
                {
                    IncreaseBrushSize();
                    GlobalState.TargetBrushSize = GlobalState.BrushSize;
                    SetMouseMoveCounter();
                }
            }

            framesElapsed = frame - GetKeyPressFrame(Keys.Down);
            if (KeyIsPressed(Keys.Down) && (framesElapsed > 5 || framesElapsed == 1) && textAnimator == null)
            {
                Animation instructions = GetFirstAnimation(typeof(Animation.Instructions));
                if (instructions != null)
                {
                    MediaBag.Play(SoundID.Dot01);
                    mouseY += 10;
                    if (mouseY <0) mouseY = 0;
                    instructions.MouseY = mouseY;
                    lastMouseY = mouseY;
                }
                else
                {
                    DecreaseBrushSize();
                    GlobalState.TargetBrushSize = GlobalState.BrushSize;
                    SetMouseMoveCounter();
                }
            }

            if (passWordHint)
            {
                passWordHint = false;
                if (!AnimationExists(typeof(Animation.PasswordHint)))
                {
                    AddAnimation(new Animation.PasswordHint(dvWindow, Settings.ExitHint));
                }
            }
           
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// decrease the size of the current brush
        /// </summary>
        /// --------------------------------------------------------------------------
        private static void DecreaseBrushSize()
        {
            GlobalState.BrushSize--;
            MediaBag.Play(SoundID.Dot17, 1.0 / (GlobalState.BrushSize / 50.0 + 0.5), .1);
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Increase the size of the current brush
        /// </summary>
        /// --------------------------------------------------------------------------
        private static void IncreaseBrushSize()
        {
            GlobalState.BrushSize++;
            MediaBag.Play(SoundID.Dot17, 1.0 / (GlobalState.BrushSize / 50.0 + 0.5), .1);
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// indication if the specified animation type is already in the queue
        /// </summary>
        /// --------------------------------------------------------------------------
        private bool AnimationExists(Type type)
        {
            lock (animationQueueLockHandle)
            {
                foreach (Animation animation in animationQueue)
                {
                    if (animation.GetType() == type) return true;
                }
            }

            return false;
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// A count of the number of active animations of a particular type
        /// </summary>
        /// --------------------------------------------------------------------------
        private int AnimationCount(Type type)
        {
            int count = 0;
            lock (animationQueueLockHandle)
            {
                foreach (Animation animation in animationQueue)
                {
                    if (animation.GetType() == type) count++;
                }
            }

            return count;
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Get a handle to a running animation
        /// </summary>
        /// --------------------------------------------------------------------------
        private Animation GetFirstAnimation(Type type)
        {
            lock (animationQueueLockHandle)
            {
                foreach (Animation animation in animationQueue)
                {
                    if (animation.GetType() == type) return animation;
                }
            }
            return null;
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Get a handle to a running animation
        /// </summary>
        /// --------------------------------------------------------------------------
        private List<Animation> GetAllAnimations(Type type)
        {
            List<Animation> found = new List<Animation>();
            lock (animationQueueLockHandle)
            {
                foreach (Animation animation in animationQueue)
                {
                    if (animation.GetType() == type) found.Add(animation);
                }
            }
            return found;
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Stop all animations of the specified type
        /// </summary>
        /// <param name="type"></param>
        /// --------------------------------------------------------------------------
        private void StopAnimations(Type type)
        {
            lock (animationQueueLockHandle)
            {
                foreach (Animation animation in animationQueue)
                {
                    if (type == null || animation.GetType() == type) animation.IsDone = true;
                }
            }

            RenderAnimations();
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Safely add an animation
        /// </summary>
        /// --------------------------------------------------------------------------
        private void AddAnimation(Animation animation)
        {
            lock (animationQueueLockHandle)
            {
                animationQueue.Add(animation);
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Draw the PixelWhimsy logo
        /// </summary>
        /// --------------------------------------------------------------------------
        public void DrawLogo()
        {
            SizeF pixelSize = MediaBag.font_LogoPixel.Measure("Pixel");
            SizeF whimsySize = MediaBag.font_LogoWhimsy.Measure("Whimsy");
            int w = (int)(pixelSize.Width + whimsySize.Width);
            int h = (int)whimsySize.Height;
            int x = (int)((dvWindow.MainBuffer.Width - w) / 2);
            int y = (int)((dvWindow.MainBuffer.Height - h) / 2);
            DrawLogo(x,y);
        }

        public void DrawLogo(int x, int y)
        {
            if (!AnimationExists(typeof(Animation.Logo)))
            {
                SoundID logoSound = SoundID.PixelWhimsy2;
                switch (Utilities.Rand(3))
                {
                    case 0: logoSound = SoundID.PixelWhimsy2; break;
                    case 1: logoSound = SoundID.PixelWhimsy3; break;
                    default: logoSound = SoundID.PixelWhimsy5; break;
                }
                AddAnimation(new Animation.Logo(dvWindow, x, y, logoSound));
            }
        }


        /// --------------------------------------------------------------------------
        /// <summary>
        /// CurrentWingding character
        /// </summary>
        /// --------------------------------------------------------------------------
        public char GetWinddingChar()
        {
            return (char)((modulator * 50 + GlobalState.BrushSize) % 223 + 33); 
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Draw a Wingding
        /// </summary>
        /// --------------------------------------------------------------------------
        public void DrawWingding(int x, int y, ushort color)
        {
            int size = 60 * ((modulator/4) + 1);

            MediaBag.SetWingdingFont(size);
            dvWindow.MainBuffer.Print(
                color,
                MediaBag.font_Wingding,
                x - size / 2,
                y - size / 2,
                new string(GetWinddingChar(), 1));
        }

        /// <summary>
        /// Funny things
        /// </summary>
        string[] thingsToSend = new string[]
            {
                "missing socks",
                "figs",
                "prosthetic implants",
                "expired parking tickets",
                "organic fruit",
                "baby eels",
                "blurry photographs",
                "refrigerator magnets",
                "idempotent operations",
                "mirth",
                "used post-its",
                "Swiss chocolate",
                "lucky rabbit's feet",
                "ornamental shrubbery",
                "astro-turf fertilizer",
                "day-glow upholstery",
                "unread Nancy Drew novels",
                "cabbage recipes",
                "Little Debbie cakes",
                "dremel bits",
                "peer pressure",
                "Sammy Davis Jr. albums",
                "Cap’n Crunch box tops",
                "potted plants",
                "breathable air",
                "that feeling you've been here before",
                "microwave popcorn",
                "a shred of decency",
                "a train of thought",
                "mercury-free tunafish",
            };

        int lastMessageX = 0;
        int lastMessageY = 0;
        string lastMessageText = null;

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Show the current version and any special messages
        /// </summary>
        /// --------------------------------------------------------------------------
        public void ShowVersion()
        {
            string version = "Version " + AssemblyConstants.Version;
            int textWidth = (int)MediaBag.font_Status.Measure(version).Width;
            lastMessageY = dvWindow.MainBuffer.Height / 2 + 20;

            dvWindow.MainBuffer.Print(
                0,
                MediaBag.font_Status,
                (dvWindow.MainBuffer.Width - textWidth) / 2, lastMessageY, version);
            dvWindow.MainBuffer.Print(
                MediaBag.color_White,
                MediaBag.font_Status,
                (dvWindow.MainBuffer.Width - textWidth) / 2 - 1, lastMessageY - 1, version);

            lastMessageY += 10;
            string otherText = "";

            if (GlobalState.Debugging)
            {
                otherText = "This is a pre-release version of PixelWhimsy" + (char)0x99 + "\n" +
                                    "Please send comments, sincere praise, shameless flattery, \n" +
                                    "gifts, money, and " + thingsToSend[Utilities.Rand(thingsToSend.Length)] + " to:\n" +
                                    "\n                            eric@pixelwhimsy.com\n" +
                                    "\nSoftware updates can be found at www.pixelwhimsy.com.";
            }
            else
            {
                otherText = "Every key does something different and special...\n" +
                            "               Explore!  Have fun!  Play!";
            }

            textWidth = (int)MediaBag.font_Text.Measure(otherText).Width;

            if (lastMessageText != null)
            {
                dvWindow.MainBuffer.Print(
                    0,
                    MediaBag.font_Text,
                    lastMessageX - 1, lastMessageY + 17 , lastMessageText);
            }

            lastMessageX = (dvWindow.MainBuffer.Width - textWidth) / 2;
            lastMessageY += 18;
            lastMessageText = otherText;

            dvWindow.MainBuffer.Print(
                0,
                MediaBag.font_Text,
                    lastMessageX, lastMessageY, lastMessageText + "\n");
            dvWindow.MainBuffer.Print(
                MediaBag.color_Gray,
                MediaBag.font_Text,
                    lastMessageX - 1, lastMessageY - 1, lastMessageText);
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Flip the screen vertically
        /// </summary>
        /// --------------------------------------------------------------------------
        void FlipVertical()
        {
            int spot1 = 0;
            int spot2 = (dvWindow.MainBuffer.Height - 1) * dvWindow.MainBuffer.BufferPitch;
            ushort temp;
            ushort[] buffer = dvWindow.MainBuffer.RawBuffer;

            int ystart = 0;
            int yend = dvWindow.MainBuffer.Height - 1;
            int xstart = 0;
            int xend = dvWindow.MainBuffer.Width - 1;

            GetSortedLimits(ref ystart, ref yend, ref xstart, ref xend);

            if (ystart < 0) ystart = 0;
            if (xstart < 0) xstart = 0;
            if (ystart >= dvWindow.MainBuffer.Height) ystart = dvWindow.MainBuffer.Height - 1;
            if (xstart >= dvWindow.MainBuffer.Width) xstart = dvWindow.MainBuffer.Width - 1;
            if (yend < 0) yend = 0;
            if (xend < 0) xend = 0;
            if (yend >= dvWindow.MainBuffer.Height) yend = dvWindow.MainBuffer.Height - 1;
            if (xend >= dvWindow.MainBuffer.Width) xend = dvWindow.MainBuffer.Width - 1;

            int ywidth = (yend - ystart)/2;

            for (int i = 0; i < ywidth; i++)
            {
                int y = ystart + i;
                spot1 = (y) * dvWindow.MainBuffer.BufferPitch + xstart;
                spot2 = (yend - i) * dvWindow.MainBuffer.BufferPitch + xstart;
                for (int x = xstart; x <= xend; x++)
                {
                    temp = buffer[spot1];
                    buffer[spot1] = buffer[spot2];
                    buffer[spot2] = temp;
                    spot1++;
                    spot2++;
                }
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Helper method to sort the working points
        /// </summary>
        /// --------------------------------------------------------------------------
        private void GetSortedLimits(ref int ystart, ref int yend, ref int xstart, ref int xend)
        {
            if (workingPoints.Count == 2)
            {
                ystart = workingPoints[0].Y;
                yend = workingPoints[1].Y;

                if (ystart > yend)
                {
                    int hold = ystart;
                    ystart = yend;
                    yend = hold;
                }

                xstart = workingPoints[0].X;
                xend = workingPoints[1].X;

                if (xstart > xend)
                {
                    int hold = xstart;
                    xstart = xend;
                    xend = hold;
                }

                if (xstart < 0) xstart = 0;
                if (xstart >= dvWindow.Width) xstart = dvWindow.Width - 1;
                if (xend < 0) xend = 0;
                if (xend >= dvWindow.Width) xend = dvWindow.Width - 1;
                if (ystart < 0) ystart = 0;
                if (ystart >= dvWindow.Height) ystart = dvWindow.Height - 1;
                if (yend < 0) yend = 0;
                if (yend >= dvWindow.Height) yend = dvWindow.Height - 1;
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Flip the screen Horixontally
        /// </summary>
        /// --------------------------------------------------------------------------
        void FlipHorizontal()
        {
            int spot1 = 0;
            int spot2 = (dvWindow.MainBuffer.Height - 1) * dvWindow.MainBuffer.BufferPitch;
            ushort temp;
            ushort[] buffer = dvWindow.MainBuffer.RawBuffer;

            int ystart = 0;
            int yend = dvWindow.MainBuffer.Height - 1;
            int xstart = 0;
            int xend = dvWindow.MainBuffer.Width - 1;

            GetSortedLimits(ref ystart, ref yend, ref xstart, ref xend);

            int xwidth = (xend - xstart) / 2;

            for (int y = ystart; y <= yend; y++)
            {
                spot1 = (y) * dvWindow.MainBuffer.BufferPitch + xstart;
                spot2 = (y) * dvWindow.MainBuffer.BufferPitch + xend;
                for (int i = 0; i < xwidth; i++)
                {
                    int x = xstart + i;
                    temp = buffer[spot1];
                    buffer[spot1] = buffer[spot2];
                    buffer[spot2] = temp;
                    spot1++;
                    spot2--;
                }
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Render one frame's worth of spraypaint
        /// </summary>
        /// --------------------------------------------------------------------------
        private void SprayPaint(int centerx, int centery, int radius, int dotCount, ushort color)
        {

            for (int i = 0; i < dotCount; i++)
            {
                Utilities.AnimateColor(ref color, (uint)(paintingFrame + i)); 

                double theta = (Utilities.Rand(10000) - 5000) / 5000.0 * Math.PI;
                int r = Utilities.Rand(radius * 2) + Utilities.Rand(radius * 2);
                r = (r / 2) - radius;

                int x = (int)(centerx + r * Math.Cos(theta));
                int y = (int)(centery + r * Math.Sin(theta));

                dvWindow.MainBuffer.DrawPixel(color, x, y);
            }

        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Convert animated colors to non-animated equivalents
        /// </summary>
        /// --------------------------------------------------------------------------
        private void FlattenAnimatedColors()
        {
            ushort[] buffer = dvWindow.MainBuffer.RawBuffer;

            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = dvWindow.MainBuffer.GetPaletteColor(GlobalState.Palette[buffer[i]]);
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Launch fireworks with throttling
        /// </summary>
        /// --------------------------------------------------------------------------
        private FireworkType LaunchFirework(KeyEventArgs keyArgs, int h, int rx, ushort shortRc, FireworkType mode)
        {
            TimeSpan span = DateTime.Now - lastFireworkTime;
            if (span.TotalMilliseconds > 100)
            {
                if (modulator != 0) mode = (FireworkType)(modulator % (int)FireworkType.MaxCount);
                AddAnimation(new Animation.Firework(dvWindow, mode, rx, h, shortRc));
                lastFireworkTime = DateTime.Now;
            }
            return mode;
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Trigger the showing of the password hint
        /// </summary>
        /// --------------------------------------------------------------------------
        public void SignalPasswordHint()
        {
            passWordHint = true;
        }

        List<Point> workingPoints = new List<Point>();
        /// --------------------------------------------------------------------------
        /// <summary>
        /// Working points clue various functions to their geometry
        /// </summary>
        /// --------------------------------------------------------------------------
        public void SetWorkingPoint()
        {
            Point p = new Point(mouseX, mouseY);

            switch (workingPoints.Count)
            {
                case 0:
                    MediaBag.Play(SoundID.Dot18,1,.3);
                    AddAnimation(new Animation.WorkingPoint(dvWindow, mouseX, mouseY, Color.Red));
                    workingPoints.Add(p);
                    break;
                case 1:
                    if (mouseX != workingPoints[0].X || mouseY != workingPoints[0].Y)
                    {
                        MediaBag.Play(SoundID.Dot17, 1, .3);
                        AddAnimation(new Animation.WorkingPoint(dvWindow, mouseX, mouseY, Color.Red));
                        workingPoints.Add(p);
                    }
                    break;
                case 2:
                    MediaBag.Play(SoundID.Slide_Screech, 1, .3);
                    ClearWorkingPoints();
                    break;
            }

        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Clealr any working points
        /// </summary>
        /// --------------------------------------------------------------------------
        private void ClearWorkingPoints()
        {
            StopAnimations(typeof(Animation.WorkingPoint));
            workingPoints.Clear();
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Convert the log to xml so that we can export it
        /// </summary>
        /// --------------------------------------------------------------------------
        public string SerializeLog()
        {
            StringBuilder outputXml = new StringBuilder();
            outputXml.Append("<PixelWhimsyData>");
            outputXml.Append("<TotalFrames>" + (frame - stressModeFrames) + "</TotalFrames>");

            outputXml.Append("<KeyPresses>");
            foreach (Keys key in keypressLog.Keys)
            {
                outputXml.Append("<Key><Type>" + key + "</Type><Value>" + keypressLog[key] + "</Value></Key>");
            }
            outputXml.Append("</KeyPresses>");

            outputXml.Append("<Animations>");
            foreach (Type animationType in animationLog.Keys)
            {
                outputXml.Append("<Animation><Type>" + animationType.ToString().Replace("PixelWhimsy.Animation+","") + "</Type><Value>" + animationLog[animationType] + "</Value></Animation>");
            }
            outputXml.Append("</Animations>");

            outputXml.Append("</PixelWhimsyData>");
            return outputXml.ToString();
        }

        #endregion

        #region delayed actions

        /// --------------------------------------------------------------------
        /// <summary>
        /// Class to support delayed actions
        /// </summary>
        /// --------------------------------------------------------------------
        public class DelayedAction
        {
            DateTime when;
            DelayedActionFunc doit;

            public DelayedAction(DateTime when, DelayedActionFunc method)
            {
                this.when = when;
                this.doit = method;
            }

            public bool ReadyToGo { get { return DateTime.Now > when; } }
            public void Execute(){doit();}
        }

        /// --------------------------------------------------------------------
        /// <summary>
        /// Remove any delayed actions
        /// </summary>
        /// --------------------------------------------------------------------
        public void StopDelayedActions()
        {
            lock (delayedActions)
            {
                delayedActions.Clear();
            }
        }

        /// --------------------------------------------------------------------
        /// <summary>
        /// Add a delayed action
        /// </summary>
        /// --------------------------------------------------------------------
        void AddDelayedAction(DelayedAction action)
        {
            lock (delayedActions)
            {
                delayedActions.Add(action);
            }
        }

        /// --------------------------------------------------------------------
        /// <summary>
        /// Execute any delayed actions that should run now
        /// </summary>
        /// --------------------------------------------------------------------
        void ExecuteDelayedActions()
        {
            lock (delayedActions)
            {
                for (int i = 0; i < delayedActions.Count; )
                {
                    if (delayedActions[i].ReadyToGo)
                    {
                        delayedActions[i].Execute();
                        delayedActions.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }
            }
        }
        #endregion
 
        #region Toolbars
        /// --------------------------------------------------------------------------
        /// <summary>
        /// Draw the animations toolbar
        /// </summary>
        /// --------------------------------------------------------------------------
        void RenderAnimationToolbar()
        {
            if (MediaBag.iconPics == null || dvWindowAnimations == null) return;

            dvWindowAnimations.MainBuffer.Clear(0);
            Dictionary<Type, string> activeAnimations = new Dictionary<Type, string>();

            lock (animationQueueLockHandle)
            {
                foreach (Animation animation in animationQueue)
                {
                    activeAnimations[animation.GetType()] = null;
                }
            }

            Type[] animationTypes = new Type[]
            {   
                typeof(Animation.ScreenDecay),
                typeof(Animation.GameOfLife),
                typeof(Animation.Fader),
                typeof(Animation.PixelDiffuser),
                typeof(Animation.ColorDiffuser),
                typeof(Animation.ScreenFlow),
                typeof(Animation.Rain),
                typeof(Animation.ScreenFlowSimple),
                typeof(Animation.GroundCollapse),
                typeof(Animation.Snow),
            };

            for (int i = 0; i < animationTypes.Length; i++)
            {
                int iconID = activeAnimations.ContainsKey(animationTypes[i]) ? i+3 : i + 53;
                dvWindowAnimations.MainBuffer.DrawSprite(MediaBag.iconPics, iconID, i * 40, 0);
            }
            this.panelAnimations.Invalidate();
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Draw the Modulators
        /// </summary>
        /// --------------------------------------------------------------------------
        void RenderModulatorToolbar()
        {
            
            for (int i = 0; i < 10; i++)
            {
                int y = i * 16;
                Color color = (i == modulator )? Color.Blue : Color.Black;
                dvWindowModulators.MainBuffer.DrawFilledRectangle(color, 0, y, 15, y + 15);
                dvWindowModulators.MainBuffer.DrawRectangle(Color.White, 0, y, 15, y + 15);
                string text = i.ToString();
                if (i == 0) text = "N";
                dvWindowModulators.MainBuffer.Print(MediaBag.color_White, MediaBag.font_Status, 3, y + 1, text);
            }



            this.panelModulators.Invalidate();

        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Draw the brushes
        /// </summary>
        /// --------------------------------------------------------------------------
        void RenderBrushToolbar()
        {
            if (MediaBag.iconPics == null || dvWindowBrushes == null) return;
            dvWindowBrushes.MainBuffer.Clear(0);

            // Kaleidoscope
            int iconID = GlobalState.PaintingStyle == PaintingStyle.Kaleidoscope ? 1 : 51;
            dvWindowBrushes.MainBuffer.DrawSprite(MediaBag.iconPics, iconID, 40, 0);

            // Tile
            iconID = GlobalState.PaintingStyle == PaintingStyle.Tile ? 2 : 52;
            dvWindowBrushes.MainBuffer.DrawSprite(MediaBag.iconPics, iconID, 40, 40);

            // Eraser
            iconID = 30;
            dvWindowBrushes.MainBuffer.DrawSprite(MediaBag.iconPics, iconID, 0, 0);
            
            // Freeze animations
            iconID = freezeAnimations ? 31 : 81;
            dvWindowBrushes.MainBuffer.DrawSprite(MediaBag.iconPics, iconID, 0, 40);

            // Text ENtry
            iconID = screenMode == ScreenMode.TextEntry ? 32 : 82;
            dvWindowBrushes.MainBuffer.DrawSprite(MediaBag.iconPics, iconID, 0, 80);

            //[.] Help
            iconID = AnimationExists(typeof(Animation.Instructions)) ? 33 : 83;
            dvWindowBrushes.MainBuffer.DrawSprite(MediaBag.iconPics, iconID, 0, 120);

            //[.] Load
            iconID = screenMode == ScreenMode.LoadScreen ? 34 : 84;
            dvWindowBrushes.MainBuffer.DrawSprite(MediaBag.iconPics, iconID, 0, 160);

            //[.] Save
            iconID = screenMode == ScreenMode.SaveScreen ? 35 : 85;
            dvWindowBrushes.MainBuffer.DrawSprite(MediaBag.iconPics, iconID, 0, 200);

            //[.] colorPicker
            iconID = colorPicker ? 36 : 86;
            dvWindowBrushes.MainBuffer.DrawSprite(MediaBag.iconPics, iconID, 0, 240);

            //[.] Bees
            iconID = AnimationExists(typeof(Animation.Bee)) ? 37 : 87;
            dvWindowBrushes.MainBuffer.DrawSprite(MediaBag.iconPics, iconID, 0, 280);

            //[.] Autobrushes
            iconID = AnimationExists(typeof(Animation.AutoBrush)) ? 38 : 88;
            dvWindowBrushes.MainBuffer.DrawSprite(MediaBag.iconPics, iconID, 0, 320);

            //[.] Autobrushes
            iconID = GlobalState.RandomBrush ? 39 : 89;
            dvWindowBrushes.MainBuffer.DrawSprite(MediaBag.iconPics, iconID, 0, 360);


            BrushType[] brushTypes = new BrushType[]
            {   
                BrushType.Circle,
                BrushType.Shader,
                BrushType.Windmill,
                BrushType.SprayPaint,
                BrushType.LifePattern,
                BrushType.PictureStamp,
                BrushType.LineTarget,
                BrushType.FloodFill,
                BrushType.Dragging
            };


            for (int i = 0; i < brushTypes.Length; i++)
            {
                iconID = GlobalState.BrushType == brushTypes[i] ? i + 20 : i + 70;
                dvWindowBrushes.MainBuffer.DrawSprite(MediaBag.iconPics, iconID, 40, i * 40 + 80);
            }

            // Winding indicator
            MediaBag.SetWingdingFont(18);
            dvWindowBrushes.MainBuffer.Print(MediaBag.color_White,
                MediaBag.font_Wingding,
                -2, dvWindowBrushes.Height - 35,
                new string(GetWinddingChar(), 1));

            this.panelBrushes.Invalidate();
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Handle painting event for brushes
        /// </summary>
        /// --------------------------------------------------------------------------
        private void panelBrushes_Paint(object sender, PaintEventArgs e)
        {
            if(dvWindowBrushes != null) dvWindowBrushes.Render();
        }

        ushort[] scratchBuffer;

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Find edges graphics algorithm
        /// </summary>
        /// --------------------------------------------------------------------------
        void FindEdges()
        {
            int spotUp, spotLeft, spotRight, spotCenter, spotDown;
            ushort[] data = dvWindow.MainBuffer.RawBuffer;
            if (scratchBuffer == null) scratchBuffer = new ushort[data.Length];

            for (int y = 1; y < dvWindow.MainBuffer.Height - 1; y++)
            {
                spotUp = dvWindow.MainBuffer.BufferPitch * (y - 1) + 1;
                spotLeft = dvWindow.MainBuffer.BufferPitch * y;
                spotCenter = dvWindow.MainBuffer.BufferPitch * y + 1;
                spotRight = dvWindow.MainBuffer.BufferPitch * y + 2;
                spotDown = dvWindow.MainBuffer.BufferPitch * (y + 1) + 1;
                for (int x = 1; x < dvWindow.MainBuffer.Width - 1; x++)
                {
                    int r = 0, g = 0, b = 0;
                    r -= GlobalState.rgbLookup5bit[data[spotUp], 0];
                    g -= GlobalState.rgbLookup5bit[data[spotUp], 1];
                    b -= GlobalState.rgbLookup5bit[data[spotUp], 2];
                    r -= GlobalState.rgbLookup5bit[data[spotLeft], 0];
                    g -= GlobalState.rgbLookup5bit[data[spotLeft], 1];
                    b -= GlobalState.rgbLookup5bit[data[spotLeft], 2];
                    r -= GlobalState.rgbLookup5bit[data[spotRight], 0];
                    g -= GlobalState.rgbLookup5bit[data[spotRight], 1];
                    b -= GlobalState.rgbLookup5bit[data[spotRight], 2];
                    r -= GlobalState.rgbLookup5bit[data[spotDown], 0];
                    g -= GlobalState.rgbLookup5bit[data[spotDown], 1];
                    b -= GlobalState.rgbLookup5bit[data[spotDown], 2];
                    r += GlobalState.rgbLookup5bit[data[spotCenter], 0] * 4;
                    g += GlobalState.rgbLookup5bit[data[spotCenter], 1] * 4;
                    b += GlobalState.rgbLookup5bit[data[spotCenter], 2] * 4;

                    if (r < 0) r = 0;
                    if (g < 0) g = 0;
                    if (b < 0) b = 0;
                    if (r > 31) r = 31;
                    if (g > 31) g = 31;
                    if (b > 31) b = 31;

                    ushort newColor = (ushort)((r << 10) | (g << 5) | b);
                    scratchBuffer[spotCenter] = newColor;

                    spotLeft++;
                    spotUp++;
                    spotRight++;
                    spotDown++;
                    spotCenter++;
                }
            }

            scratchBuffer.CopyTo(data, 0);
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Emboss graphics algorithm
        /// </summary>
        /// --------------------------------------------------------------------------
        void Emboss()
        {
            int spotUp, spotLeft, spotUpLeft, spotRight, spotRightDown, spotDown;
            ushort[] data = dvWindow.MainBuffer.RawBuffer;
            if (scratchBuffer == null) scratchBuffer = new ushort[data.Length];

            for (int y = 1; y < dvWindow.MainBuffer.Height - 1; y++)
            {
                spotUp = dvWindow.MainBuffer.BufferPitch * (y - 1) + 1;
                spotUpLeft = dvWindow.MainBuffer.BufferPitch * (y - 1);
                spotLeft = dvWindow.MainBuffer.BufferPitch * y;
                spotRight = dvWindow.MainBuffer.BufferPitch * y + 2;
                spotDown = dvWindow.MainBuffer.BufferPitch * (y + 1) + 1;
                spotRightDown = dvWindow.MainBuffer.BufferPitch * (y + 1) + 2;
                for (int x = 1; x < dvWindow.MainBuffer.Width - 1; x++)
                {
                    int r = 15, g = 15, b = 15;
                    r += GlobalState.rgbLookup5bit[data[spotUp], 0];
                    g += GlobalState.rgbLookup5bit[data[spotUp], 1];
                    b += GlobalState.rgbLookup5bit[data[spotUp], 2];
                    r += GlobalState.rgbLookup5bit[data[spotLeft], 0];
                    g += GlobalState.rgbLookup5bit[data[spotLeft], 1];
                    b += GlobalState.rgbLookup5bit[data[spotLeft], 2];
                    r += GlobalState.rgbLookup5bit[data[spotUpLeft], 0];
                    g += GlobalState.rgbLookup5bit[data[spotUpLeft], 1];
                    b += GlobalState.rgbLookup5bit[data[spotUpLeft], 2];

                    r -= GlobalState.rgbLookup5bit[data[spotRight], 0];
                    g -= GlobalState.rgbLookup5bit[data[spotRight], 1];
                    b -= GlobalState.rgbLookup5bit[data[spotRight], 2];
                    r -= GlobalState.rgbLookup5bit[data[spotDown], 0];
                    g -= GlobalState.rgbLookup5bit[data[spotDown], 1];
                    b -= GlobalState.rgbLookup5bit[data[spotDown], 2];
                    r -= GlobalState.rgbLookup5bit[data[spotRightDown], 0];
                    g -= GlobalState.rgbLookup5bit[data[spotRightDown], 1];
                    b -= GlobalState.rgbLookup5bit[data[spotRightDown], 2];

                    if (r < 0) r = 0;
                    if (g < 0) g = 0;
                    if (b < 0) b = 0;
                    if (r > 31) r = 31;
                    if (g > 31) g = 31;
                    if (b > 31) b = 31;

                    ushort newColor = (ushort)((r << 10) | (g << 5) | b);
                    scratchBuffer[spotLeft + 1] = newColor;

                    spotLeft++;
                    spotUp++;
                    spotUpLeft++;
                    spotRight++;
                    spotDown++;
                    spotRightDown++;
                }
            }

            scratchBuffer.CopyTo(data, 0);
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Emboss graphics algorithm
        /// </summary>
        /// --------------------------------------------------------------------------
        void ChannelSwap()
        {
            ushort[] data = dvWindow.MainBuffer.RawBuffer;

            for (int spot = 0; spot < data.Length; spot++)
            {
                int r, g, b;
                r = GlobalState.rgbLookup5bit[data[spot], 1];
                g = GlobalState.rgbLookup5bit[data[spot], 2];
                b = GlobalState.rgbLookup5bit[data[spot], 0];

                ushort newColor = (ushort)((r << 10) | (g << 5) | b);
                data[spot] = newColor;

            }
        }


        /// --------------------------------------------------------------------------
        /// <summary>
        /// Handle painting event for animations
        /// </summary>
        /// --------------------------------------------------------------------------
        private void panelAnimations_Paint(object sender, PaintEventArgs e)
        {
            if (dvWindowAnimations != null) dvWindowAnimations.Render();
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Handle painting event for modulators
        /// </summary>
        /// --------------------------------------------------------------------------
        private void panel_Modulators_Paint(object sender, PaintEventArgs e)
        {
            if (dvWindowModulators != null) dvWindowModulators.Render();
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Handle clicks on the panelAnimations
        /// </summary>
        /// --------------------------------------------------------------------------
        private void panelAnimations_Click(object sender, EventArgs e)
        {
            int id = (absoluteMouseX - this.panelAnimations.Left) / 40;
            switch (id)
            {
                case 0: GenerateKeyPress(KeyAction.AnimateScreenDecay); break;
                case 1: GenerateKeyPress(KeyAction.AnimateGameOfLife); break;
                case 2: GenerateKeyPress(KeyAction.AnimateFader); break;
                case 3: GenerateKeyPress(KeyAction.AnimatePixelDiffuser); break;
                case 4: GenerateKeyPress(KeyAction.AnimateColorDiffuser); break;
                case 5: GenerateKeyPress(KeyAction.AnimateScreenFlow); break;
                case 6: GenerateKeyPress(KeyAction.AnimateRain); break;
                case 7: GenerateKeyPress(KeyAction.AnimateSimpleScreenFlow); break;
                case 8: GenerateKeyPress(KeyAction.AnimateGroundCollapse); break;
                case 9: GenerateKeyPress(KeyAction.AnimateSnow); break;
            }
        }


        /// --------------------------------------------------------------------------
        /// <summary>
        /// Handle clicks on the panelbrushes
        /// </summary>
        /// --------------------------------------------------------------------------
        private void panelBrushes_Click(object sender, EventArgs e)
        {
            int id = (absoluteMouseY - this.panelBrushes.Top) / 40 + ((absoluteMouseX - this.panelBrushes.Left)/40) * 11;
            switch (id)
            {
                case 0: GenerateKeyPress(KeyAction.ClearScreen); break;
                case 1: GenerateKeyPress(KeyAction.FreezeAnimations); break;
                case 2: GenerateKeyPress(KeyAction.TextEditor); break;
                case 3: 
                    if(!AnimationExists(typeof(Animation.Instructions)))
                    {
                        GenerateKeyPress(KeyAction.Help);
                        needToStopInstructions = false;
                    }
                    break;
                case 4: GenerateKeyPress(KeyAction.ScreenLoad); break;
                case 5: GenerateKeyPress(KeyAction.ScreenStore); break;
                case 6: GenerateKeyPress(KeyAction.ColorPicker); break;
                case 7: 
                    if(AnimationExists(typeof(Animation.Bee)))
                    {
                        StopAnimations(typeof(Animation.Bee));
                    }
                    else
                    {
                        for(int i = 0; i < 50; i++)
                        {
                            int rx = Utilities.Rand(dvWindow.Width);
                            int ry = Utilities.Rand(dvWindow.Height);
                            Animation.Bee newBee = new Animation.Bee(dvWindow, rx, ry, GlobalState.CurrentDrawingColor);
                            newBee.MouseX = rx;
                            newBee.MouseY = ry;
                            AddAnimation(newBee);
                        }
                    }
                    break;
                case 8: 
                    if(AnimationExists(typeof(Animation.AutoBrush)))
                    {
                        StopAnimations(typeof(Animation.AutoBrush));
                    }
                    else
                    {
                        MediaBag.Play(SoundID.Dot_Dinglow);
                        for (int i = 0; i < 5; i++)
                        {
                            int rx = Utilities.Rand(dvWindow.Width);
                            int ry = Utilities.Rand(dvWindow.Height);
                            int size = GlobalState.BrushSize;
                            double xm = Utilities.DRand(2) - 1;
                            double ym = Utilities.DRand(2) - 1;
                            Animation.AutoBrush newBrush = new Animation.AutoBrush(dvWindow, rx, ry, size, xm, ym, GlobalState.CurrentDrawingColor);
                            newBrush.MouseX = rx;
                            newBrush.MouseY = ry;
                            AddAnimation(newBrush);
                        }
                    }
                    break;
                case 9: GenerateKeyPress(KeyAction.BrushSelectRandom); break;
                case 11: GenerateKeyPress(KeyAction.KaleidoPaint); break;
                case 12: GenerateKeyPress(KeyAction.TilePaint); break;
                case 13: GenerateKeyPress(KeyAction.BrushSelectCircle); break;
                case 14: GenerateKeyPress(KeyAction.BrushSelectShader); break;
                case 15: GenerateKeyPress(KeyAction.BrushSelectSquare); break;
                case 16: GenerateKeyPress(KeyAction.BrushSelectSprayPaint); break;
                case 17: GenerateKeyPress(KeyAction.BrushSelectLifePattern); break;
                case 18: GenerateKeyPress(KeyAction.BrushSelectPictureStamp); break;
                case 19: GenerateKeyPress(KeyAction.BrushSelectLineTarget); break;
                case 20: GenerateKeyPress(KeyAction.BrushSelectFloodFill); break;
                case 21:
                    GlobalState.BrushType = BrushType.Dragging;
                    ClearWorkingPoints();
                    break;
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Handle clicks on the panelModulators
        /// </summary>
        /// --------------------------------------------------------------------------
        private void panelModulators_Click(object sender, EventArgs e)
        {
            int id = (absoluteMouseY - this.panelModulators.Top) / 16;
            modulator = id;
            RenderModulatorToolbar();
        }

        private void toolbar_MouseEnter(object sender, EventArgs e)
        {
            //insideToolbar = true;
        }

        private void toolbar_MouseLeave(object sender, EventArgs e)
        {
            //insideToolbar = false;
        }

        #endregion

        private void Slate_Paint(object sender, PaintEventArgs e)
        {
            panelAnimations.Invalidate();
            panelBrushes.Invalidate();
            RenderAnimationToolbar();
            RenderBrushToolbar();

            Graphics g = Graphics.FromHwnd(this.Handle);
            Image image = Bitmap.FromStream(DVTools.GetStream("PixelWhimsy.bitmaps.main-page-logo.png"));
            g.DrawImageUnscaled(image, 0, 0);
        }

        private void Slate_SizeChanged(object sender, EventArgs e)
        {
        }

        private void Slate_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        private void Slate_VisibleChanged(object sender, EventArgs e)
        {
        }



        /// --------------------------------------------------------------------------
        /// <summary>
        /// Create a text summary of the animations
        /// </summary>
        /// --------------------------------------------------------------------------
        internal string AnimationSummary()
        {
            Dictionary<string, int> summary = new Dictionary<string, int>();

            lock (animationQueueLockHandle)
            {
                foreach (Animation a in animationQueue)
                {
                    string name = a.GetType().ToString();
                    if (!summary.ContainsKey(name)) summary[name] = 0;

                    summary[name]++;
                }
            }

            StringBuilder summaryText = new StringBuilder();
            foreach (string name in summary.Keys)
            {
                summaryText.Append(name + " (" + summary[name] + ")" + Environment.NewLine); 
            }

            return summaryText.ToString();
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Exit buttons
        /// </summary>
        /// --------------------------------------------------------------------------
        private void buttonExit_Click(object sender, EventArgs e)
        {
            if (Settings.KidSafe) passWordHint = true;
            else if(mouseY < 0 && !stressMode) GlobalState.EndApplication = true;
        }

        private void buttonExit_MouseEnter(object sender, EventArgs e)
        {
            insideExitButton = true;
        }

        // UNDO CODE
        // Probably don't want to do this.  Best case compression is 6 ms on a medium screem.  
        //const uint MaxUndoMemory = 50000000;

        //uint[] scratchCompress = null;

        //HiPerfTimer hpt = new HiPerfTimer();
        ///// -------------------------------------------------------
        ///// <summary>
        ///// Save the buffer to a list of remembered buffers
        ///// </summary>
        ///// -------------------------------------------------------
        //void SaveBuffer(PixelBuffer buffer)
        //{
        //    if (scratchCompress == null)
        //    {
        //        scratchCompress = new uint[dvWindow.Width * dvWindow.Height];
        //    }

        //    hpt.Start();
        //    ushort[] data = buffer.RawBuffer;

        //    int scratchSpot = 0;
        //    uint pixelCount = 1;
        //    ushort lastColor = data[0];
        //    for (int i = 1; i < data.Length; i++)
        //    {
        //        if (data[i] != lastColor || pixelCount == 0xffff)
        //        {
        //            scratchCompress[scratchSpot++] = (uint)((pixelCount << 16) + lastColor);
        //            lastColor = data[i];
        //            pixelCount = 1;
        //        }
        //        else pixelCount++;
        //    }

        //    scratchCompress[scratchSpot++] = (uint)((pixelCount << 16) + lastColor);

        //    uint[] newData = new uint[scratchSpot];
        //    double foo = hpt.ElapsedSeconds;
        //}

    }
}