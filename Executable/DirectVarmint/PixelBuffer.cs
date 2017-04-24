using System;
using System.Collections.Generic;
using System.Text;
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

        // General buffer information
        int width;
        int height;
        ushort[] mainBuffer = null;
        uint[] palette = null;
        int bufferPitch = 1;

        // Properties
        public int Width { get { return this.width; } }
        public int Height { get { return this.height; } }

        /// <summary>
        /// Use this for raw access to the main buffer, but do not
        /// hold on to it, because the buffer handle will sometimes change.
        /// </summary>
        public ushort[] RawBuffer { get { return mainBuffer; } }

        public int BufferPitch { get { return width; } }

		/// --------------------------------------------------------------------------
        /// <summary>
        /// Constructor
        /// </summary>
        /// --------------------------------------------------------------------------
        public PixelBuffer(int width, int height, int bufferPitch, uint[] palette)
        {
            this.width = width;
            this.height = height;
            this.bufferPitch = bufferPitch;

            mainBuffer = new ushort[bufferPitch * height];
            this.palette = palette;
        }
    }
}
