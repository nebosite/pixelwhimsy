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
        /// AutoBrushes just hang around
        /// </summary>
        /// --------------------------------------------------------------------------
        public class AutoBrush : Animation
        {
            double x;
            double y;
            double lastx=-1;
            double lasty=-1;
            double xm;
            double ym;
            double size;
            ushort color;
            uint frame = 0;
            int lifeTime;

            static int totalBrushes = 0;
            const int MAXBRUSHES = 50;

            public override bool IsDone
            {
                get
                {
                    return base.IsDone;
                }
                set
                {
                    base.IsDone = value;
                    if (value) totalBrushes--;
                }
            }

            public static bool TooManyBrushes { get { return totalBrushes > MAXBRUSHES; } }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Constructor
            /// </summary>
            /// --------------------------------------------------------------------------
            public AutoBrush(DVWindow window, int x, int y, int size, double xm, double ym, ushort color)
                : base(window)
            {
                this.x = lastx = x;
                this.y = lasty = y;
                this.xm = xm;
                this.ym = ym;
                this.size = size;
                this.color = color;
                this.lifeTime = 2500;

                totalBrushes++;
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Render
            /// </summary>
            /// --------------------------------------------------------------------------
            public override void Render()
            {
                if (IsDone) return;

                double d = Math.Sqrt(xm * xm + ym * ym);
                if (d < 1.0) d = 1.0;
                double xm1 = xm / d;
                double ym1 = ym / d;
                double a = 0;
                int right = dvWindow.MainBuffer.Width - 1;
                int bottom = dvWindow.MainBuffer.Height - 1;

                while (a <= d)
                {
                    frame++;
                    AnimateColor(ref color, frame); 
                    dvWindow.MainBuffer.DrawFilledCircle(color, (int)x, (int)y, (int)size);
                    x += xm1;
                    y += ym1;
                    if (x > right - size || x < size)
                    {
                        xm1 = -xm1;
                        xm = -xm;
                    }
                    if (y > bottom - size || y < size)
                    {
                        ym1 = -ym1;
                        ym = -ym;
                    }
                    a += 1.0;
                }


                lastx = x;
                lasty = y;

                lifeTime--;
                if (lifeTime == 0) IsDone = true;
            }

        }
    }
}
