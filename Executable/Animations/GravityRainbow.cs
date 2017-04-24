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
        ///Draw one little GravityRainbow to follow the cursor around
        /// </summary>
        /// --------------------------------------------------------------------------
        public class GravityRainbow : Animation
        {
            const int MAXDOTS = 2000;
            List<Dot> dots = new List<Dot>();
            SoundPlayer.SoundEffect soundEffect;
            SoundPlayer.SoundInstance sound;
            bool started = false;
            ushort color;
            double x, y;

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
            public GravityRainbow(DVWindow window, int x, int y, int mousex, int mousey, ushort color)
                : base(window)
            {

                this.mousex = mousex;
                this.mousey = mousey;
                this.color = color;
                this.x = x + DRand(.5);
                this.y = y + DRand(.5);

                soundEffect = new SoundPlayer.SoundEffect(MAXDOTS);

                sound = MediaBag.Player.Play(soundEffect);
                sound.Looping = true;
            }

            double averageDistance = 200;
            double totald = 0;

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Initialize the dots
            /// </summary>
            /// --------------------------------------------------------------------------
            void InitDots(int numDots)
            {
                double direction = Utilities.GetAngle(0, 1, x - MouseX, y - MouseY);
                double dx = MouseX - x;
                double dy = MouseY - y;
                double d = Math.Sqrt(dx * dx + dy * dy);
                double magnitude = Math.Sqrt(d)/20.0;

                for (int i = 0; i < numDots; i++)
                {
                    Utilities.AnimateColor(ref color, (uint)i);
                    double theta = direction + (i - numDots / 2) / (numDots * 100.0);
                    double xm = magnitude * Math.Cos(theta);
                    double ym = magnitude * Math.Sin(theta);
                    Dot newDot = new Dot(x, y, xm, ym, color);
                    dots.Add(newDot);
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

                if (!started)
                {
                    double dx = Math.Abs(MouseX - x);
                    double dy = Math.Abs(MouseY - y);
                    if (dx + dy > 20)
                    {
                        InitDots(MAXDOTS);
                        started = true;
                    }
                    else return;
                }

                List<Point> attractors = new List<Point>();
                attractors.Add(new Point(MouseX, MouseY));

                int i = 0;
                double lastx = dots[0].x;
                double lasty = dots[0].y;
                double theta1 = 0;
                double theta2 = 0;
                double delta1 = (Math.PI * 2) / dots.Count * 4570;
                double delta2 = (Math.PI * 2) / dots.Count * 4582;

                if (totald > 3000) totald = 3000;
                double maxSoundValue = totald - Math.Sqrt(averageDistance) * 100;
                if (maxSoundValue < 0) maxSoundValue = 0;
                totald = averageDistance = 0;
                int renderedDots = 0;

                foreach (Dot dot in dots)
                {
                    soundEffect.SoundData[i] = (short)(maxSoundValue * Math.Sin(theta1));
                    soundEffect.SoundData[i + 1] = (short)(maxSoundValue * Math.Sin(theta2));
                    if (dot.isAlive)
                    {
                        dot.Move(attractors);
                        dot.Render(dvWindow);
                        renderedDots++;
                    }
                    i += 2;
                    double dx = Math.Abs(dot.x - lastx) ;
                    double dy = Math.Abs(dot.y - lasty);
                    double d = dx + dy;
                    double moused = Math.Abs(dot.x - MouseX) + Math.Abs(dot.y - MouseY);
                    averageDistance += moused;
                    totald += d * 100;
                    lastx = dot.x;
                    lasty = dot.y;
                    theta1 += delta1 / (1 + dx * 400);
                    theta2 += delta2 / (1 + dy * 400);
                }
                averageDistance /= dots.Count;
                if (renderedDots < 250) this.IsDone = true;
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// A class for handling a single gravitating dot
            /// </summary>
            /// --------------------------------------------------------------------------
            new class Dot
            {
                static double G = 100.0;
                public double x, y, xm, ym, lastx, lasty;
                ushort color;
                public bool isAlive = true;

                int limit = 1000;

                /// --------------------------------------------------------------------------
                /// <summary>
                /// Constructor
                /// </summary>
                /// --------------------------------------------------------------------------
                public Dot(double x, double y, double xm, double ym, ushort color)
                {
                    this.x = x;
                    this.y = y;
                    this.xm = xm;
                    this.ym = ym;
                    this.color = color;
                }

                /// --------------------------------------------------------------------------
                /// <summary>
                /// Move
                /// </summary>
                /// --------------------------------------------------------------------------
                internal void Move(List<Point> attractors)
                {
                    double xf = 0;
                    double yf = 0;

                    foreach (Point p in attractors)
                    {
                        double xd = p.X - x;
                        double yd = p.Y - y;
                        double d2 = xd * xd + yd * yd;
                        double d = Math.Sqrt(d2);

                        double f = G / d2;
                        xf += f * xd / d;
                        yf += f * yd / d;
                    }

                    xm += xf;
                    ym += yf;

                    lastx = x;
                    lasty = y;
                    x += xm;
                    y += ym;
                }

                /// --------------------------------------------------------------------------
                /// <summary>
                /// Draw me
                /// </summary>
                /// --------------------------------------------------------------------------
                internal void Render(DVWindow dvWindow)
                {
                    if (x < -limit || x > limit || y < -limit || y > limit || double.IsNaN(x) || double.IsNaN(y)) isAlive = false;
                    dvWindow.MainBuffer.DrawLine(color, (int)lastx, (int)lasty, (int)x, (int)y);

                    //dvWindow.MainBuffer.DrawLine(color, (int)lastx + 1, (int)lasty, (int)x + 1, (int)y);
                    //dvWindow.MainBuffer.DrawLine(color, (int)lastx, (int)lasty + 1, (int)x, (int)y + 1);
                    //dvWindow.MainBuffer.DrawLine(color, (int)lastx - 1, (int)lasty, (int)x - 1, (int)y);
                    //dvWindow.MainBuffer.DrawLine(color, (int)lastx, (int)lasty + 1, (int)x, (int)y + 1);
                }
            }
        }
    }
}
