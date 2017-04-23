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
        /// Generate a FloodFill
        /// </summary>
        /// --------------------------------------------------------------------------
        public class FloodFill : Animation
        {
            SoundPlayer.SoundInstance sound;
            Dictionary<int, Point> points = new Dictionary<int, Point>();
            List<int> keys = new List<int>();
            int[,] neighbors = new int[,] { { -1, 0 }, { 0, -1 }, { 1, 0 }, { 0, 1 } };
            ushort[] data;
            int pitch;
            int width;
            int height;
            ushort color;
            ushort colorToFill;
            ushort colorMask = (ushort)0xffff;
            int maxFillCount = 0;
            int pointsFilled = 0;

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
            public FloodFill(DVWindow window, ushort color, int x, int y)
                : base(window)
            {
                sound = MediaBag.Play(SoundID.Loop_Gargle, .6, .1, true);

                this.data = dvWindow.MainBuffer.RawBuffer;
                this.pitch = dvWindow.MainBuffer.BufferPitch;
                this.width = dvWindow.Width;
                this.height = dvWindow.Height;
                this.color = color;
                colorToFill = dvWindow.MainBuffer.GetPixel(x, y);

                if (colorToFill >= 0x8000) colorMask = (ushort)0xffc0;
                this.colorToFill = (ushort)(colorToFill & colorMask);
                maxFillCount = width * height * 2;
                if (x < 0 || y < 0 || x >= width || y >= height || ((color & colorMask) == colorToFill))
                {
                    IsDone = true;
                }
                else
                {
                    AddPoint(x, y);
                }
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Render
            /// </summary>
            /// --------------------------------------------------------------------------
            private void AddPoint(int x, int y)
            {
                int index = x + y * pitch;
                if(!points.ContainsKey(index))
                {
                    points[index] = new Point(x, y);
                    keys.Add(index);
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

                for (int i = 0; i < 300; i++)
                {
                    DoOne();
                }
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Render one fill pixel
            /// </summary>
            /// --------------------------------------------------------------------------
            private void DoOne()
            {
                if (keys.Count == 0 || pointsFilled > maxFillCount)
                {
                    IsDone = true;
                    return;
                }

                pointsFilled++;

                // pick a random point
                int keyIndex = Rand(keys.Count);
                int index = keys[keyIndex];
                Point p = points[index];
                points.Remove(index);
                keys.RemoveAt(keyIndex);

                // fill it  
                Utilities.AnimateColor(ref color, (uint)(index));
                data[index] = color;

                // Look at neighbors, add them if they can be filled
                for (int i = 0; i < 4; i++)
                {
                    int nx = p.X + neighbors[i, 0];
                    int ny = p.Y + neighbors[i, 1];

                    if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                    {
                        int newIndex = nx + ny * pitch;
                        ushort c = (ushort)(data[newIndex] & colorMask);
                        if (c == colorToFill)
                        {
                            AddPoint(nx, ny);
                        }
                    }
                }
            }
        }
    }
}
