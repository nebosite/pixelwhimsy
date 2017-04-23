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
        /// Big huge explosion
        /// </summary>
        /// --------------------------------------------------------------------------
        public class KaCheese : Animation
        {
            bool exploded = false;
            int x, y;
            ushort color;
            double radius = 0;
            double maxRadius = 0;
            double spin = 0;
            int frame = 0;
            int framesToWaitBeforeExploding = 10;

            static int totalCheeses = 0;
            const int MAXCHEESES = 50;

            public override bool IsDone
            {
                get
                {
                    return base.IsDone;
                }
                set
                {
                    base.IsDone = value;
                    if (value) totalCheeses--;
                }
            }

            public static bool TooManyCheeses { get { return totalCheeses > MAXCHEESES; } }


            /// --------------------------------------------------------------------------
            /// <summary>
            /// Constructor
            /// </summary>
            /// --------------------------------------------------------------------------
            public KaCheese(DVWindow window, ushort color, int x, int y)
                : base(window)
            {

                this.x = x;
                this.y = y;
                this.color = color;
                totalCheeses++;
                ShowMe();
                spin = (DRand(1.0) - 0.5) / 100.0;

                double d = Distance(x, y, 0, 0);
                if (d > maxRadius) maxRadius = d;

                d = Distance(x, y, dvWindow.Width, 0);
                if (d > maxRadius) maxRadius = d;

                d = Distance(x, y, 0, dvWindow.Height);
                if (d > maxRadius) maxRadius = d;

                d = Distance(x, y, dvWindow.Width, dvWindow.Height);
                if (d > maxRadius) maxRadius = d;
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Redraw the cheese in its current location
            /// </summary>
            /// --------------------------------------------------------------------------
            public void ShowMe()
            {
                MediaBag.DrawMiniPic(dvWindow.MainBuffer, 52, x - 20, y - 15);
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Handy distance calculator
            /// </summary>
            /// --------------------------------------------------------------------------
            double Distance(double x1, double y1, double x2, double y2)
            {
                double dx = x1 - x2;
                double dy = y1 - y2;
                return Math.Sqrt(dx * dx + dy * dy);
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Render
            /// </summary>
            /// --------------------------------------------------------------------------
            public override void Render()
            {
                if (IsDone) return;

                if (!exploded)
                {
                    frame++;
                    if (frame > framesToWaitBeforeExploding && Distance(x, y, mousex, mousey) < 15)
                    {
                        exploded = true;
                        MediaBag.Play(SoundID.Thx);
                    }
                    else return;
                }

                int segments = 200;
                double thetaD = (Math.PI * 2) / segments;
                for (int counter = 0; counter < 10; counter++) 
                {
                    frame++;
                    for (int i = 0; i < segments; i++)
                    {
                        double theta1 = i * thetaD + spin * frame;
                        double theta2 = theta1 + thetaD;
                        int checkerSeed = (int)(radius / 10) + (int)(i / 4);
                        ushort c = (checkerSeed % 2 == 0) ? (ushort)0 : color;
                        AnimateColor(ref c, (uint)frame);

                        double x1 = x + radius * Math.Cos(theta1);
                        double x2 = x + radius * Math.Cos(theta2);
                        double y1 = y + radius * Math.Sin(theta1);
                        double y2 = y + radius * Math.Sin(theta2);

                        dvWindow.MainBuffer.DrawLine(c, (int)x1, (int)y1, (int)x2, (int)y2);
                    }
                    radius += .5;
                }

                
                if (radius > maxRadius) IsDone = true;
            }
        }
    }
}
