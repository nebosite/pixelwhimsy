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
        /// Spirograph animation
        /// </summary>
        /// --------------------------------------------------------------------------
        public class Spirograph : Animation
        {
            int x, y, size;
            int lastx= -9999, lasty = -9999;
            double theta;
            double stopTheta;
            ushort color;
            int frame = 0;
            SoundPlayer.SoundInstance sound;

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
            public Spirograph(DVWindow window, int x, int y, int size, ushort color)
                : base(window)
            {
                this.x = x;
                this.y = y;
                this.color = color;
                this.size = size + 3;
                thetaFactor = Rand(100) / 50.0 + 1;
                sizeF1 = DRand(.8) + .1;
                sizeF2 = 1 - sizeF1;

                stopTheta = Math.PI * 2 * thetaFactor * 50;

                sound = MediaBag.Play(SoundID.Loop_Gargle, .4, .2, true);
            }

            double thetaFactor = 1.1;
            double sizeF1 = 0.4;
            double sizeF2 = 0.6;

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Render
            /// </summary>
            /// --------------------------------------------------------------------------
            public override void Render()
            {
                if (IsDone) return;

                for (int i = 0; i < 100; i++)
                {
                    frame++;

                    double newSize = size * sizeF1 + size * sizeF2 * Math.Sin(theta * thetaFactor);
                    int newx = (int)(x + newSize * Math.Cos(theta));
                    int newy = (int)(y + newSize * Math.Sin(theta));

                    if (theta > stopTheta) IsDone = true;

                    if (lastx != -9999)
                    {
                        dvWindow.MainBuffer.DrawLine(GetAnimatedColor(color, frame), lastx, lasty, newx, newy);
                    }

                    lastx = newx;
                    lasty = newy;
                    theta += .03;

                }

                if (frame > 100000) IsDone = true;
            }

        }
    }
}
