using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace DirectVarmint
{

    public partial class PixelBuffer
    {
        /// --------------------------------------------------------------------------
        /// <summary>
        /// An Effect object represents a global effect (blur, fade, etc.) that can
        /// operate on an entire pixelbuffer.  
        /// </summary>
        /// --------------------------------------------------------------------------
        public abstract class Effect
        {
            // Interface
            public abstract void Render(PixelBuffer buffer);

            #region Fade
            /// --------------------------------------------------------------------------
            /// <summary>
            /// Fades to the specified color
            /// </summary>
            /// --------------------------------------------------------------------------
            public class Fade : Effect
            {
                private uint fadeTo;
                private int r, g, b;

                /// --------------------------------------------------------------------------
                /// <summary>
                /// Constructor
                /// </summary>
                /// <param name="fadeTo"></param>
                /// --------------------------------------------------------------------------
                public Fade(Color fadeTo)
                {
                    this.fadeTo = (uint)fadeTo.ToArgb();
                    this.r = (int)(this.fadeTo >> 19) & 0x1f;
                    this.g = (int)(this.fadeTo >> 11) & 0x1f;
                    this.b = (int)(this.fadeTo >> 3) & 0x1f;
                }

                /// --------------------------------------------------------------------------
                /// <summary>
                /// Render
                /// </summary>
                /// --------------------------------------------------------------------------
                public override void Render(PixelBuffer buffer)
                {
                    for (uint i = 0; i < buffer.Width; i++)
                    {
                        for (uint j = 0; j < buffer.Height; j++)
                        {
                            uint color = buffer.mainBuffer[(i + j * buffer.bufferPitch)];
                            uint red = (color >> 10) & 0x1f;
                            uint green = (color >> 5) & 0x1f;
                            uint blue = (color >> 0) & 0x1f;

                            if (red > r) red--;
                            else if (red < r) red++;

                            if (green > g) green--;
                            else if (green < g) green++;

                            if (blue > b) blue--;
                            else if (blue < b) blue++;

                            buffer.mainBuffer[(i + j * buffer.bufferPitch)] = (ushort)(blue + (green << 5) + (red << 10));
                        }
                    }
                }  
            }
            #endregion

            #region BouncingLines

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Draw some bouncing lines on the buffer
            /// </summary>
            /// --------------------------------------------------------------------------
            public class BouncingLines : Effect
            {
                double[,] dtData = new double[2, 4];
                Random dtRand = null;
                int frame = 0;
                int stepsPerFrame = 1;
                
                /// --------------------------------------------------------------------------
                /// <summary>
                /// Constructor.  
                /// </summary>
                /// --------------------------------------------------------------------------
                public BouncingLines(int stepsPerFrame, int width, int height)
                {
                    this.stepsPerFrame = stepsPerFrame;
                    dtRand = new Random();
                    for (int i = 0; i < 2; i++)
                    {
                        dtData[i, 0] = dtRand.Next(width);
                        dtData[i, 1] = dtRand.Next(height);
                        double direction = dtRand.Next(1000) / 500.0 * Math.PI;
                        dtData[i, 2] = Math.Cos(direction) / 2.0;
                        dtData[i, 3] = Math.Sin(direction) / 2.0;
                    }
                }

                /// --------------------------------------------------------------------------
                /// <summary>
                /// Test method to see how fast we can touch all the pixels
                /// </summary>
                /// --------------------------------------------------------------------------
                public override void  Render(PixelBuffer buffer)
                {
                    frame++;
                    int r = (int)(127 + 126 * Math.Sin(frame / 80.0));
                    int g = (int)(127 + 126 * Math.Sin(frame / 190.0));
                    int b = (int)(127 + 126 * Math.Sin(frame / 30.0));

                    for (int j = 0; j < stepsPerFrame; j++)
                    {
                        buffer.DrawLine(
                            Color.FromArgb(r, g, b),
                            (int)dtData[0, 0],
                            (int)dtData[0, 1],
                            (int)dtData[1, 0],
                            (int)dtData[1, 1]);

                        for (int i = 0; i < 2; i++)
                        {
                            dtData[i, 0] += dtData[i, 2];
                            if (dtData[i, 0] < 0 || dtData[i, 0] > buffer.width) dtData[i, 2] = -dtData[i, 2];
                            dtData[i, 1] += dtData[i, 3];
                            if (dtData[i, 1] < 0 || dtData[i, 1] > buffer.height) dtData[i, 3] = -dtData[i, 3];
                        }
                    }
                }
            }
            #endregion
        }
    }
}
