using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using DirectVarmint;
using System.IO;
using System.Drawing.Imaging;
using System.Threading;
using System.Collections;
using System.Diagnostics;

namespace PixelWhimsy
{
    public enum KeyAction
    {
        Unknown,
        AnimateColorDiffuser,
        AnimateGameOfLife,
        AnimateGravityRainbow,
        AnimateFader,
        AnimateGroundCollapse,
        AnimatePixelDiffuser,
        AnimateRain,
        AnimateSimpleScreenFlow,
        AnimateScreenDecay,
        AnimateScreenFlow,
        AnimateSnow,
        AutoBrush,
        Bee,
        BrushSelectBomb,
        BrushSelectCircle,
        BrushSelectFloodFill,
        BrushSelectLifePattern,
        BrushSelectLineTarget,
        BrushSelectPictureStamp,
        BrushSelectRandom,
        BrushSelectSquare,
        BrushSelectSprayPaint,
        BrushSelectShader,
        ChannelSwap,
        ClearScreen,
        ColorCounter,
        ColorPicker,
        DrawArgyleDot,
        DrawBomb,
        DrawCheckerBoard,
        DrawCircle,
        DrawFilledCircle,
        DrawFilledCircleActive,
        DrawFilledCircleXor,
        DrawFilledRectangle,
        DrawFilledRectangleActive,
        DrawFilledRectangleXor,
        DrawFirework1,
        DrawFirework2,
        DrawFirework3,
        DrawGradient,
        DrawLifePattern,
        DrawLine,
        DrawLogo,
        DrawMaze,
        DrawMiniPicture,
        DrawMoirePattern,
        DrawPlasma,
        DrawPolkaDots,
        DrawRectangle,
        DrawSpirograph,
        DrawSpikes,
        DrawTree,
        DrawWingding,
        DropCheese,
        Emboss,
        FindEdges,
        FlattenAnimatedColors,
        FlipHorizontal,
        FlipVerticle,
        FunKey,
        FreezeAnimations,
        Help,
        KaleidoPaint,
        Modulate,
        NoAction,
        Note00,
        Note01,
        Note02,
        Note03,
        Note04,
        Note05,
        Note06,
        Note07,
        Note08,
        Note09,
        Note10,
        Note11,
        Note12,
        PasswordHint,
        RandomBrushSize,
        RandomColor,
        RandomColorAnimated,
        ScreenLoad,
        ScreenStore,
        ScreenXor,
        SelectNoteSound,
        ShowVersion,
        SprayPaint,
        StopEveryThing,
        StressMode,
        TextEditor,
        TilePaint,
        VolumeMute,
        VolumeUp,
        VolumeDown,
        WorkingPoints,
        NumberOfKeyActions,
    }

