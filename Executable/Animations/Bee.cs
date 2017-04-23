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
        ///Draw one little bee to follow the cursor around
        /// </summary>
        /// --------------------------------------------------------------------------
        public class Bee : Animation
        {
            double x;
            double y;
            double lastx=-1;
            double lasty=-1;
            double xm;
            double ym;
            double direction;
            double velocity;
            ushort color;
            int lifeTime;
            SoundPlayer.SoundInstance sound;

            public override bool IsDone
            {
                get
                {
                    return base.IsDone;
                }
                set
                {
                    dvWindow.OverlayBuffer.DrawPixel(0, (int)lastx, (int)lasty);
                    sound.Finished = true;
                    base.IsDone = value;
                }
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Constructor
            /// </summary>
            /// --------------------------------------------------------------------------
            public Bee(DVWindow window, int x, int y, ushort color)
                : base(window)
            {
                this.x = lastx = x;
                this.y = lasty = y;
                this.color = color;

                xm = 0;
                ym = 0;
                lifeTime = 2000;

                sound = MediaBag.Play(SoundID.Loop_Buzz_High, 1.0 + DRand(.25) - .125, 1, true);

                direction = DRand(2) * Math.PI;
                velocity = 1;
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Render
            /// </summary>
            /// --------------------------------------------------------------------------
            public override void Render()
            {
                if (IsDone) return;

                for (int i = 0; i < 4; i++)
                {
                    RenderOneStep();
                }
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Render just one step
            /// </summary>
            /// --------------------------------------------------------------------------
            private void RenderOneStep()
            {
                double dx = x - mousex;
                double dy = y - mousey;
                double d = Math.Sqrt(x * x + y * y);

                if (Rand(2) == 0)
                {
                    direction += DRand(1) - .5;
                }

                xm = velocity * Math.Cos(direction);
                ym = velocity * Math.Sin(direction);

                double directionChange = 0;
                if(Rand(3) == 0) directionChange = Utilities.GetDirectionChange(xm, ym, dx, dy);

                // Handle navigation
                direction += directionChange * .1;

                // Handle movement

                x += xm;
                y += ym;

                Utilities.AnimateColor(ref color, (uint)(lifeTime));

                dvWindow.OverlayBuffer.DrawPixel(0, (int)lastx, (int)lasty);
                dvWindow.MainBuffer.DrawPixel(color, (int)lastx, (int)lasty);
                dvWindow.OverlayBuffer.DrawPixel(Color.White, (int)x, (int)y);
                lastx = x;
                lasty = y;
                lifeTime--;
                if (lifeTime < 1)
                {
                    this.IsDone = true;
                    dvWindow.OverlayBuffer.DrawPixel(0, (int)lastx, (int)lasty);
                }
            }

        }
    }
}
