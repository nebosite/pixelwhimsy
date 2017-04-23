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
        /// Conway's game of life
        /// </summary>
        /// --------------------------------------------------------------------------
        public class GameOfLife : Animation
        {
            HiPerfTimer timer = new HiPerfTimer();
            public double milliseconds;

            int pitch; 
            int height;
            int width;
            ushort[] line0;
            ushort[] line1;
            ushort[] line2;

            ushort[] neighborCount0;
            ushort[] neighborCount1;
            ushort[] neighborCount2;

            ushort[] tempLine;
            int y = 0;
            SoundPlayer.SoundInstance sound;

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
                    sound.Finished = true;
                    base.IsDone = value;
                }
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Constructor
            /// </summary>
            /// --------------------------------------------------------------------------
            public GameOfLife(DVWindow window)
                : base(window)
            {
                pitch = dvWindow.MainBuffer.BufferPitch;
                height = dvWindow.MainBuffer.Height;
                width = dvWindow.MainBuffer.Width;

                line0 = new ushort[pitch + 2];
                line1 = new ushort[pitch + 2];
                line2 = new ushort[pitch + 2];
                neighborCount0 = new ushort[pitch + 2];
                neighborCount1 = new ushort[pitch + 2];
                neighborCount2 = new ushort[pitch + 2];
                tempLine = null;
                SetZeroLine(dvWindow.MainBuffer.RawBuffer);

                sound = MediaBag.Play(SoundID.Loop_Chiggers, 1.3, 0.5, true);
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Reset the animation to start from the top line 
            /// </summary>
            /// --------------------------------------------------------------------------
            private void SetZeroLine(ushort[] data)
            {
                Array.Copy(data, (height - 1) * pitch, line0, 1, pitch);
                line0[0] = line0[pitch];
                line0[pitch + 1] = line0[1];

                Array.Copy(data, 0, line1, 1, pitch);
                line1[0] = line1[pitch];
                line1[pitch + 1] = line1[1];

                Array.Copy(data, pitch, line2, 1, pitch);
                line2[0] = line2[pitch];
                line2[pitch + 1] = line2[1];

                for (int i = 0; i < pitch; i++)
                {
                    neighborCount0[i] = 0;
                    neighborCount1[i] = 0;
                    neighborCount2[i] = 0;
                }

                for (int i = 1; i < pitch-1; i++)
                {
                    if (line0[i] > 0)
                    {
                        neighborCount0[i-1]++;
                        neighborCount0[i]++;
                        neighborCount0[i+1]++;
                    }

                    if (line1[i] > 0)
                    {
                        neighborCount0[i - 1]++;
                        neighborCount0[i + 1]++;
                        neighborCount1[i - 1]++;
                        neighborCount1[i]++;
                        neighborCount1[i + 1]++;
                    }
                }

            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Render
            /// </summary>
            /// --------------------------------------------------------------------------
            public override void Render()
            {
                if (IsDone) return;

                ushort[] data = dvWindow.MainBuffer.RawBuffer;
                ushort[] neighborColors = new ushort[8];

                timer.Start();
                
                for (int i = 0; i < height; i++)
                {
                    int writeSpot = y * pitch;

                    // include the next line in the neighborcount
                    for (int j = 1; j <= pitch; j++)
                    {
                        if (line2[j] > 0)
                        {
                            neighborCount0[j - 1]++;
                            neighborCount0[j]++;
                            neighborCount0[j + 1]++;
                            neighborCount1[j - 1]++;
                            neighborCount1[j + 1]++;
                            neighborCount2[j - 1]++;
                            neighborCount2[j]++;
                            neighborCount2[j + 1]++;
                        }
                    }

                    if (line2[pitch+1] > 0)
                    {
                        neighborCount0[pitch]++;
                        neighborCount0[pitch + 1]++;
                        neighborCount1[pitch]++;
                        neighborCount2[pitch]++;
                        neighborCount2[pitch + 1]++;
                    }

                    if (line2[0] > 0)
                    {
                        neighborCount0[0]++;
                        neighborCount0[1]++;
                        neighborCount1[1]++;
                        neighborCount2[0]++;
                        neighborCount2[1]++;
                    }



                    // Decide which pixels live or die
                    for (int x = 1; x <= width; x++)
                    {
                        int neighborCount = neighborCount0[x];
                        ushort localColor = line1[x];
                        if (line1[x] > 0)
                        {
                            if (neighborCount < 2 || neighborCount > 3)
                            {
                                data[writeSpot + x - 1] = 0;   
                            }
                        }
                        else if (neighborCount == 3)
                        {
                            if (line0[x - 1] > 0 && neighborCount > 0) neighborColors[--neighborCount] = line0[x - 1];
                            if (line0[x + 0] > 0 && neighborCount > 0) neighborColors[--neighborCount] = line0[x + 0];
                            if (line0[x + 1] > 0 && neighborCount > 0) neighborColors[--neighborCount] = line0[x + 1];
                            if (line1[x - 1] > 0 && neighborCount > 0) neighborColors[--neighborCount] = line1[x - 1];

                            if (line1[x + 1] > 0 && neighborCount > 0) neighborColors[--neighborCount] = line1[x + 1];
                            if (line2[x - 1] > 0 && neighborCount > 0) neighborColors[--neighborCount] = line2[x - 1];
                            if (line2[x + 0] > 0 && neighborCount > 0) neighborColors[--neighborCount] = line2[x + 0];
                            if (line2[x + 1] > 0 && neighborCount > 0) neighborColors[--neighborCount] = line2[x + 1];

                            data[writeSpot + x - 1] = neighborColors[Rand(3)];
                        }

                    }

                    tempLine = line0;
                    line0 = line1;
                    line1 = line2;
                    line2 = tempLine;

                    tempLine = neighborCount0;
                    neighborCount0 = neighborCount1;
                    neighborCount1 = neighborCount2;
                    neighborCount2 = tempLine;
                    for (int j = 0; j <= pitch+1; j++) neighborCount2[j] = 0;

                    y++;
                    if (y < height-1)
                    {
                        Array.Copy(data, pitch * (y +1), line2, 1, pitch);
                    }
                    else if (y == height - 1)
                    {
                        Array.Copy(data, 0, line2, 1, pitch);
                    }
                    else
                    {
                        Array.Copy(data, pitch, line2, 1, pitch);
                        y = 0;
                        //SetZeroLine(data);
                    }
                    line2[0] = line2[pitch];
                    line2[pitch + 1] = line2[1];
                }

                milliseconds = timer.ElapsedSeconds * 1000;
            }
        }
    }
}
