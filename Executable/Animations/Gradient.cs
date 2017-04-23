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
        /// Draw a Gradient 
        /// </summary>
        /// --------------------------------------------------------------------------
        public class Gradient : Animation
        {
            int centerx, centery;
            int y;
            int speed = 10;
            int frame = 0;
            uint startR, startG, startB;
            uint targetR, targetG, targetB;
            int spanR, spanB, spanG;
            double errorR, errorG, errorB;
            ushort color;

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Constructor
            /// </summary>
            /// --------------------------------------------------------------------------
            public Gradient(DVWindow window, ushort color1, ushort color2)
                : base(window)
            {
                this.y = dvWindow.Height-1;
                errorR = errorG = errorB = 0;
                startR = (GlobalState.Palette[color1] >> 16) & 0xff;
                startG = (GlobalState.Palette[color1] >> 8) & 0xff;
                startB = (GlobalState.Palette[color1] >> 0) & 0xff;
                targetR = (GlobalState.Palette[color2] >> 16) & 0xff;
                targetG = (GlobalState.Palette[color2] >> 8) & 0xff;
                targetB = (GlobalState.Palette[color2] >> 0) & 0xff;
                spanR = (int)targetR - (int)startR;
                spanG = (int)targetG - (int)startG;
                spanB = (int)targetB - (int)startB;
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
                    if (IsDone == true)
                    {
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
                double ratio = (dvWindow.Height - y) / (double)dvWindow.Height;
                double r = startR + spanR * ratio;
                double er = r - errorR;
                if (er < 0) er = 0;
                if (er > 255) er = 255;
                int choppedR = ((int)(er)) & 0xF8;
                double g = startG + spanG * ratio;
                double eg = g - errorG;
                if (eg < 0) eg = 0;
                if (eg > 255) eg = 255;
                int choppedG = ((int)(eg)) & 0xF8;
                double b = startB + spanB * ratio;
                double eb = b - errorB;
                if (eb < 0) eb = 0;
                if (eb > 255) eb = 255;
                int choppedB = ((int)(eb)) & 0xF8;

                errorR += choppedR - r;
                errorG += choppedG - g;
                errorB += choppedB - b;


                ushort c = (ushort)((choppedR << 7) + (choppedG << 2) + (choppedB >> 3));

                dvWindow.MainBuffer.DrawLine(c, 0, y, dvWindow.Width-1, y);
                this.y--;
                if (y < 0) IsDone = true;
            }

        }
    }
}
