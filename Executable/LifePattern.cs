using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using DirectVarmint;

namespace PixelWhimsy
{
    /// --------------------------------------------------------------------------
    /// <summary>
    /// A class that represents a single pattern in conways game of life
    /// </summary>
    /// --------------------------------------------------------------------------
    class LifePattern
    {
        List<Point> points = new List<Point>();
        public int width;
        public int height;

        public static List<LifePattern> GlobalPatterns = new List<LifePattern>();

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Static construction
        /// </summary>
        /// --------------------------------------------------------------------------
        static LifePattern()
        {
            string[] fileNames = 
               {
                "ak47.lif",
                "bhepto.lif",
                "bi-gun.lif",
                "bship.lif",
                "bship2.lif",
                "gun30.lif",
                "lwssgun.lif",
                "max.lif",
                "mwssrake.lif",
                "pi.lif",
                "psrtrain.lif",
                "puftrain.lif",
                "rake.lif",
                "relay.lif",
                "slopuf2.lif",
                "stretch.lif",
                "switchen.lif",
                "thingun2.lif",
                "thue.lif",
                "tiretrax.lif",
                "venetia2.lif",
                "wing.lif",
                "zip2.lif",
                "zips.lif", 
               }; 
     
            foreach(string name in fileNames)
            {
                GlobalPatterns.Add(new LifePattern(DVTools.GetStream("PixelWhimsy.lifedata." + name)));
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="lifeData"></param>
        /// --------------------------------------------------------------------------
        public LifePattern(Stream lifeData)
        {
            int x = 0;
            int y = 0;
            int startColumn = 0;
            int minx = int.MaxValue;
            int maxx = int.MinValue;
            int miny = int.MaxValue;
            int maxy = int.MinValue;

            StreamReader reader = new StreamReader(lifeData);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("#P"))
                {
                    string[] parts = line.Split(' ');
                    x = startColumn = int.Parse(parts[1]);
                    y = int.Parse(parts[2]);
                }

                if (line.StartsWith("*") || line.StartsWith("."))
                {
                    for (int i = 0; i < line.Length; i++)
                    {
                        if (line[i] == '*')
                        {
                            points.Add(new Point(x, y));
                            if (x < minx) minx = x;
                            if (y < miny) miny = y;
                            if (x > maxx) maxx = x;
                            if (y > maxy) maxy = y;
                        }
                        x++;
                    }
                    y++;
                    x = startColumn;
                }
            }

            width = (maxx - minx);
            height = (maxy - miny);

            for (int i = 0; i < points.Count; i++)
            {

                this.points[i] = new Point(points[i].X - minx, points[i].Y - miny);
            }

        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Draw this pattern as pixels on a buffer
        /// </summary>
        /// --------------------------------------------------------------------------
        public void Draw(PixelBuffer buffer, ushort color, int x, int y)
        {
            for (int i = 0; i < points.Count; i++)
            {
                Utilities.AnimateColor(ref color, (uint)i);
                buffer.DrawPixel(color,points[i].X + x,points[i].Y + y); 
            }
        }
    }
}
