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
        public class CheckerBoard : Animation
        {
            int x, y, size, step;

            ushort color1, color2;
            List<Point> squaresToDraw = new List<Point>();
            int startCount = 0;

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
                    base.IsDone = value;
                }
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Constructor
            /// </summary>
            /// --------------------------------------------------------------------------
            public CheckerBoard(DVWindow window, ushort color1, ushort color2, int seedSize)
                : base(window)
            {
                this.size = seedSize + 5;
                this.color1 = color1;
                this.color2 = color2;

                int w = window.Width / size + 2;
                int h = window.Height / size + 2;

                for (int i = 0; i < w; i++)
                {
                    for (int j = 0; j < h; j++)
                    {
                        squaresToDraw.Add(new Point(i, j));
                    }
                }

                startCount = w * h;
                step = startCount / 30 + 1;
                this.x = -(window.Width % size) / 2;
                this.y = -(window.Height % size) / 2;
            }


            /// --------------------------------------------------------------------------
            /// <summary>
            /// Render
            /// </summary>
            /// --------------------------------------------------------------------------
            public override void Render()
            {
                if (IsDone) return;

                MediaBag.Play(SoundID.Click03, 1.5 - (squaresToDraw.Count / (double)startCount), .3);
             
                for (int count = 0; count < step; count++)                 
                {
                    if (squaresToDraw.Count == 0)
                    {
                        IsDone = true;
                        break;
                    }

                    int index = Rand(squaresToDraw.Count);
                    int sx = squaresToDraw[index].X;
                    int sy = squaresToDraw[index].Y;
                    int x1 = x + sx * size;
                    int y1 = y + sy * size;
                    int x2 = x + sx * size + size - 1;
                    int y2 = y + sy * size + size - 1;
                    ushort color = ((sx + sy) % 2 == 0) ? color1 : color2;
                    AnimateColor(ref color, (uint)squaresToDraw.Count);
                    squaresToDraw.RemoveAt(index);
                    dvWindow.MainBuffer.DrawFilledRectangle(color, x1, y1, x2, y2);
                }
            }
        }
    }
}
