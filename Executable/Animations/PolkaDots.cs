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
        /// Draw PolkaDots around the edge of the screen
        /// </summary>
        /// --------------------------------------------------------------------------
        public class PolkaDots : Animation
        {
            int speed = 5;
            int frame = 0;
            int size;
            ushort color;
            List<Dot> dots = new List<Dot>();

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Constructor
            /// </summary>
            /// --------------------------------------------------------------------------
            public PolkaDots(DVWindow window, ushort color, int size)
                : base(window)
            {
                this.size = size;
                this.color = color;
                this.speed = 60 - size;
                if (speed < 1) speed = 1;

            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Render
            /// </summary>
            /// --------------------------------------------------------------------------
            public override void Render()
            {
                if (IsDone) return;

                MediaBag.Play(SoundID.Pop, speed / 40.0 + .5, .1);
                for (int i = 0; i < speed; i++)
                {
                    if(!RenderDot())
                    {
                        IsDone = true;
                        return;
                    }
                }
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Draw just one line of the pattern
            /// </summary>
            /// --------------------------------------------------------------------------
            bool RenderDot()
            {
                this.frame++;
                AnimateColor(ref color, (uint)frame);
                ushort c = color;

                for(int i = 0; ; i++)
                {
                    if(i == 30) return false;
                    int x = Utilities.Rand(GlobalState.resolutionX);
                    int y = Utilities.Rand(GlobalState.resolutionY);
                    int r = size - size / 20 +  Utilities.Rand(size / 10) + 2;

                    int minRadius = (int)(size*2 + 15);

                    bool drawOK = true;
                    foreach(Dot d in dots)
                    {
                        double distance = Utilities.GetDistance(x,y,d.Location.X, d.Location.Y);
                        if(distance < minRadius)
                        {
                            drawOK = false;
                            break;
                        }
                    }

                    if (drawOK)
                    {
                        dvWindow.MainBuffer.DrawFilledCircle(c, x, y, r);

                        dots.Add(new Dot(x,y,r));
                        return true;
                    }
                }



            }

        }

        struct Dot
        {
            public Point Location;
            public double Radius;
            public Dot(int x, int y, double r)
            {
                this.Location = new Point(x,y);
                this.Radius = r;
            }
        }
    }
}
