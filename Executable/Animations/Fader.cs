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
        /// Fade the screen to a color
        /// </summary>
        /// --------------------------------------------------------------------------
        public class Fader : Animation
        {
            SoundPlayer.SoundInstance sound;
            HiPerfTimer timer = new HiPerfTimer();
            public double milliseconds;

            uint redTarget;
            uint greenTarget;
            uint blueTarget;
            int speed;
            double soundDecay;
            ushort[] fadeTo = new ushort[0x10000];

            int y = 0;

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

            double soundTheta = 0;
            /// --------------------------------------------------------------------------
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="color"></param>
            /// <param name="slow">half speed if true</param>
            /// --------------------------------------------------------------------------
            public Fader(DVWindow window, ushort color, bool slow)
                : base(window)
            {
                this.redTarget = (uint)(color >> 10) & 0x1f;
                this.greenTarget = (uint)(color >> 5) & 0x1f;
                this.blueTarget = (uint)(color >> 0) & 0x1f;
                sound = MediaBag.Play(SoundID.Loop_Hum_Low, 1.3,.1,true);

                speed = 8;
                if (slow)
                {
                    speed = 16;
                }

                // prep the color fade matrix
                for (uint c = 0; c < 0x8000; c++)
                {
                    uint red = (c >> 10) & 0x1f;
                    uint green = (c >> 5) & 0x1f;
                    uint blue = (c >> 0) & 0x1f;

                    if (red > redTarget) red--;
                    else if (red < redTarget) red++;

                    if (green > greenTarget) green--;
                    else if (green < greenTarget) green++;

                    if (blue > blueTarget) blue--;
                    else if (blue < blueTarget) blue++;

                    fadeTo[c] = (ushort)(blue + (green << 5) + (red << 10));
                }
            }

            int frame = 0;
            /// --------------------------------------------------------------------------
            /// <summary>
            /// Render
            /// </summary>
            /// --------------------------------------------------------------------------
            public override void Render()
            {
                if (IsDone) return;

                frame++;
                bool doit =  (frame % speed == 0);

                ushort[] data = dvWindow.MainBuffer.RawBuffer;
                int height = dvWindow.MainBuffer.Height;
                int width = dvWindow.MainBuffer.Width;
                int pitch = dvWindow.MainBuffer.BufferPitch;

                timer.Start();

                soundTheta += (24 - speed) / 300.0;
                if (soundTheta > Math.PI) soundTheta -= Math.PI;

                sound.RelativeFrequency = .4 + .3 * Math.Cos(soundTheta);
                sound.Volume = Math.Sin(soundTheta) * .2;
                

                for (int i = 0x8000; i < 0x10000; i++)
                {
                    fadeTo[i] = Flatten((ushort)i);
                }

                for (uint j = 0; j < dvWindow.MainBuffer.Height; j++)
                {
                    int spot = (int)(y * pitch);
                    for (uint i = 0; i < width; i++)
                    {
                        uint color = data[spot];
                        if ((color & 0x8000) == 0)
                        {
                            if(doit) data[spot] = fadeTo[color];
                        }
                        else
                        {
                            data[spot] = Flatten((ushort)color);
                        }
                        spot++;

                    }
                    y++;
                    if (y >= height) y -= height;
                }

                milliseconds = timer.ElapsedSeconds * 1000;
                //dvWindow.OverlayBuffer.DrawFilledRectangle(0, 0, 0, 100, 10);
                //dvWindow.OverlayBuffer.Print(0x7fff, MediaBag.font_Status, 0, 0, milliseconds.ToString("0.00"));
            }
        }
    }
}
