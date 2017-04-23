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
        /// Draw ArgyleDot around the edge of the screen
        /// </summary>
        /// --------------------------------------------------------------------------
        public class ArgyleDot : Animation
        {
            int x;
            int y;
            int xm=1, ym=1;
            int speed = 1;
            int frame = 0;
            int lastFrame = 40000;
            ushort color;

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Constructor
            /// </summary>
            /// --------------------------------------------------------------------------
            public ArgyleDot(DVWindow window, ushort color, int x, int y)
                : base(window)
            {
                this.x = x;
                this.y = y;
                this.color = color;
                this.speed = 100;
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

                for (int i = 0; i < speed; i++)
                {
                    RenderDot();
                    if (frame > lastFrame)
                    {
                        IsDone = true;
                        return;
                    }
                }
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Render the dot
            /// </summary>
            /// --------------------------------------------------------------------------
            private void RenderDot()
            {
                frame++;
                AnimateColor(ref color, (uint)frame);
                ushort c = color;
                dvWindow.MainBuffer.DrawPixel(c, x, y);
                x += xm;
                y += ym;
                if (x <= 0)
                {
                    x = 0;
                    xm = 1;
                }
                if (x >= dvWindow.Width)
                {
                    x = dvWindow.Width;
                    xm = -1;
                }
                if (y <= 0)
                {
                    y = 0;
                    ym = 1;
                }
                if (y >= dvWindow.Height)
                {
                    y = dvWindow.Height;
                    ym = -1;
                }
            }
        }
    }
}
