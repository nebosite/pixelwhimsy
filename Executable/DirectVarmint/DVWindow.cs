using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Diagnostics;


namespace DirectVarmint
{
    /// --------------------------------------------------------------------------
    /// <summary>
    /// The DVWindow is the focus for all media activities.  It tracks the DirectX
    /// device, special buffers, sound hardward, etc.
    /// </summary>
    /// --------------------------------------------------------------------------
    public class DVWindow
    {
        DirectVarmintControl underlyingControl;

        private PixelBuffer mainBuffer = null;
        private PixelBuffer overlayBuffer = null;

        internal IntPtr owner;
        private Control ownerControl = null;

        RenderMethod renderMethod = null;
        public uint[] palette = null;
        private bool autoRender = true;
        private bool windowWasReset = false;
        private bool closed = false;
        private uint renderFrame = 0;

        public uint RenderFrame { get { return renderFrame; } }
        public bool Closed { get { return closed; } }
        public bool WindowWasReset { get { return this.windowWasReset; } set { windowWasReset = value; } }

        public bool AutoRender { get { return autoRender; } set { autoRender = value; } }

        HiPerfTimer renderTimer = new HiPerfTimer();
        double renderMilliseconds = 0;
        string lastRenderProblem = null;
        uint frame = 0;

        // General properties
        public double RenderMilliseconds { get { return renderMilliseconds; } set { this.renderMilliseconds = value; } }
        public string LastRenderProblem { get { return lastRenderProblem; } set { this.lastRenderProblem = value; } }
        public uint Frame { get { return frame; } set { this.frame = value; } }
        public int Width { get { return underlyingControl.GraphicsWidth; } }
        public int Height { get { return underlyingControl.GraphicsHeight; } }
        public bool HasMouseInside { get { return underlyingControl.HasMouseInside; } }

        /// <summary>
        /// Property to access the main pixelbuffer
        /// </summary>
        public PixelBuffer MainBuffer
        {
            get
            {
                if (mainBuffer == null) mainBuffer = CreatePixelBuffer();
                return mainBuffer;
            }
        }

        /// <summary>
        /// Set this to generate an externally public palette for the pixel buffers
        /// </summary>
        public uint[] Palette { get { return this.palette; } set { this.palette = value; } }

        /// <summary>
        /// Property to access the overlay pixelbuffer
        /// </summary>
        public PixelBuffer OverlayBuffer
        {
            get
            {
                if (overlayBuffer == null) overlayBuffer = CreatePixelBuffer();
                return overlayBuffer;
            }
        }


