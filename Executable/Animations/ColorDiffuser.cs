using System;
using System.Collections.Generic;
using System.Text;
using DirectVarmint;
using System.Drawing;

namespace PixelWhimsy
{
    public abstract partial class Animation
    {
        /// --------------------------------------------------------------------------
        /// <summary>
        /// Diffuse the image by swapping pixels
        /// </summary>
        /// --------------------------------------------------------------------------
        public class ColorDiffuser : Animation
        {
            SoundPlayer.SoundInstance sound;
            int w, h, pitch;
            ushort[] data;
            public double milliseconds;

            public override bool IsDone
            {
                get
                {
                    return base.IsDone;
                }
                set
                {
                    sound.Finished = true;
                    base.IsDone = value;
                }
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Constructor
            /// </summary>
            /// --------------------------------------------------------------------------
            public ColorDiffuser(DVWindow window)
                : base(window)
            {
                sound = MediaBag.Play(SoundID.Loop_Rumble, 1.0, .2, true);
            }

            int x1 = 0;
            int y1 = 0;
            /// --------------------------------------------------------------------------
            /// <summary>
            /// Render
            /// </summary>
            /// --------------------------------------------------------------------------
            public override void Render()
            {
                if (IsDone) return;

                w = dvWindow.MainBuffer.Width;
                h = dvWindow.MainBuffer.Height;
                pitch = dvWindow.MainBuffer.BufferPitch;
                data = dvWindow.MainBuffer.RawBuffer;

                for (int i = 0; i < 2000; i++)
                {
                    x1 = Rand(w);
                    y1 = Rand(h);
                    
                    DiffuseOnePixel(x1, y1);
                }
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// 
            /// </summary>
            /// --------------------------------------------------------------------------
            private void DiffuseOnePixel(int x1, int y1)
            {
                int totalRed = 3;
                int totalGreen = 3;
                int totalBlue = 3;
                int count = 0;

                for (int x = x1 - 1; x <= x1 + 1; x++)
                {
                    // int x2 = ((x + w) % w);  // wrapping version
                    if (x >= 0 && x < w)
                    {
                        for (int y = y1 - 1; y <= y1 + 1; y++)
                        {
                            // int y2 = ((y + h) % h); // wrapping version
                            if (y >= 0 && y < h)
                            {
                                int color = data[x + y * pitch];

                                if (color >= 0x8000)
                                {
                                    color = Flatten((ushort)color);
                                }

                                totalRed += GlobalState.rgbLookup5bit[color, 0];
                                totalGreen += GlobalState.rgbLookup5bit[color, 1];
                                totalBlue += GlobalState.rgbLookup5bit[color, 2];
                                count++;
                            }
                        }
                    }
                }

                if (count == 0) return;

                ushort newColor = (ushort)(((totalRed / count) << 10) | ((totalGreen / count) << 5) | (totalBlue / count));

                for (int x = x1 - 1; x <= x1 + 1; x++)
                {
                    // int x2 = ((x + w) % w);  // wrapping version
                    if (x >= 0 && x < w)
                    {
                        for (int y = y1 - 1; y <= y1 + 1; y++)
                        {
                            // int y2 = ((y + h) % h); // wrapping version
                            if (y >= 0 && y < h)
                            {
                                int color = data[x + y * pitch];
                                data[x + y * pitch] = newColor;
                            }
                        }
                    }
                }
            }
        }
    }
}
