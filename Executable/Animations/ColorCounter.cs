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
        /// Show a count of the top MAXCOLORS screen colors
        /// </summary>
        /// --------------------------------------------------------------------------
        public class ColorCounter : Animation
        {
            const int MAXCOLORS = 6;

            int[] colorCount = new int[0x10000];
            int x;
            int y;
            int lastHeight;

            public override bool IsDone
            {
                get
                {
                    return base.IsDone;
                }
                set
                {
                    dvWindow.OverlayBuffer.DrawFilledRectangle(0, x, y, x + 60, y + lastHeight);
                    base.IsDone = value;
                }
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Constructor
            /// </summary>
            /// --------------------------------------------------------------------------
            public ColorCounter(DVWindow window, int x, int y)
                : base(window)
            {
                this.x = x;
                this.y = y;
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Render
            /// </summary>
            /// --------------------------------------------------------------------------
            public override void Render()
            {
                dvWindow.OverlayBuffer.DrawFilledRectangle(0, x, y, x + 60, y + lastHeight);
                if (IsDone) return;

                // Clear out the count
                for (int i = 0; i < 0x10000; i+=4)
                {
                    colorCount[i] = 0;
                    colorCount[i+1] = 0;
                    colorCount[i+2] = 0;
                    colorCount[i+3] = 0;
                }

                // Perform a raw Count
                ushort[] buffer = dvWindow.MainBuffer.RawBuffer;
                for (int liney = 0; liney < dvWindow.Height; liney++)
                {
                    int bufferSpot = liney * dvWindow.MainBuffer.BufferPitch;
                    for (int linex = 0; linex < dvWindow.Width; linex++)
                    {
                        colorCount[buffer[bufferSpot++]]++;
                    }
                }

                // Consolidate animated colors into a single representative color
                for (int i = 0x8000; i < 0x10000; i += 64)
                {
                    for (int j = i + 1; j < i + 64; j++)
                    {
                        colorCount[i] += colorCount[j];
                        colorCount[j] = 0;
                    }
                }

                // Ignore Dark colors
                int darkThreshhold = 8;
                for (int r = 0; r < darkThreshhold; r++)
                {
                    for (int g = 0; g < darkThreshhold; g++)
                    {
                        for (int b = 0; b < darkThreshhold; b++)
                        {
                            int color = (r << 10) + (g << 5) + b;
                            colorCount[color] = 0;
                        }
                    }
                }

                // Find highest Colors
                List<CountElement> topOnes = new List<CountElement>();
                for (int j = 0; j < MAXCOLORS + 1; j++) topOnes.Add(new CountElement(0, -1));
                for (int i = 1; i < 0x10000; i++)
                {
                    if (colorCount[i] != 0)
                    {
                        CountElement element = new CountElement((ushort)i, colorCount[i]);
                        for (int j = MAXCOLORS - 1; j >= 0; j--)
                        {
                            if (colorCount[i] > topOnes[j].Count)
                            {
                                topOnes[j + 1] = topOnes[j];
                                topOnes[j] = element;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
               

                TimeSpan span = DateTime.Now - start;
                int numColors = 0;
                for (int j = 0; j < MAXCOLORS; j++) if (topOnes[j].Color != 0) numColors++;


                int h = LocalHeight(numColors);
                dvWindow.OverlayBuffer.DrawFilledRectangle(MediaBag.color_DarkGray, x, y, x + 60, y + h);
                dvWindow.OverlayBuffer.DrawRectangle(MediaBag.color_White, x, y, x + 60, y + h);
                MediaBag.DrawMiniPic(dvWindow.OverlayBuffer, 53, x + 50, y + 2);


                lastHeight = h;

                for (int j = 0; j < numColors; j++)
                {
                    int cx = x + 2;
                    int cy = y + 8 + j * 12;
                    if (topOnes[j].Color != 0)
                    {
                        dvWindow.OverlayBuffer.DrawFilledRectangle(topOnes[j].Color, cx, cy, cx + 10, cy + 10);
                        dvWindow.OverlayBuffer.DrawRectangle(MediaBag.color_White, cx, cy, cx + 10, cy + 10);
                        dvWindow.OverlayBuffer.Print(MediaBag.color_White, MediaBag.font_Status, cx + 12, cy, topOnes[j].Count.ToString());
                    }
                }

                if (mousex > x && mousex < x + 60 && mousey > y && mousey < y + h)
                {
                    // Draw Mouse CUrsor
                    int mouseLeft = mousex - 2;
                    if (mouseLeft < x) mouseLeft = x;
                    int mouseRight = mousex + 2;
                    if (mouseRight > x + 60) mouseRight = x + 60;
                    int mouseTop = mousey - 2;
                    if (mouseTop < y) mouseTop = y;
                    int mouseBottom = mousey + 2;
                    if (mouseBottom > y + h) mouseBottom = y + h;

                    dvWindow.OverlayBuffer.DrawLine(MediaBag.color_White, mouseLeft, mousey, mouseLeft + 1, mousey);
                    dvWindow.OverlayBuffer.DrawLine(MediaBag.color_White, mouseRight, mousey, mouseRight - 1, mousey);
                    dvWindow.OverlayBuffer.DrawLine(MediaBag.color_White, mousex, mouseTop, mousex, mouseTop + 1);
                    dvWindow.OverlayBuffer.DrawLine(MediaBag.color_White, mousex, mouseBottom, mousex, mouseBottom - 1);
                }


            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Figure out the height of the box
            /// </summary>
            /// --------------------------------------------------------------------------
            public bool MouseClicked(int mx, int my)
            {
                if (mx >= x + 50 && mx <= x + 58 && my >= y + 2 && my <= y + 10)
                {
                    IsDone = true;
                    return true;
                }
                return false;
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Figure out the height of the box
            /// </summary>
            /// --------------------------------------------------------------------------
            private int LocalHeight(int numColors)
            {
                return numColors * 12 + 4 + 6;
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Helper struct
            /// </summary>
            /// --------------------------------------------------------------------------
            struct CountElement
            {
                public ushort Color;
                public int Count;
                public CountElement(ushort color, int count)
                {
                    this.Color = color;
                    this.Count = count;
                }
            }
        }
    }
}