        /// --------------------------------------------------------------------------
        /// <summary>
        /// Constructor.  Use DVTools.CreateDVWindow to construct this object.
        /// </summary>
        /// --------------------------------------------------------------------------
        internal DVWindow(Control owner, RenderMethod renderMethod)
        {
            Init(owner, renderMethod, owner.Width, owner.Height);
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Constructor.  Use DVTools.CreateDVWindow to construct this object.
        /// </summary>
        /// --------------------------------------------------------------------------
        internal DVWindow(Control owner, RenderMethod renderMethod, int width, int height)
        {
            Init(owner, renderMethod, width, height);
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Initialize this window
        /// </summary>
        /// --------------------------------------------------------------------------
        private void Init(Control owner, RenderMethod renderMethod, int width, int height)
        {
            if (width <= 0) width = owner.Width;
            if (height <= 0) height = owner.Height;
            underlyingControl = new DirectVarmintControl(width, height);
            underlyingControl.Dock = DockStyle.Fill;
            underlyingControl.Width = owner.Width;
            underlyingControl.Height = owner.Height;

            owner.SuspendLayout();
            owner.Controls.Add(underlyingControl);
            owner.ResumeLayout(false);


            this.owner = owner.Handle;
            this.ownerControl = owner;

            this.renderMethod = renderMethod;
        }

        
        /// --------------------------------------------------------------------------
        /// <summary>
        /// Renders the windows
        /// </summary>
        /// --------------------------------------------------------------------------
        public void Render()
        {
            
            renderFrame++;
            if (closed) return;


            if (renderMethod != null) renderMethod();            

            renderTimer.Start();
            RECT location;
            User32.GetWindowRect(owner, out location);

            LastRenderProblem = null;

            // Dump the PixelBuffers into the drawing surface
            try
            {
                int[] rgbBuffer = underlyingControl.RgbBuffer;

                //////////////////////////
                // RENDER MAIN BUFFER
                //////////////////////////
                if (mainBuffer != null)
                {
                    ushort[] sourceBuffer = mainBuffer.RawBuffer;
                    int i = 0;
                    int end = sourceBuffer.Length - 4;


                    while (i < end)
                    {
                        rgbBuffer[i] = (int)palette[sourceBuffer[i]];
                        rgbBuffer[i + 1] = (int)palette[sourceBuffer[i + 1]];
                        rgbBuffer[i + 2] = (int)palette[sourceBuffer[i + 2]];
                        rgbBuffer[i + 3] = (int)palette[sourceBuffer[i + 3]];
                        i += 4;
                    }

                    //// catch straglers
                    while (i < sourceBuffer.Length)
                    {
                        rgbBuffer[i] = (int)palette[sourceBuffer[i]];
                        i++;
                    }

                }

                /////////////////////////////
                //  RENDER OVERLAY BUFFER
                // (Index 0 is transparent)
                /////////////////////////////
                if (overlayBuffer != null)
                {
                    ushort[] sourceBuffer = overlayBuffer.RawBuffer;
                    int i = 0;
                    int end = sourceBuffer.Length - 4;

                    while (i < end)
                    {
                        if (sourceBuffer[i] > 0) rgbBuffer[i] = (int)palette[sourceBuffer[i]];
                        if (sourceBuffer[i + 1] > 0) rgbBuffer[i + 1] = (int)palette[sourceBuffer[i + 1]];
                        if (sourceBuffer[i + 2] > 0) rgbBuffer[i + 2] = (int)palette[sourceBuffer[i + 2]];
                        if (sourceBuffer[i + 3] > 0) rgbBuffer[i + 3] = (int)palette[sourceBuffer[i + 3]];
                        i += 4;
                    }
                    //// catch straglers
                    while (i < sourceBuffer.Length)
                    {
                        if (sourceBuffer[i] > 0) rgbBuffer[i] = (int)palette[sourceBuffer[i]];
                        i++;
                    }
                }

                underlyingControl.UpdateBits();
                underlyingControl.Invalidate();
            }
            catch (Exception e)
            {
                LastRenderProblem = e.ToString();
                Debug.WriteLine(e.ToString());
            }
            finally
            {
            }

            frame++;
            renderMilliseconds = renderTimer.ElapsedSeconds * 1000;
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// close this window
        /// </summary>
        /// --------------------------------------------------------------------------
        public void Close()
        {
            closed = true;
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Create a pixelbuffer
        /// </summary>
        /// --------------------------------------------------------------------------
        private PixelBuffer CreatePixelBuffer()
        {
            RECT clientRectangle;

            if (this.palette == null)
            {
                this.palette = new uint[0x10000];
            }

            // Generate a 5 bit rgb palette
            double colorFactor = 255.0 / 31.0;

            for (int r = 0; r < 32; r++)
            {
                int redValue = (int)(r * colorFactor);

                for (int g = 0; g < 32; g++)
                {
                    int greenValue = (int)(g * colorFactor);

                    for (int b = 0; b < 32; b++)
                    {
                        int blueValue = (int)(b * colorFactor);

                        uint c = (uint)((r << 10) + (g << 5) + b);
                        palette[c] = 0x00000000 | (uint)((redValue << 16) + (greenValue << 8) + blueValue);
                    }
                }
            }
            palette[0] = 0x00000000;

            DVTools.FixPalette(palette);

            PixelBuffer newBuffer = new PixelBuffer(
                Width,
                Height,
                Width,
                palette);

            return newBuffer;
        }

    }
}
