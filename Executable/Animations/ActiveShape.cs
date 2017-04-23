using System;
using System.Collections.Generic;
using System.Text;
using DirectVarmint;
using System.Drawing;

namespace PixelWhimsy
{
    public enum ActiveShapeType
    {
        Circle,
        Square
    }

    public abstract partial class Animation
    {
        /// --------------------------------------------------------------------------
        /// <summary>
        /// Generate a ActiveShape
        /// </summary>
        /// --------------------------------------------------------------------------
        public class ActiveShape : Animation
        {
            int x, y, w, h = 0;
            ushort color;
            ushort complimentaryColor;
            ActiveShapeType type;
            int count = 0;
            int finish;

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Constructor
            /// </summary>
            /// --------------------------------------------------------------------------
            public ActiveShape(DVWindow window, ushort color, int x, int y, int w, int h, ActiveShapeType type )
                : base(window)
            {
                if (w < 0)
                {
                    w = -w;
                    x -= w;
                }

                if (h < 0)
                {
                    h = -h;
                    y -= h;
                }

                this.x = x;
                this.y = y;
                this.w = w;
                this.h = h;


                this.type = type;
                this.color = color;
                this.complimentaryColor = (ushort)(color ^ 0x7fff);
                this.finish = (int)(Math.Sqrt(w * w + h * h));
            }


            /// --------------------------------------------------------------------------
            /// <summary>
            /// Render
            /// </summary>
            /// --------------------------------------------------------------------------
            public override void Render()
            {
                if (IsDone) return;

                for (uint j = 0; j < 4; j++)                 
                {
                    count++;
                    ushort c = ((count/4) % 2) == 0 ? color : complimentaryColor;
                    if (color > 0x8000)
                    {
                        c = color;
                        AnimateColor(ref c, (uint)count);
                    }

                    switch (type)
                    {
                        case ActiveShapeType.Circle:
                            dvWindow.MainBuffer.DrawFilledCircle(c, x , y, finish - count);
                            break;
                        case ActiveShapeType.Square:
                            dvWindow.MainBuffer.DrawRectangle(c, x + count, y + count, x + w - count, y + h - count);
                            break;
                    }
                    if (count >= finish)
                    {
                        IsDone = true;
                        break;
                    }
                }
            }
        }
    }
}
