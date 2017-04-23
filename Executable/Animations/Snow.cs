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
        /// Fluttery snow that accumulates
        /// </summary>
        /// --------------------------------------------------------------------------
        public class Snow : Animation
        {
            List<Flake> flakes = new List<Flake>();

            static int heightStartFactor = -2;

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
            public Snow(DVWindow window, ushort color)
                : base(window)
            {
                for (int i = 0; i < 5000; i++)
                {
                    flakes.Add(new Flake(Rand(dvWindow.MainBuffer.Width), Rand(dvWindow.MainBuffer.Height) * heightStartFactor));
                }
                Flake.color = color;
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Render
            /// </summary>
            /// --------------------------------------------------------------------------
            public override void Render()
            {
                if (IsDone) return;
                Utilities.AnimateColor(ref Flake.color, (uint)(Flake.frame / 100));

                for (int counter = 0; counter < 40; counter++)
                {
                    Flake.frame++;
                    for (int i = 0; i < flakes.Count; i++)
                    {
                        Flake flake = flakes[i];

                        if (Flake.frame % flake.speed == 0)
                        {
                            if (flake.ShouldStick(dvWindow))
                            {
                                flake.Stick(dvWindow);
                                flakes[i] = new Flake(Rand(dvWindow.MainBuffer.Width), Rand(dvWindow.MainBuffer.Height) * heightStartFactor);
                                break;
                            }

                            flake.Move(dvWindow);
                        }
                    }
                }
                //double dx = x - mousex;
                //double dy = y - mousey;
                //double d = Math.Sqrt(x * x + y * y);

                //if (rand.Next(2) == 0)
                //{
                //    direction += (rand.Next(1000) - 500) / 1000.0;
                //}

                //xm = velocity * Math.Cos(direction);
                //ym = velocity * Math.Sin(direction);

                //double directionChange = Utilities.GetDirectionChange(xm, ym, dx, dy);

                //// Handle navigation
                //direction += directionChange * .2;

                //// Handle movement

                //x += xm;
                //y += ym;

                //Utilities.AnimateColor(ref color, (uint)(lifeTime));

                //dvWindow.OverlayBuffer.DrawPixel(0, (int)lastx, (int)lasty);
                //dvWindow.MainBuffer.DrawPixel(color, (int)lastx, (int)lasty);
                //dvWindow.OverlayBuffer.DrawPixel(Color.White, (int)x, (int)y);
                //lastx = x;
                //lasty = y;
                //lifeTime--;
                //if (lifeTime < 1)
                //{
                //    this.IsDone = true;
                //    dvWindow.OverlayBuffer.DrawPixel(0, (int)lastx, (int)lasty);
                //}
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// A Snow Flake
            /// </summary>
            /// --------------------------------------------------------------------------
            class Flake
            {
                int x, y, oldx, oldy, oldxm;
                public int speed;
                public static int frame = 0;
                public static ushort color;
                bool movedDown = true;

                /// --------------------------------------------------------------------------
                /// <summary>
                /// Examine the frame buffer and decide if the snow should stick
                /// </summary>
                /// --------------------------------------------------------------------------
                public bool ShouldStick(DVWindow dvWindow)
                {
                    if (y >= dvWindow.MainBuffer.Height - 1) return true;
                    if (x <= -1 || x >= dvWindow.MainBuffer.Width) return false;
                    if (!movedDown && Rand(10) == 0) return true;

                    ushort dotleft = dvWindow.MainBuffer.GetPixel(x - 1, y + 1);
                    ushort dotMiddle = dvWindow.MainBuffer.GetPixel(x, y + 1);
                    ushort dotRight = dvWindow.MainBuffer.GetPixel(x + 1, y + 1);

                    if (dotleft > 0 && dotMiddle > 0 && dotRight > 0) return true;

                    return false;
                }

                /// --------------------------------------------------------------------------
                /// <summary>
                /// Constructor
                /// </summary>
                /// --------------------------------------------------------------------------
                public Flake(int x, int y)
                {
                    this.x = x;
                    this.y = y;
                    this.speed = Rand(30) + 10;
                }

                /// --------------------------------------------------------------------------
                /// <summary>
                /// Draw the flake in the main buffer
                /// </summary>
                /// --------------------------------------------------------------------------
                internal void Stick(DVWindow dvWindow)
                {
                    dvWindow.OverlayBuffer.DrawPixel(0, (int)x, (int)y);
                    dvWindow.MainBuffer.DrawPixel(color, (int)x, (int)y);
                }

                /// --------------------------------------------------------------------------
                /// <summary>
                /// Move and redraw the flake in the overlay buffer
                /// </summary>
                /// --------------------------------------------------------------------------
                internal void Move(DVWindow dvWindow)
                {
                    movedDown = false;
                    int flutter = Rand(3) - 1;

                    if (dvWindow.MainBuffer.GetPixel(x + flutter, y) == 0)
                    {
                        x += flutter;
                    }

                    if (x >= 0 && x < dvWindow.MainBuffer.Width && dvWindow.MainBuffer.GetPixel(x, y + 1) == 0)
                    {
                        y++;
                        movedDown = true;
                    }

                    dvWindow.OverlayBuffer.DrawPixel(0, oldx, oldy);

                    oldxm = flutter;
                    oldx = x;
                    oldy = y;

                    dvWindow.OverlayBuffer.DrawPixel(MediaBag.color_White, x, y);
                }
            }
        }
    }
}
