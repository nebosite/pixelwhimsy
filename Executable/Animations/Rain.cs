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
        ///Draw Rain Animation
        /// </summary>
        /// --------------------------------------------------------------------------
        public class Rain : Animation
        {
            List<RainDrop> drops = new List<RainDrop>();
            double xm, ym;
            int maxDrops = 800;
            SoundPlayer.SoundInstance sound;

            public override bool IsDone
            {
                get
                {
                    return base.IsDone;
                }
                set
                {
                    dvWindow.OverlayBuffer.Clear(0);
                    base.IsDone = value;
                    sound.Finished = true;
                }
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Constructor
            /// </summary>
            /// --------------------------------------------------------------------------
            public Rain(DVWindow window)
                : base(window)
            {
                this.xm = DRand(1) - .5; 
                this.ym = 1;

                int w = dvWindow.MainBuffer.Width;
                int h = dvWindow.MainBuffer.Height;

                for (int i = 0; i < maxDrops; i++)
                {
                    drops.Add(new RainDrop(Rand(w), Rand(h), Rand(h) + h, this.xm, this.ym));
                }

                sound = MediaBag.Play(SoundID.Loop_Rain, 1, .01, true);
                MediaBag.Play(SoundID.Thunder);
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Render
            /// </summary>
            /// --------------------------------------------------------------------------
            public override void Render()
            {
                int w = dvWindow.MainBuffer.Width;
                int h = dvWindow.MainBuffer.Height;
                
                if (sound.Volume < 0.2) sound.Volume += .002;
                if (IsDone) return;
                for (int i = 0; i < drops.Count; i++)
                {
                    drops[i].Render(dvWindow, 15);

                    if (drops[i].done) drops[i] = new RainDrop(Rand(w), Rand(h), Rand(h) + h, this.xm, this.ym);
                }
            }

            /// <summary>
            /// Different animation modes for a drop
            /// </summary>
            enum DropMode
            {
                Falling,
                Splatting,
                Running
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Handler for one raindrop
            /// </summary>
            /// --------------------------------------------------------------------------
            public class RainDrop
            {
                double x, y;
                double width = 10;
                double targetx, targety;
                int lastx, lasty;
                double xm, ym;
                public bool done = false;
                DropMode mode = DropMode.Falling;

                /// --------------------------------------------------------------------------
                /// <summary>
                /// Constructor
                /// </summary>
                /// --------------------------------------------------------------------------
                public RainDrop(double targetx, double targety, double timeToReachTarget, double xm, double ym)
                {
                    this.x = (int)(targetx - xm * timeToReachTarget);
                    this.y = (int)(targety - ym * timeToReachTarget);
                    this.targety = targety;
                    this.targetx = targetx;
                    this.xm = xm;
                    this.ym = ym;
                }

                /// --------------------------------------------------------------------------
                /// <summary>
                /// Move the raindrop
                /// </summary>
                /// --------------------------------------------------------------------------
                public void Render(DVWindow dvWindow, double timeStep)
                {
                    switch(mode)
                    {
                        case DropMode.Falling: RenderFalling(dvWindow, timeStep); break;
                        case DropMode.Splatting: RenderSplat(dvWindow); break;
                        case DropMode.Running: RenderRun(dvWindow); break;
                        default: break;
                    }
                }

                /// --------------------------------------------------------------------------
                /// <summary>
                /// Draw raindrop-like lines to look like falling rain
                /// </summary>
                /// <param name="dvWindow"></param>
                /// <param name="timeStep"></param>
                /// --------------------------------------------------------------------------
                public void RenderFalling(DVWindow dvWindow, double timeStep)
                {
                    dvWindow.OverlayBuffer.DrawLine(0, (int)x, (int)y, lastx, lasty);

                    if (y > targety)
                    {
                        mode = DropMode.Splatting;
                        return;
                    }

                    lastx = (int)x;
                    lasty = (int)y;

                    x += xm * timeStep;
                    y += ym * timeStep;

                    dvWindow.OverlayBuffer.DrawLine(Color.Blue, (int)x, (int)y, lastx, lasty);
                }

                int splatFrame = 0;

                /// --------------------------------------------------------------------------
                /// <summary>
                /// Draw raindrop-like lines to look like falling rain
                /// </summary>
                /// <param name="dvWindow"></param>
                /// <param name="timeStep"></param>
                /// --------------------------------------------------------------------------
                public void RenderSplat(DVWindow dvWindow)
                {
                    Color splatColor = splatFrame == 0 ? Color.Blue : Color.Black;

                    for (int i = 0; i < 16; i++)
                    {
                        double theta = i * Math.PI / 8.0;
                        double sx = x + width/2.0 * Math.Cos(theta);
                        double sy = y + width/5.0 * Math.Sin(theta);

                        dvWindow.OverlayBuffer.DrawLine(splatColor, (int)sx, (int)sy, (int)sx, (int)sy);
                    }

                    splatFrame++;
                    if (splatFrame > 1) mode = DropMode.Running;
                }
                
                int runCount = 10;
                /// --------------------------------------------------------------------------
                /// <summary>
                /// Draw raindrop-like lines to look like falling rain
                /// </summary>
                /// <param name="dvWindow"></param>
                /// <param name="timeStep"></param>
                /// --------------------------------------------------------------------------
                public void RenderRun(DVWindow dvWindow)
                {
                    int pitch = dvWindow.MainBuffer.BufferPitch;
                    int w = dvWindow.MainBuffer.Width;
                    int halfw = (int)(this.width / 2);
                    int h = dvWindow.MainBuffer.Height; 
                    int speed = 99;
                    int[] contribution = new int[] { 100, 400, 100, 400, 600, 400 };

                    ushort[] data = dvWindow.MainBuffer.RawBuffer;
                    ushort[] output = new ushort[(int)(width)];

                    for (int count = 0; count < speed && runCount >= 0; count++)
                    {
                        int ry = (int)(targety + runCount);
                        if(ry >= h)
                        {
                            runCount--;
                            continue;
                        }
                        if (ry < 1) ry = 1;

                        int dropw = halfw * 2;
                        int startx = (int)(targetx) - halfw;
                        int endx = (int)(targetx) + halfw;

                        if (startx < 0)
                        {
                            dropw += startx;
                            startx = 0;
                        }

                        if (endx >= w)
                        {
                            dropw -= (endx - w);
                            endx = w - 1;
                        }

                        int dataSpot = ry * pitch + startx;
                        int writeSpot = dataSpot;

                        for (int i = 0; i < dropw; i++)
                        {
                            int stepx = (int)startx + i;

                            int topred = 0, topgreen = 0, topblue = 0;
                            int lowred = 0, lowgreen = 0, lowblue = 0;
                            int topTotal = contribution[1];
                            int lowTotal = contribution[4];

                            if (ry > 0)
                            {
                                if(stepx > 0)
                                {
                                    topred += GlobalState.rgbLookup5bit[data[dataSpot - pitch - 1], 0] * contribution[0];
                                    topgreen += GlobalState.rgbLookup5bit[data[dataSpot - pitch - 1], 1] * contribution[0];
                                    topblue += GlobalState.rgbLookup5bit[data[dataSpot - pitch - 1], 2] * contribution[0];
                                    topTotal += contribution[0];
                                }

                                topred += GlobalState.rgbLookup5bit[data[dataSpot - pitch], 0] * contribution[1];
                                topgreen += GlobalState.rgbLookup5bit[data[dataSpot - pitch], 1] * contribution[1];
                                topblue += GlobalState.rgbLookup5bit[data[dataSpot - pitch], 2] * contribution[1];

                                if(stepx < w-1)
                                {
                                    topred += GlobalState.rgbLookup5bit[data[dataSpot - pitch + 1], 0] * contribution[2];
                                    topgreen += GlobalState.rgbLookup5bit[data[dataSpot - pitch + 1], 1] * contribution[2];
                                    topblue += GlobalState.rgbLookup5bit[data[dataSpot - pitch + 1], 2] * contribution[2];
                                    topTotal += contribution[2];
                                }
                            }

                            if(stepx > 0)
                            {
                                lowred += GlobalState.rgbLookup5bit[data[dataSpot - 1], 0] * contribution[3];
                                lowgreen += GlobalState.rgbLookup5bit[data[dataSpot - 1], 1] * contribution[3];
                                lowblue += GlobalState.rgbLookup5bit[data[dataSpot - 1], 2] * contribution[3];
                                lowTotal += contribution[3];
                            }

                            lowred += GlobalState.rgbLookup5bit[data[dataSpot], 0] * contribution[4];
                            lowgreen += GlobalState.rgbLookup5bit[data[dataSpot], 1] * contribution[4];
                            lowblue += GlobalState.rgbLookup5bit[data[dataSpot], 2] * contribution[4];

                            if(stepx < w-1)
                            {
                                lowred += GlobalState.rgbLookup5bit[data[dataSpot + 1], 0] * contribution[5];
                                lowgreen += GlobalState.rgbLookup5bit[data[dataSpot + 1], 1] * contribution[5];
                                lowblue += GlobalState.rgbLookup5bit[data[dataSpot + 1], 2] * contribution[5];
                                lowTotal += contribution[5];
                            }

                            int red, green, blue;
                            red = (((topred * (11 - runCount)) / topTotal) + (lowred * (runCount + 1) / lowTotal)) / 12;
                            green = (((topgreen * (11 - runCount)) / topTotal) + (lowgreen * (runCount + 1) / lowTotal)) / 12;
                            blue = (((topblue * (11 - runCount)) / topTotal) + (lowblue * (runCount + 1) / lowTotal)) / 12;

                            output[i] = (ushort)((red << 10) + (green << 5) + blue);
                            dataSpot++;

                        }

                        Array.Copy(output, 0, data, writeSpot, dropw);
                        runCount--;
                    }
                        // Pick a row, blend with colors above

                    if (runCount < 0)
                    {
                        runCount = 10;
                        width--;
                        if (width < 3) done = true;
                    }
                }
            }
        }
    }
}