    /// --------------------------------------------------------------------------
    /// <summary>
    /// Keyboard code for the drawing slate
    /// </summary>
    /// --------------------------------------------------------------------------
    public partial class Slate
    {
        Dictionary<Keys, uint> keyPressData = new Dictionary<Keys, uint>();
        string recentlyPressed = "";
        DateTime lastFireworkTime = DateTime.Now;
        DateTime lastTreeTime = DateTime.Now;
        Dictionary<Keys, KeyAction> actionTranslations = new Dictionary<Keys, KeyAction>();
        Dictionary<Keys, FunKeyFunc> funKeyTranslations = new Dictionary<Keys, FunKeyFunc>();

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Assign actions to keystrokes
        /// </summary>
        /// --------------------------------------------------------------------------
        public void AssignKeyActions()
        {

            actionTranslations.Add(Keys.A, KeyAction.DrawFilledCircle);
            actionTranslations.Add(Keys.Add, KeyAction.Note12);
            actionTranslations.Add(Keys.Apps, KeyAction.DrawMiniPicture);
            actionTranslations.Add(Keys.B, KeyAction.Bee);
            actionTranslations.Add(Keys.Back, KeyAction.FlattenAnimatedColors);
            actionTranslations.Add(Keys.C, KeyAction.ColorPicker);
            actionTranslations.Add(Keys.CapsLock, KeyAction.TilePaint);
            actionTranslations.Add(Keys.D, KeyAction.SprayPaint);
            actionTranslations.Add(Keys.D0, KeyAction.AnimateSnow);
            actionTranslations.Add(Keys.D1, KeyAction.AnimateScreenDecay);
            actionTranslations.Add(Keys.D2, KeyAction.AnimateGameOfLife);
            actionTranslations.Add(Keys.D3, KeyAction.AnimateFader);
            actionTranslations.Add(Keys.D4, KeyAction.AnimatePixelDiffuser);
            actionTranslations.Add(Keys.D5, KeyAction.AnimateColorDiffuser);
            actionTranslations.Add(Keys.D6, KeyAction.AnimateScreenFlow);
            actionTranslations.Add(Keys.D7, KeyAction.AnimateRain);
            actionTranslations.Add(Keys.D8, KeyAction.AnimateSimpleScreenFlow);
            actionTranslations.Add(Keys.D9, KeyAction.AnimateGroundCollapse);
            //actionTranslations.Add(Keys.Decimal, ); //(Keypad)
            actionTranslations.Add(Keys.Delete, KeyAction.ScreenXor);
            actionTranslations.Add(Keys.Divide, KeyAction.SelectNoteSound);
            actionTranslations.Add(Keys.Down, KeyAction.NoAction);
            actionTranslations.Add(Keys.E, KeyAction.DrawSpikes);
            actionTranslations.Add(Keys.End, KeyAction.DrawCircle);
            actionTranslations.Add(Keys.Escape, KeyAction.StopEveryThing);
            actionTranslations.Add(Keys.F, KeyAction.DrawLogo);
            actionTranslations.Add(Keys.F1, KeyAction.Help);
            actionTranslations.Add(Keys.F2, KeyAction.FunKey);
            actionTranslations.Add(Keys.F3, KeyAction.FunKey );
            actionTranslations.Add(Keys.F4, KeyAction.FunKey );
            actionTranslations.Add(Keys.F5, KeyAction.FunKey );
            actionTranslations.Add(Keys.F6, KeyAction.FunKey );
            actionTranslations.Add(Keys.F7, KeyAction.FunKey );
            actionTranslations.Add(Keys.F8, KeyAction.FunKey );
            actionTranslations.Add(Keys.F9, KeyAction.FunKey );
            actionTranslations.Add(Keys.F10, KeyAction.FunKey );
            actionTranslations.Add(Keys.F11, KeyAction.FunKey );
            actionTranslations.Add(Keys.F12, KeyAction.FunKey);
            actionTranslations.Add(Keys.G, KeyAction.DrawFilledCircleActive);
            actionTranslations.Add(Keys.H, KeyAction.AutoBrush);
            actionTranslations.Add(Keys.Home, KeyAction.BrushSelectRandom);
            actionTranslations.Add(Keys.I, KeyAction.Emboss);
            actionTranslations.Add(Keys.Insert, KeyAction.ScreenLoad);
            actionTranslations.Add(Keys.J, KeyAction.RandomBrushSize);
            actionTranslations.Add(Keys.K, KeyAction.DrawPlasma);
            actionTranslations.Add(Keys.L, KeyAction.DrawLifePattern);
            actionTranslations.Add(Keys.LControlKey, KeyAction.NoAction);
            actionTranslations.Add(Keys.LMenu, KeyAction.NoAction);
            actionTranslations.Add(Keys.LShiftKey, KeyAction.NoAction);
            actionTranslations.Add(Keys.LWin, KeyAction.DrawFilledCircleXor);
            actionTranslations.Add(Keys.Left, KeyAction.RandomColor);
            actionTranslations.Add(Keys.M, KeyAction.DrawMoirePattern);
            actionTranslations.Add(Keys.Multiply, KeyAction.Note10);
            actionTranslations.Add(Keys.N, KeyAction.DrawSpirograph);
            actionTranslations.Add(Keys.None, KeyAction.NoAction);
            actionTranslations.Add(Keys.NumLock, KeyAction.ColorCounter);
            actionTranslations.Add(Keys.NumPad0, KeyAction.Note00);
            actionTranslations.Add(Keys.NumPad1, KeyAction.Note01);
            actionTranslations.Add(Keys.NumPad2, KeyAction.Note02);
            actionTranslations.Add(Keys.NumPad3, KeyAction.Note03);
            actionTranslations.Add(Keys.NumPad4, KeyAction.Note04);
            actionTranslations.Add(Keys.NumPad5, KeyAction.Note05);
            actionTranslations.Add(Keys.NumPad6, KeyAction.Note06);
            actionTranslations.Add(Keys.NumPad7, KeyAction.Note07);
            actionTranslations.Add(Keys.NumPad8, KeyAction.Note08);
            actionTranslations.Add(Keys.NumPad9, KeyAction.Note09);
            actionTranslations.Add(Keys.O, KeyAction.FindEdges);
            actionTranslations.Add(Keys.Oem1, KeyAction.Modulate);                          // ; Key
            actionTranslations.Add(Keys.Oem5, KeyAction.DrawLine);                          // \ Key
            actionTranslations.Add(Keys.Oem6, KeyAction.DrawFilledRectangleActive);         // ] Key
            actionTranslations.Add(Keys.Oem7, KeyAction.BrushSelectBomb);                   // ' Key
            actionTranslations.Add(Keys.OemMinus, KeyAction.DrawFilledRectangleXor);
            actionTranslations.Add(Keys.OemOpenBrackets, KeyAction.DrawRectangle);
            actionTranslations.Add(Keys.OemPeriod, KeyAction.FreezeAnimations);
            actionTranslations.Add(Keys.OemQuestion, KeyAction.AnimateGravityRainbow);
            actionTranslations.Add(Keys.Oemcomma, KeyAction.DrawFirework3);   
            actionTranslations.Add(Keys.Oemplus, KeyAction.DrawTree);
            actionTranslations.Add(Keys.Oemtilde, KeyAction.ClearScreen);
            actionTranslations.Add(Keys.P, KeyAction.DrawGradient);
            actionTranslations.Add(Keys.PageDown, KeyAction.FlipVerticle);
            actionTranslations.Add(Keys.PageUp, KeyAction.FlipHorizontal);
            //actionTranslations.Add(Keys.Pause, );
            actionTranslations.Add(Keys.PrintScreen, KeyAction.ScreenStore);
            actionTranslations.Add(Keys.Q, KeyAction.PasswordHint);
            actionTranslations.Add(Keys.R, KeyAction.DrawPolkaDots);
            actionTranslations.Add(Keys.RControlKey, KeyAction.NoAction);
            actionTranslations.Add(Keys.RMenu, KeyAction.NoAction);
            actionTranslations.Add(Keys.RShiftKey, KeyAction.NoAction);
            actionTranslations.Add(Keys.Return, KeyAction.TextEditor);
            actionTranslations.Add(Keys.Right, KeyAction.RandomColorAnimated);
            actionTranslations.Add(Keys.S, KeyAction.DrawFilledRectangle);
            actionTranslations.Add(Keys.Scroll, KeyAction.ShowVersion);
            actionTranslations.Add(Keys.Space, KeyAction.DrawFirework1);
            actionTranslations.Add(Keys.Subtract, KeyAction.Note11);
            actionTranslations.Add(Keys.T, KeyAction.DrawBomb);
            actionTranslations.Add(Keys.Tab, KeyAction.KaleidoPaint);
            actionTranslations.Add(Keys.U, KeyAction.DrawArgyleDot);
            actionTranslations.Add(Keys.Up, KeyAction.NoAction);
            actionTranslations.Add(Keys.V, KeyAction.DropCheese);
            actionTranslations.Add(Keys.VolumeDown, KeyAction.VolumeDown);
            actionTranslations.Add(Keys.VolumeMute, KeyAction.VolumeMute);
            actionTranslations.Add(Keys.VolumeUp, KeyAction.VolumeUp);
            actionTranslations.Add(Keys.W, KeyAction.ChannelSwap);
            actionTranslations.Add(Keys.X, KeyAction.DrawCheckerBoard);
            actionTranslations.Add(Keys.Y, KeyAction.DrawWingding);
            actionTranslations.Add(Keys.Z, KeyAction.DrawMaze);

            // Brush selectors belong to keys that don't normally get pressed
            actionTranslations.Add(Keys.F13, KeyAction.BrushSelectCircle);
            actionTranslations.Add(Keys.F14, KeyAction.BrushSelectBomb);
            actionTranslations.Add(Keys.F15, KeyAction.BrushSelectFloodFill);
            actionTranslations.Add(Keys.F16, KeyAction.BrushSelectLifePattern);
            actionTranslations.Add(Keys.F17, KeyAction.BrushSelectLineTarget);
            actionTranslations.Add(Keys.F18, KeyAction.BrushSelectPictureStamp);
            actionTranslations.Add(Keys.F19, KeyAction.BrushSelectRandom);
            actionTranslations.Add(Keys.F20, KeyAction.BrushSelectShader);
            actionTranslations.Add(Keys.F21, KeyAction.BrushSelectSprayPaint);
            actionTranslations.Add(Keys.F22, KeyAction.BrushSelectSquare);


            if (Settings.Registered)
            {
                funKeyTranslations.Add(Keys.F2, FunKeyLifePlayer);
                funKeyTranslations.Add(Keys.F3, FunKeyGravityPlay);
                funKeyTranslations.Add(Keys.F4, FunKeyMazeGame);
                funKeyTranslations.Add(Keys.F5, FunKeyFloodFillPlay);
                funKeyTranslations.Add(Keys.F6, FunKeyBlueChaser);
                funKeyTranslations.Add(Keys.F7, FunKeyGroundFall);
                funKeyTranslations.Add(Keys.F8, FunKeyFuzzyKaleidoscope);
                funKeyTranslations.Add(Keys.F9, FunKeyExploderAndBee);
                funKeyTranslations.Add(Keys.F10, FunKeyTilePainter);
                funKeyTranslations.Add(Keys.F11, FunKeyWaterColor);
                funKeyTranslations.Add(Keys.F12, FunKeyBeesAndAnts);
            }
            else
            {
                funKeyTranslations.Add(Keys.F2, FunKeyUnregistered);
                funKeyTranslations.Add(Keys.F3, FunKeyUnregistered);
                funKeyTranslations.Add(Keys.F4, FunKeyUnregistered);
                funKeyTranslations.Add(Keys.F5, FunKeyUnregistered);
                funKeyTranslations.Add(Keys.F6, FunKeyUnregistered);
                funKeyTranslations.Add(Keys.F7, FunKeyUnregistered);
                funKeyTranslations.Add(Keys.F8, FunKeyUnregistered);
                funKeyTranslations.Add(Keys.F9, FunKeyUnregistered);
                funKeyTranslations.Add(Keys.F10, FunKeyUnregistered);
                funKeyTranslations.Add(Keys.F11, FunKeyUnregistered);
                funKeyTranslations.Add(Keys.F12, FunKeyUnregistered);
            }

        }

