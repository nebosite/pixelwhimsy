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
        /// Generate a plasma
        /// </summary>
        /// --------------------------------------------------------------------------
        public class Plasma : Animation
        {
            SoundPlayer.SoundInstance sound;
            HiPerfTimer timer = new HiPerfTimer();
            public double milliseconds;

            int y = 0;
            double xFactor;
            double yFactor;
            ushort color;
            double randLevel;

            ushort[] colorCache;

            /// <summary>
            /// Stop the sound
            /// </summary>
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
            public Plasma(DVWindow window, ushort color)
                : base(window)
            {
                sound = MediaBag.Play(SoundID.Loop_Gargle, 1.3, 0.2, true);
                this.color = color;
                xFactor = (double)(GlobalState.BrushSize + 1) / dvWindow.MainBuffer.Width / 10.0;
                yFactor = (double)(GlobalState.BrushSize + 1) / dvWindow.MainBuffer.Height / 10.0;
                randLevel = Utilities.DRand(10);

                colorCache = new ushort[500];

                if (color < 0x8000)
                {
                    ushort[] colorLevels = new ushort[64];

                    int redTarget;
                    int greenTarget;
                    int blueTarget;
                    int white = 0x1f;

                    Utilities.GetColorTargets(color, out redTarget, out greenTarget, out blueTarget);

                    // fade from white to color
                    for (int i = 0; i < 16; i++)
                    {
                        int newRed = (white * (16 - i)) / 16 + (redTarget * i) / 16;
                        int newGreen = (white * (16 - i)) / 16 + (greenTarget * i) / 16;
                        int newBlue = (white * (16 - i)) / 16 + (blueTarget * i) / 16;

                        colorLevels[i] = (ushort)((newRed << 10) + (newGreen << 5) + newBlue);
                    }

                    // fade from color to black
                    for (int i = 0; i < 16; i++)
                    {
                        int newRed = (redTarget * (16 - i)) / 16;
                        int newGreen = (greenTarget * (16 - i)) / 16;
                        int newBlue = (blueTarget * (16 - i)) / 16;

                        colorLevels[i + 16] = (ushort)((newRed << 10) + (newGreen << 5) + newBlue);
                    }

                    // Fade all the way back to white
                    for (int i = 0; i < 32; i++)
                    {
                        colorLevels[i + 32] = colorLevels[31 - i];
                    }

                    for (int i = 0; i < colorCache.Length; i++)
                    {
                        colorCache[i] = colorLevels[i % 64];
                    }
                }
                else
                {
                    for (int i = 0; i < colorCache.Length; i++)
                    {
                        AnimateColor(ref color, (uint)i);
                        colorCache[i] = color;
                    }
                }
            }


            /// --------------------------------------------------------------------------
            /// <summary>
            /// Render
            /// </summary>
            /// --------------------------------------------------------------------------
            public override void Render()
            {
                if (IsDone) return;

                ushort[] data = dvWindow.MainBuffer.RawBuffer;
                int height = dvWindow.MainBuffer.Height;
                int width = dvWindow.MainBuffer.Width;
                int pitch = dvWindow.MainBuffer.BufferPitch;

                for (uint j = 0; j < 7; j++)                 
                {
                    int spot = (int)(y * pitch);
                    for (uint x = 0; x < width; x++)
                    {
                        double value = Utilities.Noise.PerlinNoise_3D(x * xFactor, y * yFactor, randLevel);
                        data[spot] = colorCache[((int)(value * 500))];
                        spot++;
                    }

                    y++;
                    if (y >= height)
                    {
                        this.IsDone = true;
                        break;
                    }
                }
            }
        }
    }
}
