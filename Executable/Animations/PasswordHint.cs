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
        /// Show a password hint to the user for a short time
        /// </summary>
        /// --------------------------------------------------------------------------
        public class PasswordHint : Animation
        {
            string hintText;
            SizeF size;

            int lastx=0, lasty=0;

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Constructor
            /// </summary>
            /// --------------------------------------------------------------------------
            public PasswordHint(DVWindow window, string hint)
                : base(window)
            {
                hintText = Utilities.BreakString(MediaBag.font_Status, (int)(dvWindow.MainBuffer.Width * .8), hint);
                size = MediaBag.font_Status.Measure(hintText);
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Render
            /// </summary>
            /// --------------------------------------------------------------------------
            public override void Render()
            {

                TimeSpan span = DateTime.Now - start;

                dvWindow.OverlayBuffer.DrawFilledRectangle(0, lastx, lasty, (int)(lastx + size.Width + 10), (int)(lasty + size.Height + 10));
                if (IsDone) return;

                int x = (int)(20 + 10 * Math.Cos(span.TotalSeconds * 4));
                int y = (int)(20 + 10 * Math.Sin(span.TotalSeconds * 4));
                int yellow = (int)(25 + 6 * Math.Cos(span.TotalSeconds * 3));
                ushort printColor = (ushort)((yellow << 10) + (yellow << 5));

                if (span.TotalSeconds < 7)
                {
                    dvWindow.OverlayBuffer.DrawFilledRectangle(Color.Blue, x, y, (int)(x + size.Width + 10), (int)(y + size.Height + 10));
                    dvWindow.OverlayBuffer.Print(printColor, MediaBag.font_Status, x+5, y+5, hintText);
                    lastx = x; lasty = y;
                }
                else
                {
                    IsDone = true;
                }
            }
        }
    }
}
