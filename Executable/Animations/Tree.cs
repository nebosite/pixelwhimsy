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
        /// Grow a tree
        /// </summary>
        /// --------------------------------------------------------------------------
        public class Tree : Animation
        {
            SoundPlayer.SoundInstance sound;
            List<TreeBranch> branches = new List<TreeBranch>();

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
            public Tree(DVWindow window, int x, int size, int color)
                : base(window)
            {
                sound = MediaBag.Play(SoundID.Loop_Hiss, 1, 0.02, true);

                if (color >= 0)
                {
                    TreeBranch.userColor = true;
                    TreeBranch.color = (ushort)color;
                }
                else
                {
                    TreeBranch.userColor = false;
                }

                size = Math.Abs(size);
                branches.Add(new TreeBranch(size/40.0 + 5, x, dvWindow.Height, Math.PI * 1.5 + DRand(.1) - .05));
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Render
            /// </summary>
            /// --------------------------------------------------------------------------
            public override void Render()
            {
                if (IsDone) return;

                List<TreeBranch> buds = new List<TreeBranch>();

                for(int i = 0; i < branches.Count; i++)
                {
                    branches[i].Grow(dvWindow, buds);
                    if (branches[i].IsDone) branches.RemoveAt(i);
                }

                branches.AddRange(buds);

                if (branches.Count == 0) this.IsDone = true;
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// A single tree branch
            /// </summary>
            /// --------------------------------------------------------------------------
            public class TreeBranch
            {
                public bool IsDone = false;
                double growAngle;
                double x, y;
                double leftToGrow;
                double totalGrow;
                double size;
                int branches = 6;
                public static bool userColor = false;
                public static ushort color = 0;
                static int leafCounter;

                /// --------------------------------------------------------------------------
                /// <summary>
                /// Constructor
                /// </summary>
                /// --------------------------------------------------------------------------
                public TreeBranch(double size, double x, double y, double growAngle)
                {
                    this.size = size;
                    this.x = x;
                    this.y = y;
                    this.growAngle = growAngle;
                    this.totalGrow = this.leftToGrow = this.size * 11;

                }

                /// --------------------------------------------------------------------------
                /// <summary>
                /// grow me!
                /// </summary>
                /// --------------------------------------------------------------------------
                internal void Grow(DVWindow dvWindow, List<TreeBranch> buds)
                {
                    // Grow a little bit
                    double x2 = x + (totalGrow - leftToGrow) * Math.Cos(growAngle);
                    double y2 = y + (totalGrow - leftToGrow) * Math.Sin(growAngle);

                    double step = totalGrow / 10;
                    if (step > 3) step = 3;

                    if (leftToGrow > 0)
                    {
                        double tilt = growAngle - Math.PI / 2;
                        for (double bw = -size / 2; bw <= size / 2; bw += 1)
                        {
                            double brite = Noise.PerlinNoise_3D((bw + x)/3.0, (y + leftToGrow)/3.0, size);
                            if (brite < 0) brite = 0;
                            if (brite > 1) brite = 1;
                            int c = (int)(40 + 60 * brite + 30 * Math.Sin(bw / (size/2) * Math.PI));
                            double xx = x2 + bw * Math.Cos(tilt);
                            double yy = y2 + bw * Math.Sin(tilt);
                            //dvWindow.MainBuffer.DrawFilledCircle(Color.FromArgb(c,c/4,c/4), (int)xx, (int)yy, 1);
                            dvWindow.MainBuffer.DrawFilledRectangle(Color.FromArgb(c, c / 4, c / 4), (int)(xx - step/2), (int)(yy-step/2), (int)(xx + step/2), (int)(yy + step/2));
                        }
                    }

                    leftToGrow -= step;
                    if(leftToGrow < 0) leftToGrow = 0;

                    // Curve "up"
                    double angleMove = ((growAngle + 360.0) % 360.0) - (Math.PI * 1.5);
                    growAngle -= angleMove / 100.0;

                    // Branch
                    if (Rand(20) == 0 || leftToGrow == 0 )
                    {
                        double newSize = size * (.5 + DRand(.2));
                        if (newSize > 1.0 && branches > 0)
                        {
                            branches--;
                            double newGrowAngle = growAngle + (DRand(2) - 1.0) * 1.0;
                            buds.Add(new TreeBranch(newSize, x2, y2, newGrowAngle));
                        }
                        else
                        {
                            branches = 0;
                        }

                    }

                    // Terminate
                    if (branches <= 0)
                    {
                        if (size < 2)
                        {
                            ushort leafColor = dvWindow.MainBuffer.GetPaletteColor( (uint)((Rand(100) + 55) << 8) );
                            if (userColor)
                            {
                                leafColor = color;
                            }

                            Utilities.AnimateColor(ref leafColor, (uint)(leafCounter++));

                            dvWindow.MainBuffer.DrawFilledCircle(leafColor, (int)x2 + Rand(8) - 4, (int)y2 + Rand(8) - 4, Rand(3) + 1);
                            dvWindow.MainBuffer.DrawFilledCircle(leafColor, (int)x2 + Rand(8) - 4, (int)y2 + Rand(8) - 4, Rand(3) + 1);
                            dvWindow.MainBuffer.DrawFilledCircle(leafColor, (int)x2 + Rand(8) - 4, (int)y2 + Rand(8) - 4, Rand(3) + 1);
                        }
                        this.IsDone = true;
                    }
                }
            }
        }
    }
}
