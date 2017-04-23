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
        ///Draw one little Moire to follow the cursor around
        /// </summary>
        /// --------------------------------------------------------------------------
        public class Moire : Animation
        {
            int centerx, centery;
            Mode mode = Mode.Top;
            int x;
            int y;
            int speed = 30;
            int frame = 0;
            ushort color;

            enum Mode
            {
                Top,
                Right,
                Bottom,
                Left,
                Done
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Constructor
            /// </summary>
            /// --------------------------------------------------------------------------
            public Moire(DVWindow window, int x, int y, ushort color)
                : base(window)
            {
                centerx = x;
                centery = y;
                this.x = 0;
                this.y = 0;
                this.mode = Mode.Top;
                this.color = color;
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
                    RenderLine();
                    if (mode == Mode.Done)
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
            void RenderLine()
            {
                this.frame++;
                AnimateColor(ref color, (uint)frame);
                ushort c = ((frame % 2) == 0) ? (ushort)color : (ushort)0;

                dvWindow.MainBuffer.DrawLine(c, centerx, centery, x, y);

                switch (mode)
                {
                    case Mode.Top:
                        x++;
                        if (x >= dvWindow.MainBuffer.Width - 1) 
                        {
                            x = dvWindow.MainBuffer.Width - 1;
                            mode = Mode.Right;
                        }
                        break;
                    case Mode.Right:
                        y++;
                        if (y >= dvWindow.MainBuffer.Height - 1)
                        {
                            y = dvWindow.MainBuffer.Height - 1;
                            mode = Mode.Bottom;
                        }
                        break;
                    case Mode.Bottom:
                        x--;
                        if (x <= 0)
                        {
                            x = 0;
                            mode = Mode.Left;
                        }
                        break;
                    case Mode.Left:
                        y--;
                        if (y <= 0)
                        {
                            y = 0;
                            mode = Mode.Done;
                        }
                        break;
                }
            }

        }
    }
}