        #region KEY TO TEXT TRANSLATIONS
        /// --------------------------------------------------------------------------
        /// <summary>
        /// Set up the keycode translation table
        /// </summary>
        /// --------------------------------------------------------------------------
        private void AssignKeyTranslations()
        {

            keyTranslations.Add(Keys.D0, new char[] { '0', ')' });
            keyTranslations.Add(Keys.D1, new char[] { '1', '!' });
            keyTranslations.Add(Keys.D2, new char[] { '2', '@' });
            keyTranslations.Add(Keys.D3, new char[] { '3', '#' });
            keyTranslations.Add(Keys.D4, new char[] { '4', '$' });
            keyTranslations.Add(Keys.D5, new char[] { '5', '%' });
            keyTranslations.Add(Keys.D6, new char[] { '6', '^' });
            keyTranslations.Add(Keys.D7, new char[] { '7', '&' });
            keyTranslations.Add(Keys.D8, new char[] { '8', '*' });
            keyTranslations.Add(Keys.D9, new char[] { '9', '(' });

            keyTranslations.Add(Keys.A, new char[] { 'a', 'A' });
            keyTranslations.Add(Keys.B, new char[] { 'b', 'B' });
            keyTranslations.Add(Keys.C, new char[] { 'c', 'C' });
            keyTranslations.Add(Keys.D, new char[] { 'd', 'D' });
            keyTranslations.Add(Keys.E, new char[] { 'e', 'E' });
            keyTranslations.Add(Keys.F, new char[] { 'f', 'F' });
            keyTranslations.Add(Keys.G, new char[] { 'g', 'G' });
            keyTranslations.Add(Keys.H, new char[] { 'h', 'H' });
            keyTranslations.Add(Keys.I, new char[] { 'i', 'I' });
            keyTranslations.Add(Keys.J, new char[] { 'j', 'J' });
            keyTranslations.Add(Keys.K, new char[] { 'k', 'K' });
            keyTranslations.Add(Keys.L, new char[] { 'l', 'L' });
            keyTranslations.Add(Keys.M, new char[] { 'm', 'M' });
            keyTranslations.Add(Keys.N, new char[] { 'n', 'N' });
            keyTranslations.Add(Keys.O, new char[] { 'o', 'O' });
            keyTranslations.Add(Keys.P, new char[] { 'p', 'P' });
            keyTranslations.Add(Keys.Q, new char[] { 'q', 'Q' });
            keyTranslations.Add(Keys.R, new char[] { 'r', 'R' });
            keyTranslations.Add(Keys.S, new char[] { 's', 'S' });
            keyTranslations.Add(Keys.T, new char[] { 't', 'T' });
            keyTranslations.Add(Keys.U, new char[] { 'u', 'U' });
            keyTranslations.Add(Keys.V, new char[] { 'v', 'V' });
            keyTranslations.Add(Keys.W, new char[] { 'w', 'W' });
            keyTranslations.Add(Keys.X, new char[] { 'x', 'X' });
            keyTranslations.Add(Keys.Y, new char[] { 'y', 'Y' });
            keyTranslations.Add(Keys.Z, new char[] { 'z', 'Z' });

            keyTranslations.Add(Keys.Oemtilde, new char[] { '`', '~' });
            keyTranslations.Add(Keys.OemMinus, new char[] { '-', '_' });
            keyTranslations.Add(Keys.Oemplus, new char[] { '=', '+' });
            keyTranslations.Add(Keys.OemOpenBrackets, new char[] { '[', '}' });
            keyTranslations.Add(Keys.Oem1, new char[] { ';', ':' });
            keyTranslations.Add(Keys.Oem5, new char[] { '\\', '|' });
            keyTranslations.Add(Keys.Oem6, new char[] { ']', '}' });
            keyTranslations.Add(Keys.Oem7, new char[] { '\'', '\"' });
            keyTranslations.Add(Keys.Oemcomma, new char[] { ',', '<' });
            keyTranslations.Add(Keys.OemPeriod, new char[] { '.', '>' });
            keyTranslations.Add(Keys.OemQuestion, new char[] { '/', '?' });
            keyTranslations.Add(Keys.NumPad0, new char[] { '0', '0' });
            keyTranslations.Add(Keys.NumPad1, new char[] { '1', '1' });
            keyTranslations.Add(Keys.NumPad2, new char[] { '2', '2' });
            keyTranslations.Add(Keys.NumPad3, new char[] { '3', '3' });
            keyTranslations.Add(Keys.NumPad4, new char[] { '4', '4' });
            keyTranslations.Add(Keys.NumPad5, new char[] { '5', '5' });
            keyTranslations.Add(Keys.NumPad6, new char[] { '6', '6' });
            keyTranslations.Add(Keys.NumPad7, new char[] { '7', '7' });
            keyTranslations.Add(Keys.NumPad8, new char[] { '8', '8' });
            keyTranslations.Add(Keys.NumPad9, new char[] { '9', '9' });
            keyTranslations.Add(Keys.Divide, new char[] { '/', '/' });
            keyTranslations.Add(Keys.Space, new char[] { ' ', ' ' });
            keyTranslations.Add(Keys.Multiply, new char[] { '*', '*' });
            keyTranslations.Add(Keys.Subtract, new char[] { '-', '-' });
            keyTranslations.Add(Keys.Decimal, new char[] { '.', '.' });
            keyTranslations.Add(Keys.Add, new char[] { '+', '+' });
            //keyTranslations.Add(Keys, new char[] { '', '' });
            //keyTranslations.Add(Keys, new char[] { '', '' });

            keyTranslations.Add(Keys.Tab, new char[] { '\t', '\t' });
            keyTranslations.Add(Keys.Return, new char[] { '\n', '\n' });
            keyTranslations.Add(Keys.Back, new char[] { (char)127, (char)127 });

            foreach (Keys keyCode in keyTranslations.Keys)
            {
                if (!keyReverseTranslations.ContainsKey(keyTranslations[keyCode][0]))
                {
                    keyReverseTranslations.Add(keyTranslations[keyCode][0], keyCode);
                }
            }
        }
        #endregion

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Generate the keypress requried to activate the key action
        /// </summary>
        /// --------------------------------------------------------------------------
        private void GenerateKeyPress(KeyAction keyAction)
        {
            foreach (KeyValuePair<Keys, KeyAction> keyValuePair in actionTranslations)
            {
                if (keyValuePair.Value == keyAction)
                {
                    KeyEventArgs keyArgs = new KeyEventArgs(keyValuePair.Key);
                    HandleKeys_Normal(keyArgs);
                }
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>                                                                          
        /// Convert a keycode to a string
        /// </summary>
        /// --------------------------------------------------------------------------
        string GetStringFromKeyCode(Keys code, bool shiftIsPressed)
        {
            int shiftIndex = shiftIsPressed ? 1 : 0;
            if (!keyTranslations.ContainsKey(code)) return "";
            return new string(keyTranslations[code][shiftIndex], 1);
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Handle key releases
        /// </summary>
        /// --------------------------------------------------------------------------
        public void KeyUpHandler(object sender, EventArgs e)
        {
            KeyEventArgs keyArgs = (KeyEventArgs)e;
            SetKeyPressValue(keyArgs.KeyCode, false);
        }


        /// --------------------------------------------------------------------------
        /// <summary>
        /// return true if a key is currently pressed
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// --------------------------------------------------------------------------
        public bool KeyIsPressed(Keys key)
        {
            if (keyPressData.ContainsKey(key)) return keyPressData[key] > 0;
            else return false;
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// --------------------------------------------------------------------------
        public void SetKeyPressValue(Keys key, bool value)
        {
            keyPressData[key] = value ? frame : 0;
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// --------------------------------------------------------------------------
        public uint GetKeyPressFrame(Keys key)
        {
            if (keyPressData.ContainsKey(key)) return keyPressData[key];
            else return 0;
        }

        List<EventArgs> keyboardEventsToDo = new List<EventArgs>();
        List<EventArgs> mouseUpEventsToDo = new List<EventArgs>();
        List<EventArgs> mouseDownEventsToDo = new List<EventArgs>();
        List<EventArgs> mouseMoveEventsToDo = new List<EventArgs>();
        object eventsToDoLockHandle = new object();

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Handle keystrokes
        /// </summary>
        /// --------------------------------------------------------------------------
        public void KeyDownHandler(object sender, EventArgs e)
        {
            KeyEventArgs keyArgs = (KeyEventArgs)e;
            lastActivityTime = DateTime.Now;
            lock (eventsToDoLockHandle)
            {
                keyboardEventsToDo.Add(e);
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Handle keystrokes within the protected thread
        /// </summary>
        /// --------------------------------------------------------------------------
        public void Safe_Slate_KeyHandler(object ignoredObject, EventArgs e)
        {
            KeyEventArgs keyArgs = (KeyEventArgs)e;
            int keyIndex = (int)keyArgs.KeyCode;

            // Any regular key press exists stress mode
            if (stressMode && !keyArgs.SuppressKeyPress)
            {
                keyPressData.Clear();
                stressMode = false;
            }

            // return on shift, alt, ctrl if already pressed
            if ((keyArgs.KeyCode == Keys.LShiftKey || keyArgs.KeyCode == Keys.RShiftKey) && ShiftIsPressed()) return;
            if ((keyArgs.KeyCode == Keys.LControlKey || keyArgs.KeyCode == Keys.RControlKey) && CtrlIsPressed()) return;
            if ((keyArgs.KeyCode == Keys.LMenu || keyArgs.KeyCode == Keys.RMenu) && AltIsPressed()) return;

            SetKeyPressValue(keyArgs.KeyCode, true);

            CheckForQuitCode(keyArgs);
            if (GlobalState.EndApplication) return;

            if (GlobalState.RunningAsScreenSaver)
            {
                if (!Settings.Registered || !Settings.PlayableScreensaver)
                {
                    GlobalState.EndApplication = true;
                    return;
                }
            }

            switch (screenMode)
            {
                case ScreenMode.Normal: HandleKeys_Normal(keyArgs); break;
                case ScreenMode.LoadScreen: HandleKeys_ScreenIO(keyArgs); break;
                case ScreenMode.SaveScreen: HandleKeys_ScreenIO(keyArgs); break;
                case ScreenMode.TextEntry: HandleKeys_TextEntry(keyArgs); break;
                default: break;
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Quit code logic.  ENd the program if the user types the quit code.
        /// </summary>
        /// <returns></returns>
        /// --------------------------------------------------------------------------
        private void CheckForQuitCode(KeyEventArgs keyArgs)
        {
            recentlyPressed += GetStringFromKeyCode(keyArgs.KeyCode, ShiftIsPressed());
            if (recentlyPressed.Length > 100)
            {
                recentlyPressed = recentlyPressed.Substring(recentlyPressed.Length - 50, 50);
            }
            if (recentlyPressed.EndsWith(Settings.ExitCode) && !stressMode)
            {
                GlobalState.EndApplication = true;
            }

            // Easter Egg
            if (recentlyPressed.EndsWith("madlibs"))
            {
                funKeyTranslations[Keys.F10] = FunKeyTextCoolness;
                MediaBag.Play(SoundID.Thx, .3);
            }
        }


        /// --------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// --------------------------------------------------------------------------
        bool ShiftIsPressed()
        {
            return KeyIsPressed(Keys.LShiftKey) || KeyIsPressed(Keys.RShiftKey);
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// --------------------------------------------------------------------------
        bool CtrlIsPressed()
        {
            return KeyIsPressed(Keys.LControlKey) || KeyIsPressed(Keys.RControlKey);
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// --------------------------------------------------------------------------
        bool AltIsPressed()
        {
            return KeyIsPressed(Keys.LMenu) || KeyIsPressed(Keys.RMenu);
        }

        Animation.TextEntry textAnimator = null;
        /// --------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyArgs"></param>
        /// --------------------------------------------------------------------------
        void HandleKeys_TextEntry(KeyEventArgs keyArgs)
        {
            MediaBag.Play(SoundID.Click02, 6, .05);

            switch (keyArgs.KeyCode)
            {
                case Keys.Escape:
                    StopAnimations(typeof(Animation.TextEntry));
                    textAnimator = null;
                    screenMode = ScreenMode.Normal;
                    break;
                case Keys.Return:
                    if (ShiftIsPressed())
                    {
                        textAnimator.HandleChar('\n');
                    }
                    else
                    {
                        // Easter Egg
                        if (textAnimator.Text == "whimsicality" &&    
                            (GlobalState.CurrentDrawingColor & 0xffC0) == GlobalState.RainbowColor)
                        {
                            GlobalState.EasterHeads = true;
                            textAnimator.WriteToMainBuffer();
                            MediaBag.Play(SoundID.Flabbergasted2);
                        }
                        else
                        {
                            textAnimator.WriteToMainBuffer();
                        }
                        StopAnimations(typeof(Animation.TextEntry));
                        textAnimator = null;
                        screenMode = ScreenMode.Normal;
                        RenderBrushToolbar();
                    }
                    break;
                case Keys.Up:           textAnimator.HandleChar(TextKey.IncreaseFont); break;
                case Keys.Add:          textAnimator.HandleChar(TextKey.IncreaseFont); break;
                case Keys.Down:         textAnimator.HandleChar(TextKey.DecreaseFont); break;
                case Keys.Subtract:     textAnimator.HandleChar(TextKey.DecreaseFont); break;
                case Keys.Left:         textAnimator.HandleChar(TextKey.NextTypeFace); break;
                case Keys.PageUp:       textAnimator.HandleChar(TextKey.NextTypeFace); break;
                case Keys.Right:        textAnimator.HandleChar(TextKey.PrevTypeFace); break;
                case Keys.PageDown:     textAnimator.HandleChar(TextKey.PrevTypeFace); break;
                default:
                    if (keyTranslations.ContainsKey(keyArgs.KeyCode))
                    {
                        char c = keyTranslations[keyArgs.KeyCode][ShiftIsPressed() ? 1 : 0];
                        textAnimator.HandleChar(c);
                    }
                    break;
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyArgs"></param>
        /// --------------------------------------------------------------------------
        void HandleKeys_ScreenIO(KeyEventArgs keyArgs)
        {
            switch (keyArgs.KeyCode)
            {
                case Keys.Up: SetCurrentScreenSlot(screenSlot - 4); break;
                case Keys.Down: SetCurrentScreenSlot(screenSlot + 4); break;
                case Keys.Left: SetCurrentScreenSlot(screenSlot - 1); break;
                case Keys.Right: SetCurrentScreenSlot(screenSlot + 1); break;
                case Keys.Escape: EndScreenIO(); break;
                case Keys.None: break;
                default:
                    ScreenCommit();
                    break;
            }

        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Handle Keystrokes in Normal Mode
        /// </summary>
        /// --------------------------------------------------------------------------
        private void HandleKeys_Normal(KeyEventArgs keyArgs)
        {
            if (!stressMode)
            {
                if (!keypressLog.ContainsKey(keyArgs.KeyData)) keypressLog[keyArgs.KeyData] = 0;
                keypressLog[keyArgs.KeyData]++;
            }
            // toggle color picker at any key
            if (colorPicker && keyArgs.KeyCode != Keys.C) ToggleColorPicker(cpLeft, cpTop);

            //Handle Exit Conditions
            if (anyKeyExits && keyArgs.KeyCode != Keys.None)
            {
                Close();
                return;
            }
            
            // Show keyboard buffer
            //dvWindow.MainBuffer.DrawFilledRectangle(0, 0, 100, 300, 120);
            //dvWindow.MainBuffer.Print(MediaBag.color_White, MediaBag.font_Status, 0, 100, recentlyPressed);

            int w = dvWindow.MainBuffer.Width;
            int h = dvWindow.MainBuffer.Height;

            int rw = (int)(Utilities.Rand((int)(w * .6)) + w * .2);
            int rh = (int)(Utilities.Rand((int)(h * .6)) + h * .2);
            int rx = Utilities.Rand(w);
            int ry = Utilities.Rand(h);
            int rxw = Utilities.Rand((w - rw) * 2) - (w - rw) / 2;
            int ryh = Utilities.Rand((h - rh) * 2) - (h - rh) / 2;
            int rr = (int)Math.Sqrt(rw * rw + rh * rh)/4;

            if (workingPoints.Count > 0)
            {
                rxw = rx = workingPoints[0].X;
                ryh = ry = workingPoints[0].Y;
            }

            if (workingPoints.Count > 1)
            {
                rw = workingPoints[1].X - rx;
                rh = workingPoints[1].Y - ry;
                rr = (int)Math.Sqrt(rw * rw + rh * rh);
            }

            if (mouseMotion && workingPoints.Count == 0)
            {
                rxw = rx = mouseX;
                ryh = ry = mouseY;
            }


            //Color rc = Color.FromArgb(Utilities.Rand(0xffffff));
            //ushort rc = dvWindow.MainBuffer.GetPaletteColor((uint)rc.ToArgb());
            ushort rc = (ushort)Utilities.Rand(0x10000);
            if (CtrlIsPressed()) rc = GlobalState.CurrentDrawingColor;

            if (AnimationExists(typeof(Animation.Instructions)) && keyArgs.KeyCode != Keys.Up && keyArgs.KeyCode != Keys.Down)
            {
                StopAnimations(typeof(Animation.Instructions));
                return;
            }

            KeyAction action = KeyAction.Unknown;
            if(actionTranslations.ContainsKey(keyArgs.KeyCode))
            {
                action = actionTranslations[keyArgs.KeyCode];
            }

            // Handle special keys
            switch (keyArgs.KeyCode)
            {
                case Keys.Cancel: // Break
                    if (CtrlIsPressed()) passWordHint = true;
                    break;
                case Keys.LControlKey: MediaBag.Play(SoundID.Comma02); break;
                case Keys.LMenu: MediaBag.Play(SoundID.Comma01); break;
                case Keys.LShiftKey: MediaBag.Play(SoundID.Comma00); break;
                case Keys.O:
                    if (CtrlIsPressed()) action = KeyAction.ScreenLoad; 
                    break;
                case Keys.RControlKey: MediaBag.Play(SoundID.Comma02); break;
                case Keys.RMenu: MediaBag.Play(SoundID.Comma01); break;
                case Keys.RShiftKey: MediaBag.Play(SoundID.Comma00); break;
                case Keys.S:
                    if (CtrlIsPressed() && ShiftIsPressed() && AltIsPressed())
                    {
                        action = KeyAction.StressMode;
                    }
                    else if (CtrlIsPressed()) action = KeyAction.ScreenStore;
                    break;
                case Keys.Scroll:
                    if (CtrlIsPressed()) action = KeyAction.PasswordHint;
                    break;
                case Keys.Tab:
                    if (AltIsPressed()) action = KeyAction.PasswordHint;
                    break;
            }

            // Handle Key actions
            switch (action)
            {
                case KeyAction.AnimateColorDiffuser:
                    if (AnimationExists(typeof(Animation.ColorDiffuser)))
                    {
                        StopAnimations(typeof(Animation.ColorDiffuser));
                    }
                    else
                    {
                        AddAnimation(new Animation.ColorDiffuser(this.dvWindow));
                    }
                    break;
                case KeyAction.AnimateGameOfLife:
                    if (AnimationExists(typeof(Animation.GameOfLife)))
                    {
                        StopAnimations(typeof(Animation.GameOfLife));
                    }
                    else
                    {
                        AddAnimation(new Animation.GameOfLife(this.dvWindow));
                    }
                    break;
                case KeyAction.AnimateFader:
                    if (AnimationExists(typeof(Animation.Fader)))
                    {
                        StopAnimations(typeof(Animation.Fader));
                    }
                    else
                    {
                        bool slow = false;
                        if (ShiftIsPressed()) slow = true;
                        if (CtrlIsPressed() || AltIsPressed()) AddAnimation(new Animation.Fader(this.dvWindow, rc, slow));
                        else AddAnimation(new Animation.Fader(this.dvWindow, 0, slow));
                    }
                    break;
                case KeyAction.AnimateGravityRainbow:
                    if (AnimationCount(typeof(Animation.GravityRainbow)) < 5) 
                    {
                        MediaBag.Play(SoundID.Slide04);
                        AddAnimation(new Animation.GravityRainbow(dvWindow, rx, ry, mouseX, mouseY, rc));
                    }
                    break;
                case KeyAction.AnimateGroundCollapse:
                    if (AnimationExists(typeof(Animation.GroundCollapse)))
                    {
                        StopAnimations(typeof(Animation.GroundCollapse));
                    }
                    else
                    {
                        AddAnimation(new Animation.GroundCollapse(this.dvWindow));
                    }
                    break;
                case KeyAction.AnimatePixelDiffuser:
                    if (AnimationExists(typeof(Animation.PixelDiffuser)))
                    {
                        StopAnimations(typeof(Animation.PixelDiffuser));
                    }
                    else
                    {
                        AddAnimation(new Animation.PixelDiffuser(this.dvWindow));
                    }
                    break;
                case KeyAction.AnimateRain:
                    if (AnimationExists(typeof(Animation.Rain)))
                    {
                        StopAnimations(typeof(Animation.Rain));
                    }
                    else
                    {
                        AddAnimation(new Animation.Rain(this.dvWindow));
                    }
                    break;
                case KeyAction.AnimateSimpleScreenFlow:
                    if (AnimationExists(typeof(Animation.ScreenFlowSimple)))
                    {
                        StopAnimations(typeof(Animation.ScreenFlowSimple));
                    }
                    else
                    {
                        ScreenFlowSimpleMode mode = (ScreenFlowSimpleMode)Utilities.Rand((int)ScreenFlowSimpleMode.MaxCount);
                        if (modulator != 0) mode = (ScreenFlowSimpleMode)(modulator % (int)ScreenFlowSimpleMode.MaxCount);
                        AddAnimation(new Animation.ScreenFlowSimple(this.dvWindow, mode));
                    }
                    break;
                case KeyAction.AnimateScreenDecay:
                    if (AnimationExists(typeof(Animation.ScreenDecay)))
                    {
                        StopAnimations(typeof(Animation.ScreenDecay));
                    }
                    else
                    {
                        AddAnimation(new Animation.ScreenDecay(this.dvWindow));
                    }
                    break;
                case KeyAction.AnimateScreenFlow:
                    if (AnimationExists(typeof(Animation.ScreenFlow)))
                    {
                        StopAnimations(typeof(Animation.ScreenFlow));
                    }
                    else
                    {
                        ScreenFlowMode mode = (ScreenFlowMode)Utilities.Rand((int)ScreenFlowMode.MaxCount);
                        if (modulator != 0) mode = (ScreenFlowMode)(modulator % (int)ScreenFlowMode.MaxCount);
                        AddAnimation(new Animation.ScreenFlow(this.dvWindow, mode, rx, ry));
                    }
                    break;
                case KeyAction.AnimateSnow:
                    if (AnimationExists(typeof(Animation.Snow)))
                    {
                        StopAnimations(typeof(Animation.Snow));
                    }
                    else
                    {
                        MediaBag.Play(SoundID.Slide_SleighBells);
                        AddAnimation(new Animation.Snow(this.dvWindow, GlobalState.CurrentDrawingColor));
                    }
                    break;
                case KeyAction.AutoBrush:
                    if (Animation.AutoBrush.TooManyBrushes)
                    {
                        MediaBag.Play(SoundID.Dot_Dinglow, .5);
                    }
                    else
                    {
                        MediaBag.Play(SoundID.Dot_Dinglow);
                        AddAnimation(new Animation.AutoBrush(this.dvWindow, mouseX, mouseY, GlobalState.BrushSize, mouseVelocityX, mouseVelocityY, rc));
                    }
                    break;
                case KeyAction.Bee:
                    Animation.Bee newBee = new Animation.Bee(dvWindow, rx, ry, GlobalState.CurrentDrawingColor);
                    newBee.MouseX = mouseX;
                    newBee.MouseY = mouseY;
                    AddAnimation(newBee);             
                    break;
                case KeyAction.BrushSelectBomb:
                    GlobalState.BrushType = BrushType.Bomb;
                    GlobalState.BrushSound = MediaBag.Play(SoundID.Loop_Hiss, 1.5, .2, true);
                    break;
                case KeyAction.BrushSelectCircle:
                    GlobalState.BrushType = BrushType.Circle;
                    MediaBag.Play(SoundID.Dot08);
                    break;
                case KeyAction.BrushSelectLifePattern:
                    GlobalState.BrushType = BrushType.LifePattern;
                    MediaBag.Play(SoundID.Slide_Cymbalhat);
                    break;
                case KeyAction.BrushSelectFloodFill:
                    GlobalState.BrushType = BrushType.FloodFill;
                    MediaBag.Play(SoundID.Loop_Gargle, 1.6, .2);
                    break;
                case KeyAction.BrushSelectLineTarget:
                    GlobalState.BrushType = BrushType.LineTarget;
                    MediaBag.Play(SoundID.Dot12);
                    targetX = mouseX;
                    targetY = mouseY;
                    if (AltIsPressed()) passWordHint = true;
                    break;
                case KeyAction.BrushSelectPictureStamp:
                    GlobalState.BrushType = BrushType.PictureStamp;
                    MediaBag.Play(SoundID.Dot_Juice);
                    break;
                case KeyAction.BrushSelectRandom:
                    MediaBag.Play(SoundID.Firework_Peep);
                    GlobalState.RandomBrush = !GlobalState.RandomBrush;
                    break;
                case KeyAction.BrushSelectSprayPaint:
                    GlobalState.BrushType = BrushType.SprayPaint;
                    MediaBag.Play(SoundID.Dot_Snuff);
                    break;
                case KeyAction.BrushSelectSquare:
                    GlobalState.BrushType = BrushType.Windmill;
                    MediaBag.Play(SoundID.Dot09);
                    break;
                case KeyAction.BrushSelectShader:
                    GlobalState.BrushType = BrushType.Shader;
                    MediaBag.Play(SoundID.Slide08);
                    break;
                case KeyAction.ChannelSwap:
                    MediaBag.Play(SoundID.RGBSwitch,1,.3);
                    ChannelSwap();
                    break;
                case KeyAction.ClearScreen:
                    MediaBag.Play(SoundID.Dot01);
                    if (AltIsPressed() || CtrlIsPressed()) dvWindow.MainBuffer.Clear(rc);
                    else dvWindow.MainBuffer.Clear(0);
                    break;
                case KeyAction.ColorCounter:
                    MediaBag.Play(SoundID.Dot_Smack);
                    if (AnimationExists(typeof(Animation.ColorCounter)))
                    {
                        StopAnimations(typeof(Animation.ColorCounter));
                    }
                    else
                    {
                        AddAnimation(new Animation.ColorCounter(dvWindow, rx, ry));
                    }
                    break;
                case KeyAction.DrawArgyleDot:
                    MediaBag.Play(SoundID.DTine, 1, .4);
                    AddAnimation(new Animation.ArgyleDot(dvWindow, rc, rx, ry));
                    break;
                case KeyAction.DrawBomb:
                    MediaBag.Play(SoundID.Slide_VoiceSpitKaboom, 1, (GlobalState.BrushSize + 2) / 50.0 * 2);
                    AddAnimation(new Animation.Kaboom(dvWindow, rc, rx, ry, GlobalState.BrushSize));
                    break;
                case KeyAction.ColorPicker:
                    if (CtrlIsPressed()) passWordHint = true;
                    MediaBag.Play(SoundID.Dot03);
                    ToggleColorPicker(cpLeft, cpTop);
                    break;
                case KeyAction.DrawCheckerBoard:
                    AddAnimation(new Animation.CheckerBoard(dvWindow, rc, GlobalState.PreviousDrawingColor, GlobalState.BrushSize));
                    break;
                case KeyAction.DrawCircle:
                    MediaBag.Play(SoundID.Dot05);
                    dvWindow.MainBuffer.DrawCircle(rc, rx, ry, rr);
                    break;
                case KeyAction.DrawFilledCircle:
                    MediaBag.Play(SoundID.Dot02);
                    dvWindow.MainBuffer.DrawFilledCircle(rc, rx, ry, rr);
                    break;
                case KeyAction.DrawFilledCircleActive:
                    MediaBag.Play(SoundID.Slide_DownBongos);
                    if (workingPoints.Count > 1)
                    {
                        AddAnimation(new Animation.ActiveShape(dvWindow, rc, rx, ry, rw, rh, ActiveShapeType.Circle));
                    }
                    else
                    {
                        AddAnimation(new Animation.ActiveShape(dvWindow, rc, rx, ry, rxw/2, ryh/2, ActiveShapeType.Circle));
                    }
                    break;
                case KeyAction.DrawFilledCircleXor:
                    MediaBag.Play(SoundID.Dot10);
                    dvWindow.MainBuffer.DrawFilledCircle(PixelMode.XOR, rc, rx, ry, rr);
                    break;
                case KeyAction.DrawFilledRectangle:
                    MediaBag.Play(SoundID.Dot04);
                    if (workingPoints.Count > 1)
                    {
                        dvWindow.MainBuffer.DrawFilledRectangle(rc, rx, ry, rx + rw, ry + rh);
                    }
                    else
                    {
                        dvWindow.MainBuffer.DrawFilledRectangle(rc, rxw, ryh, rxw + rw, ryh + rh);
                    }
                    break;
                case KeyAction.DrawFilledRectangleActive:
                    MediaBag.Play(SoundID.Slide_RulerTwang);
                    AddAnimation(new Animation.ActiveShape(dvWindow, rc, rx, ry, rw, rh, ActiveShapeType.Square));
                    break;
                case KeyAction.DrawFilledRectangleXor:
                    MediaBag.Play(SoundID.Dot11);
                    if (workingPoints.Count > 1)
                    {
                        dvWindow.MainBuffer.DrawFilledRectangle(PixelMode.XOR, rc, rx, ry, rx + rw, ry + rh);
                    }
                    else
                    {
                        dvWindow.MainBuffer.DrawFilledRectangle(PixelMode.XOR, rc, rxw, ryh, rxw + rw, ryh + rh);
                    }
                    break;
                case KeyAction.DrawFirework1:
                    LaunchFirework(keyArgs, h, rx, rc, (FireworkType)Utilities.Rand((int)FireworkType.MaxCount));
                    break;
                case KeyAction.DrawFirework2:
                    LaunchFirework(keyArgs, h, rx, rc, FireworkType.NormalWithCrackles);
                    break;
                case KeyAction.DrawFirework3:
                    LaunchFirework(keyArgs, h, rx, rc, FireworkType.TowerOfSparks);
                    break;
                case KeyAction.DrawGradient:
                    MediaBag.Play(SoundID.Gradient, 1, .1);
                    AddAnimation(new Animation.Gradient(dvWindow, rc, GlobalState.PreviousDrawingColor));
                    break;
                case KeyAction.DrawLifePattern:
                    MediaBag.Play(SoundID.Dot_Airgun);
                    LifePattern.GlobalPatterns[rc % LifePattern.GlobalPatterns.Count].Draw(dvWindow.MainBuffer, rc, rx, ry);
                    break;
                case KeyAction.DrawLine:
                    MediaBag.Play(SoundID.Dot14);
                    int x1 = rxw;
                    int x2 = rxw + rw;
                    int y1 = ryh;
                    int y2 = ryh + rh;

                    if (workingPoints.Count > 1)
                    {
                        x1 = rx;
                        y1 = ry;
                        x2 = rx + rw;
                        y2 = ry + rh;
                    }
                    else
                    {
                        if (Utilities.Rand(2) == 0)
                        {
                            int temp = x2;
                            x2 = x1;
                            x1 = temp;
                        }
                    }

                    if (ShiftIsPressed()) x2 = x1;
                    if (AltIsPressed()) y2 = y1;
                    if (ShiftIsPressed() && AltIsPressed())
                    {
                        int size = Math.Abs(rw) > Math.Abs(rh) ? rw : rh;
                        x2 = x1 + size;
                        y2 = y1 + size;
                    }
                    
                    dvWindow.MainBuffer.DrawLine(rc, x1, y1, x2, y2);
                    break;
                case KeyAction.DrawLogo:
                    DrawLogo();
                    break;
                case KeyAction.DrawMaze:
                    if (AnimationExists(typeof(Animation.Maze))) StopAnimations(typeof(Animation.Maze));
                    else
                    {
                        MediaBag.Play(SoundID.Slide04);
                        AddAnimation(new Animation.Maze(dvWindow, rc));
                    }
                    break;
                case KeyAction.DrawMiniPicture:
                    MediaBag.Play(SoundID.Dot13);
                    MediaBag.DrawMiniPic(dvWindow.MainBuffer, Utilities.Rand(MediaBag.miniPicCount), rx - 40, ry - 40);
                    break;
                case KeyAction.DrawMoirePattern:
                    MediaBag.Play(SoundID.Slide00);
                    AddAnimation(new Animation.Moire(dvWindow, rx, ry, rc));
                    break;
                case KeyAction.DrawPlasma:
                    if (!AnimationExists(typeof(Animation.Plasma)))
                    {
                        AddAnimation(new Animation.Plasma(dvWindow, rc));
                    }
                    break;
                case KeyAction.DrawRectangle:
                    MediaBag.Play(SoundID.Dot06);
                    if (workingPoints.Count > 1)
                    {
                        dvWindow.MainBuffer.DrawRectangle(rc, rx, ry, rx + rw, ry + rh);
                    }
                    else
                    {
                        dvWindow.MainBuffer.DrawRectangle(rc, rxw, ryh, rxw + rw, ryh + rh);
                    }
                    break;
                case KeyAction.DrawSpirograph:
                    AddAnimation(new Animation.Spirograph(dvWindow, rx, ry, rr, rc));
                    break;
                case KeyAction.DrawTree:
                    {
                        TimeSpan span = DateTime.Now - lastTreeTime;
                        if (span.TotalMilliseconds > 250)
                        {
                            switch (Utilities.Rand(3))
                            {
                                case 0: MediaBag.Play(SoundID.Slide_Birds1, 1.6); break;
                                case 1: MediaBag.Play(SoundID.Slide_Birds2, 1.6); break;
                                case 2: MediaBag.Play(SoundID.Slide_Birds3, 1.6); break;
                            }
                            int treeColor = -1;
                            if (CtrlIsPressed()) treeColor = rc;
                            AddAnimation(new Animation.Tree(dvWindow, rx, (h - ry), treeColor));
                            lastTreeTime = DateTime.Now;
                        }
                    }
                    break;
                case KeyAction.DropCheese:
                    if (ShiftIsPressed())
                    {
                        foreach (Animation.KaCheese cheese in GetAllAnimations(typeof(Animation.KaCheese)))
                        {
                            cheese.ShowMe();
                        }
                    }
                    else if (Animation.KaCheese.TooManyCheeses)
                    {
                        MediaBag.Play(SoundID.Dot_Dinglow, .5);
                    }
                    else
                    {
                        MediaBag.Play(SoundID.Dot_Laugh);
                        AddAnimation(new Animation.KaCheese(this.dvWindow, rc, rx, ry));
                    }
                    break;
                case KeyAction.DrawPolkaDots:
                    AddAnimation(new Animation.PolkaDots(dvWindow, rc, GlobalState.BrushSize));
                    break;
                case KeyAction.DrawSpikes:
                    AddAnimation(new Animation.Spikes(dvWindow, rc, GlobalState.BrushSize));
                    break;
                case KeyAction.DrawWingding:
                    MediaBag.Play(SoundID.Woon);
                    DrawWingding(rx, ry, rc);
                    break;
                case KeyAction.Emboss:
                    MediaBag.Play(SoundID.Fwop, 1, .3);
                    Emboss();
                    break;
                case KeyAction.FindEdges:
                    MediaBag.Play(SoundID.EdgeDetect, 1, .3);
                    FindEdges();
                    break;
                case KeyAction.FlattenAnimatedColors:
                    MediaBag.Play(SoundID.Slide_Wow);
                    FlattenAnimatedColors();
                    break;
                case KeyAction.FlipHorizontal:
                    MediaBag.Play(SoundID.Dot_Cowbell);
                    FlipHorizontal();
                    break;
                case KeyAction.FlipVerticle:
                    MediaBag.Play(SoundID.Dot_Dinghigh);
                    FlipVertical();
                    break;
                case KeyAction.FreezeAnimations:
                    if (!freezeAnimations) MediaBag.Play(SoundID.Slide_Yawn);
                    if (CtrlIsPressed())
                    {
                        StopAnimations(null);
                        //showKeypressData = false;
                    }
                    else
                    {
                        freezeAnimations = !freezeAnimations;
                    }
                    RenderBrushToolbar();
                    break;
                case KeyAction.FunKey:
                    if (funKeyTranslations.ContainsKey(keyArgs.KeyCode) && !stressMode)
                    {
                        funKeyTranslations[keyArgs.KeyCode]();
                    }
                    break;
                case KeyAction.Help:
                    if (!AnimationExists(typeof(Animation.Instructions)))
                    {
                        MediaBag.Play(SoundID.Slide06);
                        Animation helper = new Animation.Instructions(dvWindow);
                        helper.MouseY = mouseY;
                        helper.MouseX = mouseX;
                        AddAnimation(helper);
                    }
                    break;
                case KeyAction.KaleidoPaint:
                    MediaBag.Play(SoundID.Slide05, GlobalState.PaintingStyle == PaintingStyle.Kaleidoscope ? 2 : 1);
                    SetPaintingStyle(PaintingStyle.Kaleidoscope);
                    break;
                case KeyAction.Modulate:
                    MediaBag.Play(SoundID.Dot_Snare, 3, .2);
                    modulator++;
                    if (modulator == 10) modulator = 0;
                    RenderModulatorToolbar();
                    break;
                case KeyAction.NoAction: break;
                case KeyAction.Note00: PlayNote(0); break;
                case KeyAction.Note01: PlayNote(1); break;
                case KeyAction.Note02: PlayNote(2); break;
                case KeyAction.Note03: PlayNote(3); break;
                case KeyAction.Note04: PlayNote(4); break;
                case KeyAction.Note05: PlayNote(5); break;
                case KeyAction.Note06: PlayNote(6); break;
                case KeyAction.Note07: PlayNote(7); break;
                case KeyAction.Note08: PlayNote(8); break;
                case KeyAction.Note09: PlayNote(9); break;
                case KeyAction.Note10: PlayNote(10); break;
                case KeyAction.Note11: PlayNote(11); break;
                case KeyAction.Note12: PlayNote(12); break;
                case KeyAction.PasswordHint:
                    MediaBag.Play(SoundID.Comma00);
                    passWordHint = true;
                    break;
                case KeyAction.RandomBrushSize:
                    MediaBag.Play(SoundID.Dot15,4,.2);
                    GlobalState.TargetBrushSize = Utilities.Rand(GlobalState.MaxBrushSize);
                    break;
                case KeyAction.RandomColor:
                    MediaBag.Play(SoundID.Dot15);
                    GlobalState.SetCurrentDrawingColor(Utilities.Rand(0x8000));
                    break;
                case KeyAction.RandomColorAnimated:
                    MediaBag.Play(SoundID.Dot_Snare);
                    GlobalState.SetCurrentDrawingColor(Utilities.Rand(0x8000) + 0x8000);
                    break;
                case KeyAction.ScreenLoad:
                    StartScreenIO(ScreenMode.LoadScreen);
                    break;
                case KeyAction.ScreenStore:
                    StartScreenIO(ScreenMode.SaveScreen);
                    break;
                case KeyAction.ScreenXor:
                    MediaBag.Play(SoundID.Dot11,.3);
                    dvWindow.MainBuffer.DrawFilledRectangle(PixelMode.XOR, rc, 0, 0, w, h);
                    break;
                case KeyAction.SelectNoteSound:
                    GlobalState.CurrentNoteSound = MediaBag.LastSoundPlayed;
                    MediaBag.Play(MediaBag.LastSoundPlayed);
                    break;
                case KeyAction.ShowVersion:
                    MediaBag.Play(SoundID.Dot07);
                    ShowVersion();
                    break;
                case KeyAction.SprayPaint:
                    MediaBag.Play(SoundID.Dot10);
                    int sw = rw / 5 + 5;
                    if (workingPoints.Count > 1)
                    {
                        sw = (int)(Math.Sqrt(rw * rw + rh * rh));
                    }
                    SprayPaint(rx, ry, sw, 4 * sw, rc);
                    break;
                case KeyAction.StopEveryThing:
                    MediaBag.Play(SoundID.Slide_Screech, .8, .6);
                    HaltEverything();
                    Thread.Sleep(100);
                    break;
                case KeyAction.StressMode:
                    stressMode = true;
                    break;
                case KeyAction.TextEditor:
                    if (AnimationExists(typeof(Animation.TextEntry)))
                    {
                        StopAnimations(typeof(Animation.TextEntry));
                        screenMode = ScreenMode.Normal;
                    }
                    else
                    {
                        textAnimator = new Animation.TextEntry(this.dvWindow, GlobalState.CurrentDrawingColor, mouseX, mouseY);
                        AddAnimation(textAnimator);
                        screenMode = ScreenMode.TextEntry;
                    }
                    RenderBrushToolbar();
                    break;
                case KeyAction.TilePaint:
                    MediaBag.Play(SoundID.Slide01, GlobalState.PaintingStyle == PaintingStyle.Tile ? 2 : 0.5);
                    SetPaintingStyle(PaintingStyle.Tile);
                    break;
                case KeyAction.VolumeDown:
                    MediaBag.Volume -= .2;
                    PlayNote(6);
                    break;
                case KeyAction.VolumeMute:
                    MediaBag.Mute = !MediaBag.Mute;
                    PlayNote(6);
                    break;
                case KeyAction.VolumeUp:
                    MediaBag.Volume += .2;
                    PlayNote(6);
                    break;
                case KeyAction.WorkingPoints:
                    SetWorkingPoint();
                    GlobalState.BrushType = BrushType.Dragging;
                    break;
                default:
                    // Draw a picture and play a sound from our list
                    int keyValue = (int)keyArgs.KeyCode;
                    int soundId = keyValue % (int)SoundID.NumberOfSounds;
                    int miniPicId = keyValue % MediaBag.miniPicCount;
                    MediaBag.Play((SoundID) soundId);
                    MediaBag.DrawMiniPic(dvWindow.MainBuffer, miniPicId, rx, ry);
                    if (GlobalState.Debugging)
                    {
                        dvWindow.OverlayBuffer.DrawFilledRectangle(0, w - 100, h - 15, w, h);
                        dvWindow.OverlayBuffer.Print(rc, MediaBag.font_Status, w - 100, h - 15, keyArgs.KeyCode.ToString());
                    }
                    break;
            }

            if (GlobalState.Debugging)
            {
                // This is used to test exception handling code
                if (!stressMode && keyArgs.KeyCode == Keys.Delete && CtrlIsPressed())
                {
                    while (true) Thread.Sleep(100); // generate a fake hang
                }
                if (!stressMode && keyArgs.KeyCode == Keys.Cancel && CtrlIsPressed())
                {
                    throw new ApplicationException("This is a test Exception");
                }
            }
        }


        /// --------------------------------------------------------------------
        /// <summary>
        /// Pick a random brush and color
        /// </summary>
        /// --------------------------------------------------------------------
        private void SelectRandomBrush()
        {
            MediaBag.Play(SoundID.Firework_Peep, Utilities.DRand(.4) + .8);
            switch (Utilities.Rand(4))
            {
                case 0: GlobalState.BrushType = BrushType.Circle; break;
                case 1: GlobalState.BrushType = BrushType.LifePattern; break;
                case 2: GlobalState.BrushType = BrushType.SprayPaint; break;
                case 3: GlobalState.BrushType = BrushType.PictureStamp; break;
                default: GlobalState.BrushType = BrushType.Windmill; break;
            }

            GlobalState.TargetBrushSize = Utilities.Rand(50) + 1;
            GlobalState.BrushSize = GlobalState.TargetBrushSize;
            GlobalState.SetCurrentDrawingColor(Utilities.Rand(0x10000));
            nextRandomBrushTime = DateTime.Now.AddSeconds(Utilities.DRand(5) + 2);
        }

        /// --------------------------------------------------------------------
        /// <summary>
        /// Set the global painting style
        /// </summary>
        /// --------------------------------------------------------------------
        private void SetPaintingStyle(PaintingStyle newStyle)
        {
            if (GlobalState.PaintingStyle != newStyle)
            {
                GlobalState.PaintingStyle = newStyle;
            }
            else
            {
                GlobalState.PaintingStyle = PaintingStyle.Normal;
            }
            RenderBrushToolbar();
        }

        int screenSlot = 0;

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Set the current slot used for saving and loading the screen buffer
        /// </summary>
        /// <param name="newValue"></param>
        /// --------------------------------------------------------------------------
        void SetCurrentScreenSlot(int newValue)
        {
            if (screenSlot != newValue)
            {
                MediaBag.Play(SoundID.Click00, 2.0);
            }
            screenSlot = newValue + 16;
            if (screenSlot < 0) screenSlot = 0;
            screenSlot = (screenSlot + 16) % 16;
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Give some user feedback to indicate which save slot we will use
        /// </summary>
        /// --------------------------------------------------------------------------
        void ScreenPickerAnimation()
        {
            if (!Settings.Registered)
            {
                dvWindow.MainBuffer.Clear(MediaBag.color_Blue);
                string text =
                    "Screen saving and loading is \n" +
                    "enabled in the registered version\n" +
                    "of PixelWhimsy.  Please register\n" +
                    "at www.pixelwhimsy.com.";

                DrawCenteredText(dvWindow.MainBuffer, text, MediaBag.color_White);
                return;
            }

            int smallWidth = dvWindow.MainBuffer.Width / 4;
            int smallHeight = dvWindow.MainBuffer.Height / 4;
            ushort textColor = dvWindow.MainBuffer.GetPaletteColor((uint)(Color.Yellow.ToArgb()));
            ushort activeColor = 0xa200;
            int x, y, rx, ry;

            for (int i = 0; i < 16; i++)
            {
                x = i % 4;
                y = i / 4;
                rx = x * smallWidth;
                ry = y * smallHeight;

                dvWindow.MainBuffer.DrawRectangle(Color.Gray, rx, ry, rx + smallWidth, ry + smallHeight);
            }

            x = screenSlot % 4;
            y = screenSlot / 4;
            rx = x * smallWidth;
            ry = y * smallHeight;

            string label = "";

            if (screenMode == ScreenMode.LoadScreen)
            {
                activeColor = 0x88D4;
                label = "Open";
            }
            else
            {
                activeColor = 0xC884;
                label = "Save";
            }



            dvWindow.OverlayBuffer.Clear(0);
            for (int i = 0; i < 13; i++)
            {
                Utilities.AnimateColor(ref activeColor, (uint)(i*4));
                dvWindow.OverlayBuffer.DrawRectangle(activeColor, rx+i, ry+i, rx + smallWidth-i, ry + smallHeight-i);
            }

            dvWindow.OverlayBuffer.Print(MediaBag.color_White, MediaBag.font_Status, rx, ry, label);

            //string exitMessage = "Press 'esc' to cancel.";
            //SizeF messageSize = MediaBag.font_Text.Measure(exitMessage);
            //x = (int)((dvWindow.MainBuffer.Width - messageSize.Width) / 2);
            //y = (int)(dvWindow.MainBuffer.Height - messageSize.Height * 2);
            //dvWindow.MainBuffer.Print(0x01, MediaBag.font_Text, x, y, exitMessage);
            //dvWindow.MainBuffer.Print(0x7fff, MediaBag.font_Text, x - 2, y - 2, exitMessage);
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Get the screen ready for saving
        /// </summary>
        /// --------------------------------------------------------------------------
        private void StartScreenIO(ScreenMode newMode)
        {
            if (stressMode && newMode == ScreenMode.SaveScreen) return;
            if (stressMode && Utilities.Rand(1000) < 900) return;

            MediaBag.Play(SoundID.Click01);
            screenMode = newMode;

            if (currentScreen == null)
            {
                currentScreen = dvWindow.MainBuffer.CaptureSprite(0, 0, dvWindow.MainBuffer.Width, dvWindow.MainBuffer.Height);
            }
            else
            {
                dvWindow.MainBuffer.CaptureSpriteFrame(currentScreen, 0, 0, 0);
            }

            dvWindow.MainBuffer.Clear(0);
            dvWindow.OverlayBuffer.Clear(0);
            int smallWidth = dvWindow.MainBuffer.Width / 4;
            int smallHeight = dvWindow.MainBuffer.Height / 4;
            ushort textColor = dvWindow.MainBuffer.GetPaletteColor((uint)(Color.Yellow.ToArgb()));

            Utilities.SetupDataFolders(out whimsyPicsPath, out whimsyDataPath);

            for(int i = 0; i < 16; i++)
            {
                int x = i % 4;
                int y = i / 4;
                int rx = x * smallWidth;
                int ry = y * smallHeight;

                if (savedScreens[i] == null)
                {
                    string fileName = GetDataFileName(i);
                    if (File.Exists(fileName))
                    {
                        LoadPicture(fileName,dvWindow.OverlayBuffer, 4);
                        savedScreens[i] = dvWindow.OverlayBuffer.CaptureSprite(0, 0, smallWidth, smallHeight);
                    }
                }

                if (savedScreens[i] != null)
                {
                    dvWindow.MainBuffer.DrawSprite(savedScreens[i], 0, rx, ry);
                } 


                dvWindow.MainBuffer.DrawRectangle(Color.Gray, rx, ry, rx + smallWidth, ry + smallHeight);
                dvWindow.MainBuffer.Print(textColor, MediaBag.font_Text, rx + smallWidth / 2 - 10, ry + smallHeight / 2 -10 , i.ToString());
            }

            dvWindow.OverlayBuffer.Clear(0);

        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// --------------------------------------------------------------------------
        private void EndScreenIO()
        {
            screenMode = ScreenMode.Normal;
            dvWindow.MainBuffer.DrawSprite(currentScreen, 0, 0, 0);
            leftMouse = rightMouse = middleMouse = false;
            dvWindow.OverlayBuffer.Clear(0);
        }
    }

}
