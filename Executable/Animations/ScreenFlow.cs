using System;
using System.Collections.Generic;
using System.Text;
using DirectVarmint;
using System.Drawing;

namespace PixelWhimsy
{
    public enum ScreenFlowMode
    {
        Radiate,
        BlackHole,
        StarHole,
        SpiralClockwise,
        SpiralCounterClockwise,
        RotateClockwise,
        RotateCounterClockwise,
        StrongWavySpiralClockWise,
        StrongWavySpiralCounterClockWise,
        WeakWavySpiralClockWise,
        WeakWavySpiralCounterClockWise,
        XWaves,
        YWaves,
        XYWaves,
        MaxCount
    }

    public abstract partial class Animation
    {

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Pixels flow
        /// </summary>
        /// --------------------------------------------------------------------------
        public class ScreenFlow : Animation
        {
            int centerx, centery;
            int contributionMultiplier = 256;
            SoundPlayer.SoundInstance sound;
            int frame = 0;

            int[] flowTemplateQ1;
            int[] flowTemplateQ2;
            int[] flowTemplateQ3;
            int[] flowTemplateQ4;

            int[] flowContributionQ1;
            int[] flowContributionQ2;
            int[] flowContributionQ3;
            int[] flowContributionQ4;

            ScreenFlowMode flowMode;
            static ushort[] tempBuffer = null;

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
            public ScreenFlow(DVWindow window, ScreenFlowMode flowMode, int x, int y)
                : base(window)
            {
                sound = MediaBag.Play(SoundID.Loop_Rumble2, 1.3, 0.2, true);

                this.flowMode = flowMode;
                this.centerx = x;
                this.centery = y;

                GenerateFlowTemplate(flowMode);

            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Create a flow template for the given mode
            /// </summary>
            /// --------------------------------------------------------------------------
            private void GenerateFlowTemplate(ScreenFlowMode flowMode)
            {
                int pitch = dvWindow.MainBuffer.BufferPitch;
                int w = dvWindow.MainBuffer.Width;
                int h = dvWindow.MainBuffer.Height;
                int size = pitch * h;

                flowTemplateQ1 = new int[pitch * h];
                flowTemplateQ2 = new int[pitch * h];
                flowTemplateQ3 = new int[pitch * h];
                flowTemplateQ4 = new int[pitch * h];

                flowContributionQ1 = new int[pitch * h];
                flowContributionQ2 = new int[pitch * h];
                flowContributionQ3 = new int[pitch * h];
                flowContributionQ4 = new int[pitch * h];

                double dx, dy, d;

                for (int y = 0; y < dvWindow.MainBuffer.Height; y++)
                {
                    int writeSpot = y * pitch;
                    int readSpot = y * pitch;
                    for (int x = 0; x < w; x++)
                    {
                        double fromx, fromy;
                        switch (flowMode)
                        {
                            case ScreenFlowMode.XYWaves:
                                fromx = x + Math.Cos(y / 10.0) * .5;
                                fromy = y + Math.Cos(x / 10.0) * .5;

                                CalcPixel(pitch, w, h, writeSpot, x, fromx, fromy);
                                break;
                            case ScreenFlowMode.XWaves:
                                fromx = x + Math.Cos(y / 10.0) * .3;
                                fromy = y + .5;

                                CalcPixel(pitch, w, h, writeSpot, x, fromx, fromy);
                                break;
                            case ScreenFlowMode.YWaves:
                                fromx = x + .5;
                                fromy = y + Math.Cos(x / 10.0) * .3;

                                CalcPixel(pitch, w, h, writeSpot, x, fromx, fromy);
                                break;
                            case ScreenFlowMode.StrongWavySpiralCounterClockWise:
                                CircleType(pitch, w, h, y, writeSpot, x, delegate(double a, double b) { return a + .05 * Math.Sin(b / 20) - .01; });
                                break;
                            case ScreenFlowMode.StrongWavySpiralClockWise:
                                CircleType(pitch, w, h, y, writeSpot, x, delegate(double a, double b) { return a + .05 * Math.Sin(b / 20) + .01; });
                                break;
                            case ScreenFlowMode.WeakWavySpiralCounterClockWise:
                                CircleType(pitch, w, h, y, writeSpot, x, delegate(double a, double b) { return a - .01 * Math.Cos(b / 10) + .03; });
                                break;
                            case ScreenFlowMode.WeakWavySpiralClockWise:
                                CircleType(pitch, w, h, y, writeSpot, x, delegate(double a, double b) { return a + .01 * Math.Cos(b / 10) + .03; });
                                break;
                            case ScreenFlowMode.SpiralClockwise:
                                CircleType(pitch, w, h, y, writeSpot, x, delegate(double a, double b) { return a - .001 * Math.Sqrt(b); });
                                break;
                            case ScreenFlowMode.SpiralCounterClockwise:
                                CircleType(pitch, w, h, y, writeSpot, x, delegate(double a, double b) { return a + .001 * Math.Sqrt(b); });
                                break;
                            case ScreenFlowMode.RotateClockwise:
                                CircleType(pitch, w, h, y, writeSpot, x, delegate(double a, double b) { return a - .03; });
                                break;
                            case ScreenFlowMode.RotateCounterClockwise:
                                CircleType(pitch, w, h, y, writeSpot, x, delegate(double a, double b) { return a + .03; });
                                break;
                            case ScreenFlowMode.StarHole:
                                dx = x - centerx;
                                dy = y - centery;
                                d = Math.Sqrt(dx * dx + dy * dy);

                                double theta = Utilities.GetAngle(dx, dy, 1, 0);
                                fromx = x + Math.Cos(theta * 10) * dx / 60;
                                fromy = y + Math.Cos(theta * 10) * dy / 60;

                                CalcPixel(pitch, w, h, writeSpot, x, fromx, fromy);
                                break;
                            case ScreenFlowMode.BlackHole:
                                dx = x - centerx;
                                dy = y - centery;
                                d = Math.Sqrt(dx * dx + dy * dy);

                                fromx = x + dx / 30;
                                fromy = y + dy / 30;

                                CalcPixel(pitch, w, h, writeSpot, x, fromx, fromy);
                                break;
                            case ScreenFlowMode.Radiate:
                                dx = x - centerx;
                                dy = y - centery;
                                d = Math.Sqrt(dx * dx + dy * dy);

                                fromx = x - dx / 30;
                                fromy = y - dy / 30;

                                CalcPixel(pitch, w, h, writeSpot, x, fromx, fromy);
                                break;
                        }
                    }
                }
            }

            delegate double AngleModifier(double theta, double d);

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Generic method for generating circle type flows
            /// </summary>
            /// --------------------------------------------------------------------------
            private void CircleType(int pitch, int w, int h, int y, int writeSpot, int x, AngleModifier CalcAngle)
            {
                double dx, dy, d, theta, fromx, fromy;


                dx = x - centerx;
                dy = y - centery;
                d = Math.Sqrt(dx * dx + dy * dy);

                theta = Utilities.GetAngle(dx, dy, 1, 0);
                fromx = centerx + Math.Cos(CalcAngle(theta,d)) * d;
                fromy = centery + Math.Sin(CalcAngle(theta, d)) * d;

                CalcPixel(pitch, w, h, writeSpot, x, fromx, fromy);
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Figure out the contributions for a particular pixel
            /// </summary>
            /// <param name="pitch"></param>
            /// <param name="w"></param>
            /// <param name="h"></param>
            /// <param name="writeSpot"></param>
            /// <param name="x"></param>
            /// <param name="getFromSpot"></param>
            /// <param name="fromx"></param>
            /// <param name="fromy"></param>
            /// <returns></returns>
            /// --------------------------------------------------------------------------
            private int CalcPixel(int pitch, int w, int h, int writeSpot, int x, double fromx, double fromy)
            {
                int getFromSpot = -1;

                if (fromx < 0) fromx += w;
                if (fromy < 0) fromy += h;
                if (fromx > w) fromx -= w;
                if (fromy > h) fromy -= h;

                int ifromx = (int)fromx;
                int ifromy = (int)fromy;

                double xpart = fromx - ifromx;
                double ypart = fromy - ifromy;

                getFromSpot = ifromx + ifromy * pitch;

                // Q1 (upper left)
                if (fromx >= 0 && fromx < w && fromy >= 0 && fromy < h)
                {
                    flowTemplateQ1[writeSpot + x] = (int)getFromSpot;
                    flowContributionQ1[writeSpot + x] = (int)((1 - xpart) * (1 - ypart) * contributionMultiplier + 0.995);
                }
                else
                {
                    flowTemplateQ1[writeSpot + x] = 0;
                    flowContributionQ1[writeSpot + x] = 0;
                }

                // Q2 (upper right)
                if (fromx >= 0 && fromx < w - 1 && fromy >= 0 && fromy < h)
                {
                    flowTemplateQ2[writeSpot + x] = (int)getFromSpot + 1;
                    flowContributionQ2[writeSpot + x] = (int)((xpart) * (1 - ypart) * contributionMultiplier + 0.995);
                }
                else
                {
                    flowTemplateQ2[writeSpot + x] = 0;
                    flowContributionQ2[writeSpot + x] = 0;
                }

                // Q3 (lower left)
                if (fromx >= 0 && fromx < w && fromy >= 0 && fromy < h - 1)
                {
                    flowTemplateQ3[writeSpot + x] = (int)(getFromSpot + pitch);
                    flowContributionQ3[writeSpot + x] = (int)((1 - xpart) * (ypart) * contributionMultiplier + 0.995);
                }
                else
                {
                    flowTemplateQ3[writeSpot + x] = 0;
                    flowContributionQ3[writeSpot + x] = 0;
                }

                // Q4 (upper left)
                if (fromx >= 0 && fromx < w - 1 && fromy >= 0 && fromy < h - 1)
                {
                    flowTemplateQ4[writeSpot + x] = (int)(getFromSpot + pitch + 1);
                    flowContributionQ4[writeSpot + x] = (int)((xpart) * (ypart) * contributionMultiplier + 0.995);
                }
                else
                {
                    flowTemplateQ4[writeSpot + x] = 0;
                    flowContributionQ4[writeSpot + x] = 0;
                }

                return getFromSpot;
            }

            int y = 0;
            /// --------------------------------------------------------------------------
            /// <summary>
            /// Render
            /// </summary>
            /// --------------------------------------------------------------------------
            public override void Render()
            {
                if (IsDone) return;

                if (tempBuffer == null)
                {
                    tempBuffer = new ushort[dvWindow.MainBuffer.BufferPitch * dvWindow.MainBuffer.Height];
                }

                frame++;
                
                ushort[] data = dvWindow.MainBuffer.RawBuffer;
                int height = dvWindow.MainBuffer.Height;
                int width = dvWindow.MainBuffer.Width;
                int pitch = dvWindow.MainBuffer.BufferPitch;
                int size = height * pitch;

                int[,] rgbLookup = GlobalState.rgbLookup5bit;
                int red, green, blue, contribution, readSpot;
                ushort source;

                for (int i = 0; i < height; i++ )
                {
                    int writeSpot = y * pitch;
                    for (int x = 0; x < width; x++)
                    {

                        readSpot = flowTemplateQ1[writeSpot];
                        contribution = flowContributionQ1[writeSpot];
                        source = data[readSpot];
                        red = rgbLookup[source, 0] * contribution;
                        green = rgbLookup[source, 1] * contribution;
                        blue = rgbLookup[source, 2] * contribution;

                        readSpot = flowTemplateQ2[writeSpot];
                        contribution = flowContributionQ2[writeSpot];
                        source = data[readSpot];
                        red += rgbLookup[source, 0] * contribution;
                        green += rgbLookup[source, 1] * contribution;
                        blue += rgbLookup[source, 2] * contribution;

                        readSpot = flowTemplateQ3[writeSpot];
                        contribution = flowContributionQ4[writeSpot];
                        source = data[readSpot];
                        red += rgbLookup[source, 0] * contribution;
                        green += rgbLookup[source, 1] * contribution;
                        blue += rgbLookup[source, 2] * contribution;

                        readSpot = flowTemplateQ4[writeSpot];
                        contribution = flowContributionQ3[writeSpot];
                        source = data[readSpot];
                        red += rgbLookup[source, 0] * contribution;
                        green += rgbLookup[source, 1] * contribution;
                        blue += rgbLookup[source, 2] * contribution;


                        ushort newColor = (ushort)(((red >> 8) << 10) +
                            ((green >> 8) << 5) +
                            (blue >> 8));


                        tempBuffer[writeSpot++] = newColor;
                    }

                    y++;
                    if (y >= height) y = 0;
                }

                Array.Copy(tempBuffer, 0, data, 0, height * pitch);

            }
        }
    }
}
