using System;
using System.Collections.Generic;
using System.Text;
using DirectVarmint;
using System.Drawing;

namespace PixelWhimsy
{
    public enum FireworkType
    {
        Normal,
        TowerOfSparks,
        NormalWithCrackles,
        Spinner,
        PlanarBlast,
        //SwirlySparkles,
        //SkyM80,
        //NormalWithWistlers,
        //Barrage,
        //SkySpinner,
        //Test
        MaxCount
    }


    public abstract partial class Animation
    {

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Draw a phoosh-boom
        /// </summary>
        /// --------------------------------------------------------------------------
        public class Firework : Animation
        {
            public static int Speed = 3;
            ushort color;
            float x, y;
            List<Projectile> projectiles = new List<Projectile>();
            static float sizeFactor = 1;

            public enum SpreadType
            {
                Normal,
                PlanarBlast
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Constructor
            /// </summary>
            /// --------------------------------------------------------------------------
            public Firework(DVWindow window, FireworkType type, int x, int y, ushort color)
                : base(window)
            {
                this.x = x;
                this.y = y;
                this.color = color;
                sizeFactor = (dvWindow.Height / 250.0f);
                Projectile.Gravity = .002f * sizeFactor;
                float xm = (float)(DRand(.25) - .125);
                float ym = (float)(-(.8 + DRand(.2))) * sizeFactor;

                if (!smokeColorsSet)
                {
                    SetProjectileColors(window);
                }

                Projectile projectile = null;
                switch (type)
                {
                    //case FireworkType.Test: projectile = CreateTest(); MediaBag.Play(SoundID.Dot_Thump);  break;
                    case FireworkType.NormalWithCrackles: projectile = CreateTest(); MediaBag.Play(SoundID.Dot_Thump);  break;
                    case FireworkType.Spinner: projectile = CreateSpinner(); MediaBag.Play(SoundID.Firework_Fwoosh, 1.2 + DRand(.2)); break;
                    case FireworkType.PlanarBlast: projectile = CreatePlanarBlast(); MediaBag.Play(SoundID.Dot_Thump); break;
                    case FireworkType.TowerOfSparks: projectile = CreateTowerOfSparks(); MediaBag.Play(SoundID.Firework_Fwoosh, 1.2 + DRand(.2)); break;
                    case FireworkType.Normal: projectile = CreateNormal(); MediaBag.Play(SoundID.Dot_Thump); break;
                    default: break;
                }

                projectile.Set(x, y, xm, ym);
                projectiles.Add(projectile);
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Create a firework
            /// </summary>
            /// --------------------------------------------------------------------------
            private Projectile CreateTest()
            {
                List<Projectile> contents = new List<Projectile>();
                ushort[] colorCycle = CreateWhiteToColorCycle(Utilities.PickRandomColor(dvWindow, false));
                for (int i = 0; i < 40 + Rand(50); i++)
                {
                    Fuse thisFuse = Fuse.ColorFuse.Create(colorCycle, (GetRandLifeTime(20, 40)));
                    
                    List<Projectile> subContents = new List<Projectile>();
                    Fuse crackleFuse = Fuse.CrackleFuse.Create(Projectile.brightSparkColor, (GetRandLifeTime(1, 4)));
                    subContents.Add(new Projectile(dvWindow, projectiles, null, crackleFuse, .01f, 0));

                    contents.Add(new Projectile(dvWindow, projectiles, subContents, thisFuse, .01f, 0));
                }

                Projectile projectile = new Projectile(dvWindow, projectiles, contents, Fuse.StandardFuse.Create(GetRandLifeTime(10, 80)), 1, 1);
                return projectile;
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Create a firework
            /// </summary>
            /// --------------------------------------------------------------------------
            private Projectile CreateSpinner()
            {
                Projectile projectile = new Projectile(dvWindow, projectiles, null, Fuse.SpinnerFuse.Create(GetRandLifeTime(10, 80), Utilities.PickRandomColor(dvWindow,false)), 1, 0);
                return projectile;
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Create a firework
            /// </summary>
            /// --------------------------------------------------------------------------
            private Projectile CreateTowerOfSparks()
            {
                Projectile projectile = new Projectile(dvWindow, projectiles, null, Fuse.ShowerFuse.Create(GetRandLifeTime(10, 80)), 1, 0);
                return projectile;
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Create a normal mortar-type firework
            /// </summary>
            /// --------------------------------------------------------------------------
            private Projectile CreateNormal()
            {
                // Put together the contents
                List<Projectile> contents = new List<Projectile>();
                ushort[] colorCycle = CreateWhiteToColorCycle(Utilities.PickRandomColor(dvWindow, false));
                for (int i = 0; i < 350 + Rand(100); i++)
                {
                    Fuse thisFuse = Fuse.ColorFuse.Create(colorCycle, (GetRandLifeTime(20, 50)));
                    contents.Add(new Projectile(dvWindow, projectiles, null, thisFuse, .005f * sizeFactor, 0));
                }

                Projectile projectile = new Projectile(dvWindow, projectiles, contents, Fuse.StandardFuse.Create(GetRandLifeTime(10, 40)), 1, 2);
                return projectile;
            }
            /// --------------------------------------------------------------------------
            /// <summary>
            /// Create a normal mortar-type firework
            /// </summary>
            /// --------------------------------------------------------------------------
            private Projectile CreatePlanarBlast()
            {
                // Put together the contents
                List<Projectile> contents = new List<Projectile>();
                ushort[] colorCycle = CreateWhiteToColorCycle(Utilities.PickRandomColor(dvWindow, false));
                for (int i = 0; i < 350 + Rand(100); i++)
                {
                    Fuse thisFuse = Fuse.ColorFuse.Create(colorCycle, (GetRandLifeTime(20, 50)));
                    contents.Add(new Projectile(dvWindow, projectiles, null, thisFuse, .005f * sizeFactor, 0));
                }

                Projectile projectile = new Projectile(dvWindow, projectiles, contents, Fuse.StandardFuse.Create(GetRandLifeTime(10, 40)), 1, 2);
                projectile.SpreadType = SpreadType.PlanarBlast;
                return projectile;
            }

            #region UTILITIES
            /// --------------------------------------------------------------------------
            /// <summary>
            /// helper method to create random lifetimes
            /// </summary>
            /// --------------------------------------------------------------------------
            public static int GetRandLifeTime(int randPart, int basePart)
            {
                return (int)(Rand(randPart * Speed) + basePart * Speed * sizeFactor);
            }


            /// --------------------------------------------------------------------------
            /// <summary>
            /// Create a continuous set of color values that fade from white to the color
            /// </summary>
            /// <param name="p"></param>
            /// <returns></returns>
            /// --------------------------------------------------------------------------
            public static ushort[] CreateWhiteToColorCycle(ushort color)
            {
                ushort[] cycle = new ushort[70];
                int redTarget;
                int greenTarget;
                int blueTarget;
                int white = 0x1f;

                Utilities.GetColorTargets(color, out redTarget, out greenTarget, out blueTarget);

                // Quick fade from bright white to half white
                for (int i = 0; i < 10; i++)
                {
                    int newRed = (white * (20 - i)) / 20 + (redTarget * i) / 20;
                    int newGreen = (white * (20 - i)) / 20 + (greenTarget * i) / 20;
                    int newBlue = (white * (20 - i)) / 20 + (blueTarget * i) / 20;

                    cycle[i] = (ushort)((newRed << 10) + (newGreen << 5) + newBlue);
                }

                // Slow fade from half white to full color
                for (int i = 0; i < 50; i++)
                {
                    int newRed = (white * (50 - i)) / 100 + (redTarget * (i + 50)) / 100;
                    int newGreen = (white * (50 - i)) / 100 + (greenTarget * (i + 50)) / 100;
                    int newBlue = (white * (50 - i)) / 100 + (blueTarget * (i + 50)) / 100;

                    cycle[i + 10] = (ushort)((newRed << 10) + (newGreen << 5) + newBlue);
                }

                // fade the color at the end
                for (int i = 0; i < 10; i++)
                {
                    int newRed = (redTarget * (20 - i)) / 20;
                    int newGreen = (greenTarget * (20 - i)) / 20;
                    int newBlue = (blueTarget * (20 - i)) / 20;

                    cycle[i + 60] = (ushort)((newRed << 10) + (newGreen << 5) + newBlue);
                }

                return cycle;
            }

            bool smokeColorsSet = false;
            /// --------------------------------------------------------------------------
            /// <summary>
            /// Set up the spark colors
            /// </summary>
            /// --------------------------------------------------------------------------
            private void SetProjectileColors(DVWindow window)
            {
                Projectile.smokeColor = 0;// window.MainBuffer.GetPaletteColor(0x101010);
                Projectile.redColor = new ushort[32];
                for (int i = 0; i < 32; i++)
                {
                    int redPart = (i * 127) / 32 + 127;
                    Projectile.redColor[31-i] = window.MainBuffer.GetPaletteColor((uint)(redPart << 16));
                }

                ushort goldenColor = dvWindow.MainBuffer.GetPaletteColor(0xfff0d0);
                Projectile.brightSparkColor = CreateWhiteToColorCycle(goldenColor);
                smokeColorsSet = true;
            }
            #endregion

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Render
            /// </summary>
            /// --------------------------------------------------------------------------
            public override void Render()
            {
                if (IsDone) return;

                for (int speedCount = Speed; speedCount > 0; speedCount--)
                {
                    for (int i = 0; i < projectiles.Count; )
                    {
                        Projectile projectile = projectiles[i];
                        projectile.Move();
                        projectile.Render();
                        if (!projectile.alive)
                        {
                            projectiles.RemoveAt(i);
                        }
                        else
                        {
                            i++;
                        }
                    }
                }

                if (projectiles.Count <= 0) IsDone = true;
            }

            #region PROJECTILE
            /// --------------------------------------------------------------------------
            /// <summary>
            /// A packe is a recursive structure that represents a firework
            /// </summary>
            /// --------------------------------------------------------------------------
            public class Projectile
            {
                const float DragCoefficient = .0001f;
                public static float Gravity = .002f;

                public List<Projectile> projectileList = new List<Projectile>();
                public static ushort[] redColor;
                public static ushort[] brightSparkColor;
                public DVWindow dvWindow;
               
                float chargePower = 0;
                Fuse fuse;
                float mass;

                public float x=-1, y=-1;
                int lastx = -1, lasty = -1;
                public float xm = 0, ym = 0;
                public bool alive = true;
                internal static ushort smokeColor = 0;
                public SpreadType SpreadType = SpreadType.Normal;
                List<Projectile> contents = null;

                /// --------------------------------------------------------------------------
                /// <summary>
                /// Constructor
                /// </summary>
                /// --------------------------------------------------------------------------
                public Projectile(DVWindow dvWindow, List<Projectile> projectileList, List<Projectile> contents, Fuse fuse, float mass, float chargePower)
                {
                    this.dvWindow = dvWindow;
                    this.projectileList = projectileList;

                    this.mass = mass;
                    this.fuse = fuse;
                    this.contents = contents;
                    this.chargePower = chargePower;
                }

                /// --------------------------------------------------------------------------
                /// <summary>
                /// Move the spark according to laws of physics
                /// </summary>
                /// --------------------------------------------------------------------------
                public void Move()
                {
                    lastx = (int)x;
                    lasty = (int)y;

                    x += xm;
                    y += ym;

                    // drag
                    xm += -xm * DragCoefficient / mass;
                    ym += -ym * DragCoefficient / mass;

                    // gravity
                    ym += Gravity;

                    fuse.Burn(this);
                }

                /// --------------------------------------------------------------------------
                /// <summary>
                /// Draw the current projectile.  Explode the contents if the fuse is done
                /// </summary>
                /// --------------------------------------------------------------------------
                public void Render()
                {
                    if(fuse.color != 0) dvWindow.MainBuffer.DrawPixel(smokeColor, lastx, lasty);

                    if (fuse.IsDone)
                    {
                        alive = false;

                        if (contents != null)
                        {
                            double tilt = Utilities.DRand(Math.PI);
                            double sinTilt = Math.Sin(tilt);
                            double cosTilt = Math.Cos(tilt);
                            double c1 = Utilities.DRand(1);
                            double c2 = Utilities.DRand(1);
                            foreach (Projectile projectile in contents)
                            {
                                projectile.x = x;
                                projectile.y = y;
                                projectile.xm = xm;
                                projectile.ym = ym;

                                if (SpreadType == SpreadType.Normal)
                                {
                                    double theta = (DRand(2) - 1) * Math.PI;
                                    double rho = (DRand(2) - 1) * Math.PI;

                                    // add a little random movement
                                    double p = chargePower * Math.Cos(rho);
                                    projectile.xm += (float)(p * Math.Cos(theta));
                                    projectile.ym += (float)(p * Math.Sin(theta));
                                }
                                else if (SpreadType == SpreadType.PlanarBlast)
                                {
                                    double theta = DRand(Math.PI * 2);
                                    double sinTheta = Math.Sin(theta);
                                    double cosTheta = Math.Cos(theta);
                                    double X = (c1 * cosTheta * cosTilt - c2 * sinTheta * sinTilt);
                                    double Y = (c2 * sinTheta * cosTilt + c1 * cosTheta * sinTilt);

                                    double offset = DRand(3) + 1.4;
                                    double p = (1 - (1/Math.Pow(offset,3))) * chargePower;
                                    projectile.xm += (float)(p * X);
                                    projectile.ym += (float)(p * Y);
                                }
                                else
                                {

                                }

                                projectileList.Add(projectile);
                            }
                            MediaBag.Play(SoundID.Dot_Pow,1,chargePower);
                        }

                        return;
                    }

                    if (fuse.color != 0)
                    {
                        dvWindow.MainBuffer.DrawPixel(fuse.color, (int)x, (int)y);
                    }
                }

                /// --------------------------------------------------------------------------
                /// <summary>
                /// Set position and velocity
                /// </summary>
                /// --------------------------------------------------------------------------
                internal void Set(float x, float y, float xm, float ym)
                {
                    this.x = x;
                    this.y = y;
                    this.xm = xm;
                    this.ym = ym;
                }
            }
            #endregion

            #region FUSE
            /// --------------------------------------------------------------------------
            /// <summary>
            /// A fuse is the timer for a firework.  It also controls what a firework
            /// looks like.  The sparks in a large firework a just little fireworks with
            /// no charge, no payload, and a fuse that just draws a single dot.
            /// </summary>
            /// --------------------------------------------------------------------------
            public abstract class Fuse
            {
                #region Base Fuse code

                int lifeTime = -1;
                int maxLifeTime = 0;
                public ushort color;

                public bool IsDone { get { return lifeTime >= maxLifeTime; } }

                /// --------------------------------------------------------------------------
                /// <summary>
                /// Virtual draw method
                /// </summary>
                /// --------------------------------------------------------------------------
                public virtual void Burn(Projectile parent)
                {
                    lifeTime++;
                }
                #endregion

                #region ColorFuse
                /// --------------------------------------------------------------------------
                /// <summary>
                /// A single spark that follows a color path
                /// </summary>
                /// --------------------------------------------------------------------------
                public class ColorFuse : Fuse
                {
                    protected ushort[] colorCycle;

                    /// --------------------------------------------------------------------------
                    /// <summary>
                    /// Factory
                    /// </summary>
                    /// --------------------------------------------------------------------------
                    public static ColorFuse Create(ushort[] colorCycle, int maxLifeTime)
                    {
                        ColorFuse newFuse = new ColorFuse();
                        newFuse.maxLifeTime = maxLifeTime;
                        newFuse.colorCycle = colorCycle;
                        return newFuse;
                    }

                    /// --------------------------------------------------------------------------
                    /// <summary>
                    /// Factory
                    /// </summary>
                    /// --------------------------------------------------------------------------
                    public override void Burn(Projectile parent)
                    {
                        base.Burn(parent);
                        if (!IsDone)
                        {
                            ushort colorIndex = (ushort)((lifeTime * colorCycle.Length) / maxLifeTime);
                            this.color = colorCycle[colorIndex];
                        }
                    }
                }
                #endregion

                #region CrackleFuse
                /// --------------------------------------------------------------------------
                /// <summary>
                /// A single spark that follows a color path
                /// </summary>
                /// --------------------------------------------------------------------------
                public class CrackleFuse : ColorFuse
                {
                    bool cracked = false;

                    /// --------------------------------------------------------------------------
                    /// <summary>
                    /// Factory
                    /// </summary>
                    /// --------------------------------------------------------------------------
                    new public static CrackleFuse Create(ushort[] colorCycle, int maxLifeTime)
                    {
                        if (colorCycle == null)
                        {
                            return null;
                        }
                        CrackleFuse newFuse = new CrackleFuse();
                        newFuse.maxLifeTime = maxLifeTime;
                        newFuse.colorCycle = colorCycle;
                        return newFuse;
                    }

                    /// --------------------------------------------------------------------------
                    /// <summary>
                    /// Factory
                    /// </summary>
                    /// --------------------------------------------------------------------------
                    public override void Burn(Projectile parent)
                    {
                        base.Burn(parent);
                        if (!cracked)
                        {
                            cracked = true;
                            if(Rand(10)==0) MediaBag.Play(SoundID.Firework_Crack2,1,.1);
                        }
                    }
                }
                #endregion

                #region StandardFuse
                /// --------------------------------------------------------------------------
                /// <summary>
                /// Spits out short-lived red sparks
                /// </summary>
                /// --------------------------------------------------------------------------
                public class StandardFuse : Fuse
                {
                    /// --------------------------------------------------------------------------
                    /// <summary>
                    /// Factory
                    /// </summary>
                    /// --------------------------------------------------------------------------
                    public static StandardFuse Create(int maxLifeTime)
                    {
                        StandardFuse newFuse = new StandardFuse();
                        newFuse.maxLifeTime = maxLifeTime;
                        return newFuse;
                    }

                    /// --------------------------------------------------------------------------
                    /// <summary>
                    /// 
                    /// </summary>
                    /// <param name="projectileList"></param>
                    /// --------------------------------------------------------------------------
                    public override void Burn(Projectile parent)
                    {
                        base.Burn(parent);

                        if (Rand(Firework.Speed) == 0)
                        {
                            Projectile dingleBerry = new Projectile(
                                parent.dvWindow,
                                parent.projectileList,
                                null,
                                Fuse.ColorFuse.Create(Projectile.redColor, Firework.GetRandLifeTime(2, 4)),
                                1 / 10000.0f,
                                0);

                            // add a little random movement
                            double theta = (DRand(2) - 1) * Math.PI;
                            dingleBerry.Set(
                                parent.x,
                                parent.y,
                                parent.xm + (float)(1.0 * Math.Cos(theta)),
                                parent.ym + (float)(1.0 * Math.Sin(theta)));
                            parent.projectileList.Add(dingleBerry);
                        }
                    }
                }
                #endregion

                #region ShowerFuse
                /// --------------------------------------------------------------------------
                /// <summary>
                /// Spits out lots of medium-life white sparks
                /// </summary>
                /// --------------------------------------------------------------------------
                public class ShowerFuse : Fuse
                {
                    /// --------------------------------------------------------------------------
                    /// <summary>
                    /// Factory
                    /// </summary>
                    /// --------------------------------------------------------------------------
                    public static Fuse Create(int maxLifeTime)
                    {
                        ShowerFuse newFuse = new ShowerFuse();
                        newFuse.maxLifeTime = maxLifeTime;
                        return newFuse;
                    }

                    /// --------------------------------------------------------------------------
                    /// <summary>
                    /// 
                    /// </summary>
                    /// <param name="projectileList"></param>
                    /// --------------------------------------------------------------------------
                    public override void Burn(Projectile parent)
                    {
                        base.Burn(parent);

                        for (int i = 0; i < 1; i++)
                        {
                            Projectile spark = new Projectile(
                                parent.dvWindow,
                                parent.projectileList,
                                null,
                                Fuse.ColorFuse.Create(Projectile.brightSparkColor, Firework.GetRandLifeTime(30, 80)),
                                1 / 100.0f,
                                0);

                            // add a little random movement
                            double theta = (DRand(2) - 1) * Math.PI;
                            spark.Set(
                                parent.x,
                                parent.y,
                                parent.xm + (float)(.05 * Math.Cos(theta)),
                                parent.ym + (float)(.05 * Math.Sin(theta)));

                            parent.projectileList.Add(spark);
                        }
                    }
                }
                #endregion
                #region SpinnerFuse
                /// --------------------------------------------------------------------------
                /// <summary>
                /// Spits out lots of medium-life colored sparks in a spinning pattern
                /// </summary>
                /// --------------------------------------------------------------------------
                public class SpinnerFuse : Fuse
                {
                    ushort[] sparkColor;
                    double theta;
                    double spinnerSpeed = 0.01;
                    /// --------------------------------------------------------------------------
                    /// <summary>
                    /// Factory
                    /// </summary>
                    /// --------------------------------------------------------------------------
                    public static Fuse Create(int maxLifeTime, ushort seedColor)
                    {
                        SpinnerFuse newFuse = new SpinnerFuse();
                        newFuse.maxLifeTime = maxLifeTime;
                        newFuse.sparkColor = CreateWhiteToColorCycle(seedColor);
                        newFuse.theta = Utilities.DRand(Math.PI * 2);
                        newFuse.spinnerSpeed = Utilities.DRand(0.2) + 0.1;
                        if (Utilities.Rand(100) < 50) newFuse.spinnerSpeed = -newFuse.spinnerSpeed;
                        return newFuse;
                    }

                    /// --------------------------------------------------------------------------
                    /// <summary>
                    /// 
                    /// </summary>
                    /// <param name="projectileList"></param>
                    /// --------------------------------------------------------------------------
                    public override void Burn(Projectile parent)
                    {
                        base.Burn(parent);

                        for (int i = 0; i < 5; i++)
                        {
                            Projectile spark = new Projectile(
                                parent.dvWindow,
                                parent.projectileList,
                                null,
                                Fuse.ColorFuse.Create(sparkColor, Firework.GetRandLifeTime(15, 20)),
                                1 / 100.0f,
                                0);

                            // add a spin component
                            double power = Utilities.DRand(.3) + 0.1;
                            spark.Set(
                                parent.x,
                                parent.y,
                                parent.xm + (float)(power * Math.Cos(theta)),
                                parent.ym + (float)(power * Math.Sin(theta)));

                            parent.projectileList.Add(spark);
                        }
                        theta += spinnerSpeed;
                    }
                }
                #endregion
            }
            #endregion
        }
    }
}
