using System;
using System.Collections.Generic;
using System.Text;
using DirectVarmint;
using System.Drawing;
using System.IO;

namespace PixelWhimsy
{
    public abstract partial class Animation
    {
        /// --------------------------------------------------------------------------
        /// <summary>
        /// Show instructions
        /// </summary>
        /// --------------------------------------------------------------------------
        public class Instructions : Animation
        {
            List<string> instructionText;

            public override bool IsDone
            {
                get
                {
                    return base.IsDone;
                }
                set
                {
                    base.IsDone = value;
                }
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Constructor
            /// </summary>
            /// --------------------------------------------------------------------------
            public Instructions(DVWindow window)
                : base(window)
            {
                StreamReader reader = new StreamReader(DVTools.GetStream("PixelWhimsy.OtherData.InGameInstructions.txt"));
                
                instructionText = new List<string>();
                string line;
                while((line = reader.ReadLine()) != null)
                {
                    instructionText.Add(line);    
                }
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Render
            /// </summary>
            /// --------------------------------------------------------------------------
            public override void Render()
            {
                dvWindow.OverlayBuffer.Clear(0);
                if (IsDone) return;

                dvWindow.OverlayBuffer.DrawLine(MediaBag.color_Gray, 13, 0, 13, dvWindow.Height);
                MediaBag.DrawMiniPic(dvWindow.OverlayBuffer, 50, 3, (int)(mousey * .9));

                float slider = (float) mousey / dvWindow.MainBuffer.Height;
                if (slider < 0) slider = 0;
                if (slider > .99999) slider = .99999f;

                int ySize = (int)(instructionText.Count * 25) - 250;
                int startLine = (int)(ySize * slider);

                for (int c = 0; c < instructionText.Count; c++)
                {
                    int y = c * 25 + 20 - startLine;
                    if (y < -25 || y > dvWindow.Height) continue;
                    string[] parts = instructionText[c].Split('\t');

                    if (parts[0].Length > 0)
                    {
                        DrawKey(30, y, parts[0]);
                    }

                    for (int xx = -1; xx < 2; xx++)
                    {
                        for (int yy = -1; yy < 2; yy++)
                        {
                            dvWindow.OverlayBuffer.Print(
                                MediaBag.color_Blue,
                                MediaBag.font_Instructions,
                                110 + xx,
                                y + 3 + yy,
                                parts[1]);

                        }
                    }
                    dvWindow.OverlayBuffer.Print(
                        MediaBag.color_White,
                        MediaBag.font_Instructions,
                        110,
                        y + 3,
                        parts[1]);
                }
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Draw a simulated key
            /// </summary>
            /// --------------------------------------------------------------------------
            private void DrawKey(int x, int y, string keyText)
            {
                SizeF textSize = MediaBag.font_Keys.Measure(keyText);
                textSize.Width *= 1.05f;
                dvWindow.OverlayBuffer.DrawFilledRectangle(Color.DimGray, x, y, (int)(x + textSize.Width + 8), (int)(y + textSize.Height) + 2);
                dvWindow.OverlayBuffer.DrawRectangle(Color.White, x, y, (int)(x + textSize.Width + 8), (int)(y + textSize.Height) + 2);
                dvWindow.OverlayBuffer.Print(MediaBag.color_White, MediaBag.font_Keys, x + 3, y + 2, keyText);
            }
        }
    }
}
