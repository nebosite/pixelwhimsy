using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;

namespace DirectVarmint
{
    public partial class PixelBuffer
    {
        #region Sprite methods

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Capture the screen area and save it as a sprite
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <returns></returns>
        /// --------------------------------------------------------------------------
        public Sprite CaptureSprite(int x1, int y1, int x2, int y2)
        {
            if (x2 < x1)
            {
                int temp = x1;
                x1 = x2;
                x2 = temp;
            }

            if (y2 < y1)
            {
                int temp = y1;
                y1 = y2;
                y2 = temp;
            }

            Sprite newSprite = new Sprite(x2 - x1, y2 - y1);

            CaptureSpriteFrame(newSprite, x1, y1);

            return newSprite;
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Capture a single frame of an existing sprite (creates a new frame)
        /// </summary>
        /// --------------------------------------------------------------------------
        public void CaptureSpriteFrame(Sprite sprite, int x, int y)
        {
            ushort[] frame = new ushort[sprite.Height * sprite.Width];
            sprite.FrameData.Add(frame);
            CaptureSpriteFrame(sprite, x, y, sprite.FrameData.Count - 1);
        }


        /// --------------------------------------------------------------------------
        /// <summary>
        /// Capture a single frame of an existing sprite
        /// </summary>
        /// --------------------------------------------------------------------------
        public void CaptureSpriteFrame(Sprite sprite, int x, int y, int frameID)
        {
            ushort[] frame = sprite.FrameData[frameID];
            int writeSpot = 0;
            for (int j = y; j < y + sprite.Height; j++)
            {
                for (int i = x; i < x + sprite.Width; i++)
                {
                    frame[writeSpot++] = mainBuffer[i + j * bufferPitch];
                }
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Draw a sprite to the surface
        /// </summary>
        /// --------------------------------------------------------------------------
        public void DrawSprite(PixelBuffer.Sprite sprite, int frame, int x, int y)
        {
            ushort[] data = sprite.FrameData[frame];
            int readSpot = 0;
            int xStart = x;
            int yStart = y;
            int xEnd = x + sprite.Width-1;
            int yEnd = y + sprite.Height-1;
            uint transparentColor = sprite.TransparentColor;

            if (xStart < 0) xStart = 0;
            if (yStart < 0) yStart = 0;

            if (xEnd >= this.Width) xEnd = this.Width - 1;
            if (yEnd >= this.Height) yEnd = this.Height - 1;
            if (xStart > xEnd || yStart > yEnd) return;

            for (int j = yStart; j <= yEnd; j++)
            {
                readSpot = (j - y) * sprite.Width + (xStart - x); 
                for (int i = xStart; i <= xEnd; i++)
                {
                    int writeSpot = i + j * bufferPitch;
                    ushort value = data[readSpot];
                    if (value != transparentColor) mainBuffer[writeSpot] = value;
                    readSpot++;
                }
            }
        }
        #endregion

        #region sprite class

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Represents a raster graphic with 1 or more frames
        /// </summary>
        /// --------------------------------------------------------------------------
        public class Sprite
        {
            /// <summary>
            /// Buffer to hold the sprite data
            /// </summary>

            private List<ushort[]> frameData = new List<ushort[]>();
            private int width = 0;
            private int height = 0;
            private uint transparentColor = 0xffffffff;

            public List<ushort[]> FrameData { get { return this.frameData; } }
            public int Width { get { return this.width; } }
            public int Height { get { return this.height; } }
            public uint TransparentColor { get { return this.transparentColor; } set { this.transparentColor = value; } }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Internal constructor
            /// </summary>
            /// --------------------------------------------------------------------------
            internal Sprite(int width, int height)
            {
                this.width = width;
                this.height = height;
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="fileName"></param>
            /// <param name="spriteWidth"></param>
            /// <param name="spriteHeight"></param>
            /// --------------------------------------------------------------------------
            public Sprite(string fileName, int spriteWidth, int spriteHeight, PaletteConverter getPaletteColor)
            {

                Bitmap bitmap = new Bitmap(Bitmap.FromStream(DVTools.GetStream(fileName)));
                BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                this.width = spriteWidth;
                this.height = spriteHeight;
                int pitch = data.Stride/4;

                // Copy the sprite data into a unit buffer
                int[] tempBuffer = new int[pitch * bitmap.Height];

                Marshal.Copy(data.Scan0, tempBuffer, 0, pitch * data.Height);
                
                bitmap.UnlockBits(data);

                // Normalize the bitmap data into small frame buffers with standard pixel ordering
                int framesAcross = bitmap.Width / spriteWidth;
                int framesDown = bitmap.Height / spriteHeight;
                for(int j = 0; j < framesDown; j++)
                {
                    for (int i = 0; i < framesAcross; i++)
                    {
                        ushort[] frame = new ushort[spriteHeight * spriteWidth];
                        int writeSpot = 0;
                        for (int y = 0; y < spriteHeight; y++)
                        {
                            for (int x = 0; x < spriteWidth; x++)
                            {
                                frame[writeSpot++] = getPaletteColor((uint)tempBuffer[i * spriteWidth + x + (j * spriteHeight + y) * pitch]);
                            }
                        }
                        frameData.Add(frame);
                    }
                }
            }
        }
        #endregion
    }
}
