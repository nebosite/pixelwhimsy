using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX.DirectDraw;
using Microsoft.DirectX;
using System.Drawing;
using System.Threading;

namespace DirectVarmint
{
    /// <summary>
    /// Definition for a method that can convert an rgb color to a ushort palette index
    /// </summary>
    /// <param name="rgbColor"></param>
    /// <returns></returns>
    public delegate ushort PaletteConverter(uint rgbColor);

    /// --------------------------------------------------------------------------
    /// <summary>
    /// A pixelbuffer object is a graphics buffer with methods for drawing on 
    /// it similar to how we did graphics in the old DOS days. :)
    /// </summary>
    /// --------------------------------------------------------------------------
    public partial class PixelBuffer
    {
        public const int DEFAULTWIDTH = -1;
        public const int DEFAULTHEIGHT = -1;

        // DirectX information
        Device device = null;
        GraphicsStream bufferStream = null;
        int realBufferPitch = 1;
        int bufferPitch = 1;
        Surface sourceSurface;
        SurfaceDescription surfaceDescription;
        DrawFlags drawFlags;

        // General buffer information
        int width;
        int displayWidth;
        int height;
        int displayHeight;
        ushort[] mainBuffer = null;
        uint[] paletteBuffer = null;
        uint[] palette = null;
        bool transparent = false;
        bool surfaceLost = false;

        // Internal data
        uint frame = 0;
        int screenX, screenY;

        // Properties
        public int Width { get { return this.width; } }
        public int Height { get { return this.height; } }

        /// <summary>
        /// Use this for raw access to the main buffer, but do not
        /// hold on to it, because the buffer handle will sometimes change.
        /// </summary>
        public ushort[] TemporaryBufferBits { get { return mainBuffer; } }

        public int BufferPitch { get { return bufferPitch; } }

		/// --------------------------------------------------------------------------
        /// <summary>
        /// Constructor
        /// </summary>
        /// --------------------------------------------------------------------------
        public PixelBuffer(Device device, int width, int height, int displayWidth, int displayHeight, uint[] palette, bool transparent)
        {
            this.device = device;
            this.width = width;
            this.height = height;
            this.displayWidth = displayWidth;
            this.displayHeight = displayHeight;

            CreateSurface(device, width, height, transparent);

            mainBuffer = new ushort[bufferPitch * height];
            paletteBuffer = new uint[bufferPitch * height];
            this.palette = palette;
        }

        private void CreateSurface(Device device, int width, int height, bool transparent)
        {
            try
            {
                // Create a description
                surfaceDescription = new SurfaceDescription();
                surfaceDescription.SurfaceCaps.OffScreenPlain = true;
                surfaceDescription.Width = width;
                surfaceDescription.Height = height;

                sourceSurface = new Surface(surfaceDescription, device);
                LockedData lockedData = sourceSurface.Lock(LockFlags.ReadOnly);
                realBufferPitch = lockedData.Pitch;
                bufferPitch = realBufferPitch / 4;
                sourceSurface.Unlock();
                drawFlags = DrawFlags.Wait;

                this.transparent = transparent;
                if (transparent)
                {
                    // Create a new color key and choose color = black;
                    ColorKey tempKey = new ColorKey();
                    tempKey.ColorSpaceHighValue = 0;
                    tempKey.ColorSpaceLowValue = 0;
                    sourceSurface.SetColorKey(ColorKeyFlags.SourceDraw, tempKey);
                    drawFlags |= DrawFlags.KeySource;
                }

                surfaceLost = false;
            }
            catch (UnsupportedModeException)
            {

            }

        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Set the display location of the buffer
        /// </summary>
        /// --------------------------------------------------------------------------
        public void SetLocation(int x, int y)
        {
            screenX = x;
            screenY = y;
        }

        HiPerfTimer renderTimer = new HiPerfTimer();
        public double renderMilliseconds = 0; 
        /// --------------------------------------------------------------------------
        /// <summary>
        /// Call this to update the texture
        /// </summary>
        /// --------------------------------------------------------------------------
        public void Render(Surface surface, int x, int y, int destWidth, int destHeight)
        {
            if (surfaceLost)
            {
                Thread.Sleep(200);
                CreateSurface(device, width, height, transparent);
                return;
            }

            try
            {
                renderTimer.Start();

                LockedData lockedData = new LockedData();
                try
                {
                    lockedData = sourceSurface.Lock(LockFlags.WriteOnly);
                }
                catch (ArgumentException e)
                {
                    if (e.Message != "Value does not fall within the expected range.") throw;
                    else return;
                }

                bufferStream = lockedData.Data;
                int newPitch = lockedData.Pitch;
                if (newPitch != realBufferPitch) throw new ApplicationException("Pitch value mismatch");

                int i = 0;
                int end = mainBuffer.Length - 4;

                while (i < end)
                {
                    paletteBuffer[i] = palette[mainBuffer[i]];
                    paletteBuffer[i + 1] = palette[mainBuffer[i + 1]];
                    paletteBuffer[i + 2] = palette[mainBuffer[i + 2]];
                    paletteBuffer[i + 3] = palette[mainBuffer[i + 3]];
                    i += 4;
                }

                //// catch straglers
                while (i < mainBuffer.Length)
                {
                    paletteBuffer[i] = palette[mainBuffer[i]];
                    i++;
                }

                bufferStream.Write(paletteBuffer);
                bufferStream.Dispose();
                sourceSurface.Unlock();
                //surface.DrawFast(x, y, sourceSurface, DrawFastFlags.DoNotWait);

                surface.Draw(new Rectangle(x, y, destWidth, destHeight),
                    sourceSurface, drawFlags);
            }
            catch (AccessViolationException)
            {
                // Ignore
            }
            catch (InvalidOperationException)
            {
                // Ignore
            }
            catch (InvalidRectangleException)
            {
                // Ignore
            }
            catch (SurfaceLostException)
            {
                surfaceLost = true;
            }

            frame++;
        }
    }
}
