using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using Microsoft.DirectX;
using Microsoft.DirectX.DirectDraw;


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
        private Device device = null;

        private Clipper graphicsClipper = null;
        private Surface surfacePrimary = null;
        private Surface surfaceSecondary = null; 
        
        private PixelBuffer mainBuffer = null;
        private PixelBuffer overlayBuffer = null;

        internal IntPtr owner;
        private Control ownerControl = null;

        RenderMethod renderMethod = null;
        internal int bufferWidth = -1;
        internal int bufferHeight = -1;
        public uint[] palette = null;
        private bool autoRender = true;
        private bool windowWasReset = false;
        private bool closed = false;
        private uint renderFrame = 0;

        public uint RenderFrame { get { return renderFrame; } }
        public bool Closed { get { return closed; } }
        public bool WindowWasReset { get { return this.windowWasReset; } set { windowWasReset = value; } }

        public bool AutoRender { get { return autoRender; } set { autoRender = value; } }

        // General properties
        public Device Device { get { return this.device; } }

        /// <summary>
        /// Property to access the main pixelbuffer
        /// </summary>
        public PixelBuffer MainBuffer
        {
            get
            {
                if (mainBuffer == null) mainBuffer = CreatePixelBuffer(false);
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
                if (overlayBuffer == null) overlayBuffer = CreatePixelBuffer(true);
                return overlayBuffer;
            }
        }

        HiPerfTimer renderTimer = new HiPerfTimer();
        public double renderMilliseconds = 0;

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Constructor.  Use DVTools.CreateDVWindow to construct this object.
        /// </summary>
        /// --------------------------------------------------------------------------
        internal DVWindow(IntPtr ownerHandle, RenderMethod renderMethod)
        {
            this.owner = ownerHandle;
            this.ownerControl = Control.FromHandle(ownerHandle);

            this.renderMethod = renderMethod;

            device = new Device();
            device.SetCooperativeLevel(ownerControl, CooperativeLevelFlags.Normal);

            OnResetDevice(device, null);
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

            renderTimer.Start();

            if (renderMethod != null) renderMethod();            
            if (device.Disposed || closed) return;

            RECT location;
            User32.GetWindowRect(owner, out location);
            int ownerWidth = location.right - location.left;
            int ownerHeight = location.bottom - location.top;
            
            if (mainBuffer != null) mainBuffer.Render(surfaceSecondary, location.left, location.top, ownerWidth, ownerHeight);
            if (device.Disposed || closed) return;
            
            if (overlayBuffer != null) overlayBuffer.Render(surfaceSecondary, location.left, location.top, ownerWidth, ownerHeight);
            if (device.Disposed || closed) return;

            Flip();
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
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// --------------------------------------------------------------------------
        internal void OnResetDevice(object sender, EventArgs e)
        {
            Device graphicsDevice = (Device)sender;

            // Every surface needs a description
            // This is where you set the parameters for the surface
            SurfaceDescription desc = new SurfaceDescription();
            desc.SurfaceCaps.PrimarySurface = true;

            // Create the surface
            surfacePrimary = new Surface(desc, graphicsDevice);

            graphicsClipper = new Clipper(device);
            graphicsClipper.Window = ownerControl;
            surfacePrimary.Clipper = this.graphicsClipper;

            desc.Clear();
            desc.Width = surfacePrimary.SurfaceDescription.Width;
            desc.Height = surfacePrimary.SurfaceDescription.Height;
            desc.SurfaceCaps.OffScreenPlain = true;
            surfaceSecondary = new Surface(desc, graphicsDevice);

            //// Reset the pixelbuffers
            if (mainBuffer != null) mainBuffer = CreatePixelBuffer(false);
            if (overlayBuffer != null) overlayBuffer = CreatePixelBuffer(true);

            windowWasReset = true;
        }

        /// <summary>
        /// This method flips the secondary surface to the
        /// primary one, thus drawing its content to the screen.
        /// </summary>
        public void Flip()
        {
            if (surfacePrimary == null || surfaceSecondary == null) return;

            try
            {
                surfacePrimary.Draw(surfaceSecondary, DrawFlags.Wait);
            }
            catch (SurfaceLostException)
            {
                surfacePrimary.Restore();
                surfaceSecondary.Restore();
            }
            catch (ArgumentException e)
            {
                if (e.Message != "Value does not fall within the expected range.") throw;
            }
            catch (GraphicsException) { } // Ignore
        } 

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Create a pixelbuffer
        /// </summary>
        /// --------------------------------------------------------------------------
        private PixelBuffer CreatePixelBuffer(bool overlay)
        {
            int pixelWidth = bufferWidth;
            int pixelHeight = bufferHeight;

            RECT clientRectangle;
            User32.GetWindowRect(this.owner, out clientRectangle);
            int ownerWidth = clientRectangle.right - clientRectangle.left;
            int ownerHeight = clientRectangle.bottom - clientRectangle.top;

            if (pixelWidth == -1) pixelWidth = ownerWidth;
            if (pixelHeight == -1) pixelHeight = ownerHeight;

            if (this.palette == null)
            {
                this.palette = new uint[0x8000];
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
            palette[0] = 0x00000000; //3D applications require ff000000

            PixelBuffer newBuffer = new PixelBuffer(
                this.device,
                pixelWidth,
                pixelHeight,
                ownerWidth,
                ownerHeight,
                palette,
                overlay);

            return newBuffer;
        }
    }
}
