using System;
using System.Collections.Generic;
using System.Text;
using DirectVarmint;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

namespace PixelWhimsy
{
    public partial class Slate
    {
        private int id = -1;
        double daysLeft = 10000;

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Empty constructor for testing
        /// </summary>
        /// --------------------------------------------------------------------------
        private Slate()
        {

        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Set up the graphics for this window
        /// </summary>
        /// --------------------------------------------------------------------------
        public Slate(int id)
        {
            this.id = id;
            InitializeComponent();
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Default Initialization
        /// </summary>
        /// --------------------------------------------------------------------------
        public void Initialize()
        {
            Initialize(null);
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Set up the graphics for this window
        /// </summary>
        /// --------------------------------------------------------------------------
        public void Initialize(Control displayWindow)
        {
            InitializeGraphics(displayWindow);

            AssignKeyTranslations();
            AssignKeyActions();

            daysLeft = AssemblyConstants.DaysLeftToExpiration();
            expired = daysLeft < 0;

            GlobalState.SetCurrentDrawingColor(Utilities.Rand(0x10000));
            GlobalState.BrushType = BrushType.Circle;

            keyPressData.Clear();
            StartThreadedRender();
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Reset the slate
        /// </summary>
        /// --------------------------------------------------------------------------
        internal void Reset()
        {
            lock (animationQueueLockHandle)
            {
                animationQueue.Clear();
            }
            keyPressData.Clear();
            savedScreens = new PixelBuffer.Sprite[16];
            stressMode = false;

            SetToDefaultState();
       }

       /// --------------------------------------------------------------------------
       /// <summary>
       /// Put the program in default mode
       /// </summary>
       /// --------------------------------------------------------------------------
        private void SetToDefaultState()
        {
            GlobalState.PaintingStyle = PaintingStyle.Normal;

            showStats = true;
            //showKeypressData = false;
            passWordHint = false;
            colorPickerSprite = null;
            currentScreen = null;
            screenMode = ScreenMode.Normal;
            freezeAnimations = false;
            colorPicker = false;
            textAnimator = null;
            GlobalState.RandomBrush = false;
            modulator = 0;

            RenderBrushToolbar();
            RenderAnimationToolbar();
            RenderModulatorToolbar();
            GlobalState.SetRGBPalette();
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Set up the graphics for this window
        /// </summary>
        /// --------------------------------------------------------------------------
        public void InitializeGraphics(Control displayWindow)
        {
            this.labelPleaseWait.Left = (this.Width - this.labelPleaseWait.Width) / 2;
            this.labelPleaseWait.Top = (this.Height - this.labelPleaseWait.Height) / 2;
            Application.DoEvents();

            int targetwidth = this.ClientRectangle.Width - GlobalState.ToolBarBorder;
            int targetHeight = this.ClientRectangle.Height - GlobalState.ToolBarBorder;

            int realWidth = targetwidth;
            int realHeight = (targetwidth * 3) / 4;

            if (realHeight > targetHeight)
            {
                realHeight = targetHeight;
                realWidth = (targetHeight * 4) / 3;
            }

            panelGame.Width = realWidth;
            panelGame.Height = realHeight;
            panelGame.Left = (this.Width - panelGame.Width) / 2 + 30;
            panelGame.Top = (this.Height - panelGame.Height) / 2 + 15;

            //int w = this.ClientRectangle.Width - GlobalState.ToolBarBorder;
            //int h = this.ClientRectangle.Height - GlobalState.ToolBarBorder;
            //// Force proportions to be a 4x3 rectangle
            //if (w / h > 1.34) w = (int)(h * 1.34);
            //else if (w / h < 1.33) h = (int)(w / 1.33);

            //this.panelGame.Width = w;// (int)(GlobalState.resolutionX * sizeMultiplier);
            //this.panelGame.Height = h;// (int)(GlobalState.resolutionY * sizeMultiplier);
            
            
            //this.panelGame.Left = (this.Width - this.panelGame.Width) / 2 + 30;
            //this.panelGame.Top = (this.Height - this.panelGame.Height) / 2 + 15;

            if (displayWindow == null)
            {
                displayWindow = this.panelGame;
            }
            dvWindow = DVTools.CreateDVWindow(displayWindow, MyRenderMethod, GlobalState.resolutionX, GlobalState.resolutionY);
            dvWindow.Palette = GlobalState.Palette;

            if (!GlobalState.RunningInPreview)
            {
                this.panelBrushes.Width = 80;
                this.panelBrushes.Height = 440;
                this.panelBrushes.Left = this.panelGame.Left - 90;
                this.panelBrushes.Top = (this.Height - this.panelBrushes.Height) / 2;
                dvWindowBrushes = DVTools.CreateDVWindow(this.panelBrushes, null);
                dvWindowBrushes.Palette = GlobalState.ToolPalette;
                dvWindowBrushes.AutoRender = false;
                RenderBrushToolbar();

                this.panelModulators.Width = 16;
                this.panelModulators.Height = 160;
                this.panelModulators.Left = this.panelGame.Right + 10;
                this.panelModulators.Top = (this.Height - this.panelModulators.Height) / 2;
                dvWindowModulators = DVTools.CreateDVWindow(this.panelModulators, null);
                dvWindowModulators.Palette = GlobalState.ToolPalette;
                dvWindowModulators.AutoRender = false;
                RenderModulatorToolbar();

                this.panelAnimations.Width = 400;
                this.panelAnimations.Height = 40;
                this.panelAnimations.Left = (this.Width - this.panelAnimations.Width) / 2;
                if (this.panelAnimations.Left < 232) this.panelAnimations.Left = 232; // Make room for logo
                this.panelAnimations.Top = this.panelGame.Top - 60;
                dvWindowAnimations = DVTools.CreateDVWindow(this.panelAnimations, null);
                dvWindowAnimations.Palette = GlobalState.ToolPalette;
                dvWindowAnimations.AutoRender = false;
                RenderAnimationToolbar();
            }

            dvWindow.MainBuffer.Clear(Color.Black);
            dvWindow.OverlayBuffer.Clear(Color.Black);
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Place the slate at a particular location on the desktop and maximize it
        /// </summary>
        /// <param name="newX"></param>
        /// <param name="newY"></param>
        /// --------------------------------------------------------------------------
        public void PlaceWindow(int newX, int newY, int width, int height)
        {
            this.SetDesktopLocation(newX, newY);

            if (width == -1)
            {
                this.Width = Screen.PrimaryScreen.WorkingArea.Width;
                this.Height = Screen.PrimaryScreen.WorkingArea.Height;
            }
            else
            {
                this.Width = width;
                this.Height = height;
            }
        }

    }
}
