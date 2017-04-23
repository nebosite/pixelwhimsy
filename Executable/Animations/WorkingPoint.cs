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
        ///Draw one WorkingPoint
        /// </summary>
        /// --------------------------------------------------------------------------
        public class WorkingPoint : Animation
        {
            uint color;
            int frame;
            int x, y;

            public override bool IsDone
            {
                get
                {
                    return base.IsDone;
                }
                set
                {
                    base.IsDone = value;
                    dvWindow.OverlayBuffer.DrawFilledCircle(0, x, y, 6);
                }
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Constructor
            /// </summary>
            /// --------------------------------------------------------------------------
            public WorkingPoint(DVWindow window, int x, int y, Color color)
                : base(window)
            {
                this.x = x;
                this.y = y;
                this.color = (uint)color.ToArgb();
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Render
            /// </summary>
            /// --------------------------------------------------------------------------
            public override void Render()
            {
                if (IsDone) return;

                frame++;
                uint r, g, b;
                r = (color >> 16) & 0xff;
                g = (color >> 8) & 0xff;
                b = (color >> 0) & 0xff;

                if (r == 0) r = (uint)(127 + 126 * Math.Cos(frame / 10.0));
                if (g == 0) g = (uint)(127 + 126 * Math.Cos(frame / 11.0));
                if (b == 0) b = (uint)(127 + 126 * Math.Cos(frame / 13.0));

                ushort wavyColor = dvWindow.OverlayBuffer.GetPaletteColor((r << 16) + (g << 8) + b);
                ushort centerColor = ((frame / 5) % 2) == 0 ? (ushort)0 : (ushort)0x7fff;

                dvWindow.OverlayBuffer.DrawFilledRectangle(wavyColor, x - 5, y - 1, x + 5, y + 1);
                dvWindow.OverlayBuffer.DrawFilledRectangle(wavyColor, x - 1, y - 5, x + 1, y + 5);
                dvWindow.OverlayBuffer.DrawLine(centerColor, x - 4, y, x + 4, y);
                dvWindow.OverlayBuffer.DrawLine(centerColor, x, y - 4, x, y + 4);
            }

        }
    }
}
