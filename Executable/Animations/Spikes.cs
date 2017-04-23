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
        /// Draw spikes around the edge of the screen
        /// </summary>
        /// --------------------------------------------------------------------------
        public class Spikes : Animation
        {
            Mode mode = Mode.Top;
            double x;
            double y;
            int speed = 1;
            int frame = 0;
            int minWidth;
            int spikeWidth;
            int spikeLength; 
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
            public Spikes(DVWindow window, ushort color, int size)
                : base(window)
            {
                this.x = 0;
                this.y = 0;
                minWidth = size / 2 + 1;
                spikeWidth = size / 2 + 1;
                spikeLength = GlobalState.resolutionY / 10 + Utilities.Rand(GlobalState.resolutionY / 6);
                this.mode = Mode.Top;
                this.color = color;
                this.speed = 30 - size / 2;
                if (speed < 1) speed = 1;
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Render
            /// </summary>
            /// --------------------------------------------------------------------------
            public override void Render()
            {
                if (IsDone) return;

                MediaBag.Play(SoundID.Spike, spikeWidth / 50.0 + .5, .1);
                for (int i = 0; i < speed; i++)
                {
                    RenderSpike();
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
            void RenderSpike()
            {
                this.frame++;
                AnimateColor(ref color, (uint)frame);
                ushort c = color;

                int width = minWidth + Utilities.Rand(spikeWidth);
                if (width < 2) width = 2;
                double workingWidth = width;
                double length = 5 + Utilities.Rand(spikeLength);
                double deltaw = width / (double)length;
                

                switch (mode)
                {
                    case Mode.Top:
                        for (int i = 0; i < length; i++)
                        {
                            dvWindow.MainBuffer.DrawLine(c, (int)(x - workingWidth/2), (int)(y+i), (int)(x + workingWidth/2), (int)(y+i));
                            workingWidth -= deltaw;
                        }
                        x+=width * .9;
                        if (x >= dvWindow.MainBuffer.Width - 1)
                        {
                            x = dvWindow.MainBuffer.Width - 1;
                            mode = Mode.Right;
                        }
                        break;
                    case Mode.Right:
                        for (int i = 0; i < length; i++)
                        {
                            dvWindow.MainBuffer.DrawLine(c, (int)(x - i), (int)(y - workingWidth / 2), (int)(x - i), (int)(y + workingWidth / 2));
                            workingWidth -= deltaw;
                        }
                        y += width * .9;
                        if (y >= dvWindow.MainBuffer.Height - 1)
                        {
                            y = dvWindow.MainBuffer.Height - 1;
                            mode = Mode.Bottom;
                        }
                        break;
                    case Mode.Bottom:
                        for (int i = 0; i < length; i++)
                        {
                            dvWindow.MainBuffer.DrawLine(c, (int)(x - workingWidth / 2), (int)(y - i), (int)(x + workingWidth / 2), (int)(y - i));
                            workingWidth -= deltaw;
                        }
                        x -= width * .9;
                        if (x <= 0)
                        {
                            x = 0;
                            mode = Mode.Left;
                        }
                        break;
                    case Mode.Left:
                        for (int i = 0; i < length; i++)
                        {
                            dvWindow.MainBuffer.DrawLine(c, (int)(x + i), (int)(y - workingWidth / 2), (int)(x + i), (int)(y + workingWidth / 2));
                            workingWidth -= deltaw;
                        }
                        y -= width * .9;
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
