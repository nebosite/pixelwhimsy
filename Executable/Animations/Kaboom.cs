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
        /// Big huge explosion
        /// </summary>
        /// --------------------------------------------------------------------------
        public class Kaboom : Animation
        {
            List<Shrapnel> shrapnel = new List<Shrapnel>();

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
                }
            }
            /// --------------------------------------------------------------------------
            /// <summary>
            /// Constructor
            /// </summary>
            /// --------------------------------------------------------------------------
            public Kaboom(DVWindow window, ushort color, int x, int y, int power)
                : base(window)
            {
                int numShards = (20 + power * 20);
                for (int i = 0; i < numShards; i++)
                {
                    shrapnel.Add(new Shrapnel(color, x, y, power));
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

                for (int counter = 0; counter < 4; counter++)
                {
                    Shrapnel.frame++;
                    for (int i = 0; i < shrapnel.Count; )
                    {
                        Shrapnel shard = shrapnel[i];

                        shard.Move(dvWindow);
                        if (!shard.inScreen)
                        {
                            shrapnel.RemoveAt(i);
                            continue;
                        }

                        i++;
                    }
                }
                if (shrapnel.Count == 0) IsDone = true;
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// A Kaboom Flake
            /// </summary>
            /// --------------------------------------------------------------------------
            class Shrapnel
            {
                double x, y, xm, ym, rm, theta;
                public static int frame = 0;
                ushort color;
                double size;
                public bool inScreen = true;
                int x1, y1, x2, y2;

                /// --------------------------------------------------------------------------
                /// <summary>
                /// Constructor
                /// </summary>
                /// --------------------------------------------------------------------------
                public Shrapnel(ushort color, int x, int y, int power)
                {
                    this.x = x;
                    this.y = y;
                    this.color = color;

                    double rho = DRand(2) * Math.PI;
                    double v = DRand(power/5.0) + .2;
                    //v /= 10;
                    this.xm = v * Math.Cos(rho);
                    this.ym = v * Math.Sin(rho);
                    this.rm = DRand(power / 20.0) - (power / 40.0);
                    this.size = DRand(3) + .2;
                    this.theta = DRand(2) * Math.PI;
                }

                /// --------------------------------------------------------------------------
                /// <summary>
                /// Move and redraw the flake in the overlay buffer
                /// </summary>
                /// --------------------------------------------------------------------------
                internal void Move(DVWindow dvWindow)
                {
                    int w = dvWindow.MainBuffer.Width;
                    int h = dvWindow.MainBuffer.Height;
                    dvWindow.OverlayBuffer.DrawLine(0, x1, y1, x2, y2);

                    x += xm;
                    y += ym;
                    theta += rm;

                    x1 = (int)(x + size * Math.Cos(theta));
                    y1 = (int)(y + size * Math.Sin(theta));
                    x2 = (int)(x - size * Math.Cos(theta));
                    y2 = (int)(y - size * Math.Sin(theta));

                    AnimateColor(ref color, (uint)frame);
                    dvWindow.MainBuffer.DrawLine(color, x1, y1, x2, y2);
                    dvWindow.OverlayBuffer.DrawLine(MediaBag.color_White, x1, y1, x2, y2);

                    if ((x1 < 0 && x2 < 0) ||
                        (x1 > w && x2 > w) ||
                        (y1 < 0 && y2 < 0) ||
                        (y1 > h && y2 > h))
                    {
                        inScreen = false;
                    }
                }
            }
        }
    }
}
