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
        /// Show a the pixelwhimsy logo
        /// </summary>
        /// --------------------------------------------------------------------------
        public class Logo : Animation
        {
            int x, y;
            TimeWatcher timer = new TimeWatcher(2);

            SoundPlayer.SoundInstance whimsySound = null;

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Constructor
            /// </summary>
            /// --------------------------------------------------------------------------
            public Logo(DVWindow window, int x, int y, SoundID soundID)
                : base(window)
            {
                this.x = x;
                this.y = y;
                whimsySound = MediaBag.Play(soundID);
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Render
            /// </summary>
            /// --------------------------------------------------------------------------
            public override void Render()
            {
                if (IsDone) return;

                if (timer.Expired)
                {
                    if (whimsySound.Finished)
                    {
                        this.IsDone = true;
                    }
                    // uncomment this to reserialize the logo font
                    //MediaBag.font_LogoWhimsy.SerializeToFile("bauhaus93.dvfont");
                    return;
                }

                double progress = 1 - timer.FractionLeft; // (double)whimsySound.PlayPointer / whimsySound.effect.NumSamples;
                ushort color = 0x7fff;

                ushort channel = (ushort)(32 * progress * 2);
                channel = (ushort)(channel % 32);
                ushort red = (ushort)(channel << 10);
                ushort green = (ushort)(channel << 5);
                ushort blue = (ushort)(channel << 0);
                if (progress < .5)
                {
                    dvWindow.MainBuffer.Print(0, MediaBag.font_LogoPixel, x, y, "Pixel");
                    color = (ushort)(red + green + blue);
                    dvWindow.MainBuffer.Print(color, MediaBag.font_LogoPixel, x, y, "Pixel");
                }
                else
                {
                    dvWindow.MainBuffer.Print(0, MediaBag.font_LogoPixel, x, y, "Pixel");
                    dvWindow.MainBuffer.Print(0, MediaBag.font_LogoWhimsy, "Whimsy");
                    dvWindow.MainBuffer.Print(color, MediaBag.font_LogoPixel, x, y, "Pixel");

                    color = red;
                    dvWindow.MainBuffer.Print(color, MediaBag.font_LogoWhimsy, "W");

                    color = (ushort)(red + green);
                    dvWindow.MainBuffer.Print(color, MediaBag.font_LogoWhimsy, "h");

                    color = green;
                    dvWindow.MainBuffer.Print(color, MediaBag.font_LogoWhimsy, "i");

                    color = (ushort)(green + blue);
                    dvWindow.MainBuffer.Print(color, MediaBag.font_LogoWhimsy, "m");

                    color = blue;
                    dvWindow.MainBuffer.Print(color, MediaBag.font_LogoWhimsy, "s");

                    color = (ushort)(blue + red);
                    dvWindow.MainBuffer.Print(color, MediaBag.font_LogoWhimsy, "y");

                    color = dvWindow.MainBuffer.GetPaletteColor((uint)Color.Gray.ToArgb());
                    dvWindow.MainBuffer.Print(color, MediaBag.font_Status, new string((char)0x99,1));
                }


            }
        }
    }
}
