using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms.VisualStyles;
using System.Drawing.Text;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Collections;

namespace DirectVarmint
{
    public partial class PixelBuffer
    {
        /// --------------------------------------------------------------------------
        /// <summary>
        /// Holds font data for drawing text on pixelBuffers.  
        /// </summary>
        /// --------------------------------------------------------------------------
        [Serializable]
        public class DVFont
        {
            Dictionary<char, CharData>  characters = new Dictionary<char, CharData>();
            ABC[] abcData = null;

            [NonSerialized] Graphics fontGraphics = null;
            [NonSerialized] Font internalFont = null;
            [NonSerialized] SolidBrush fontBrush = null;
            [NonSerialized] Bitmap scratchBuffer = null;
            [NonSerialized] int[] tempBuffer = null;
            [NonSerialized] int bitmapPitch = 0;

            float       pointSize;
            int         padding = 0;
            int         size = 0;
            int         height;
            string      fontFamily;
            FontStyle fontStyle;
            bool antiAlias;

            public int Height { get { return this.height; } }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Constructor for serialization
            /// </summary>
            /// --------------------------------------------------------------------------
            private DVFont() { } 

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Constructor
            /// </summary>
            /// --------------------------------------------------------------------------
            public DVFont(string fontFamily, float pointSize, FontStyle fontStyle, bool antiAlias)
            {
                this.fontFamily = fontFamily;
                this.fontStyle = fontStyle;
                this.antiAlias = antiAlias;
                this.pointSize = pointSize;
                
                size = (int)(pointSize + 1) * 2;
                padding = size / 8;

                Init();
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Initialize buffers for use in rendering characters
            /// </summary>
            /// --------------------------------------------------------------------------
            private void Init()
            {
                internalFont = new Font(fontFamily, pointSize, fontStyle);
                fontBrush = new SolidBrush(Color.White);

                CreateFontScratchSpace();
                height = (int)fontGraphics.MeasureString("ABC", internalFont).Height;
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Create an object for rendering font stuff
            /// </summary>
            /// --------------------------------------------------------------------------
            private void CreateFontScratchSpace()
            {
                scratchBuffer = new Bitmap(size, size, PixelFormat.Format32bppRgb);
                BitmapData data = scratchBuffer.LockBits(new Rectangle(0, 0, scratchBuffer.Width, scratchBuffer.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                bitmapPitch = data.Stride / 4;
                tempBuffer = new int[bitmapPitch * scratchBuffer.Height];
                scratchBuffer.UnlockBits(data);

                fontGraphics = Graphics.FromImage(scratchBuffer);
                fontGraphics.TextRenderingHint = antiAlias ? TextRenderingHint.AntiAliasGridFit : TextRenderingHint.SingleBitPerPixelGridFit;
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Measure the size of the font
            /// </summary>
            /// <param name="text">text to measure</param>
            /// <returns>Size of the string on the screen</returns>
            /// --------------------------------------------------------------------------
            public SizeF Measure(string text)
            {
                return this.fontGraphics.MeasureString(text, this.internalFont);
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Index accessor to get at character data
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            /// --------------------------------------------------------------------------
            public CharData this[char index]
            {
                get
                {
                    if (!characters.ContainsKey(index))
                    {
                        characters[index] = CreateCharacter(index);
                    }

                    return characters[index];
                }
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// 
            /// </summary>
            /// <param name="character"></param>
            /// <returns></returns>
            /// --------------------------------------------------------------------------
            private ABC GetCharABC(char character)
            {
                if (abcData == null)
                {
                    abcData = new ABC[256];
                    for (char i = (char)0; i < (char)256; i++)
                    {
                        abcData[i] = new ABC(0, (int)pointSize, 0);
                    }

                    IntPtr hdc = IntPtr.Zero;
                    IntPtr previosObject = IntPtr.Zero;
                    try
                    {
                        hdc = fontGraphics.GetHdc();
                        previosObject = User32.SelectObject(hdc, internalFont.ToHfont());
                        User32.GetCharABCWidths(hdc, (uint)0, (uint)255, abcData);
                        //for (char i = (char)0; i < (char)256; i++)
                        //{
                        //    characters[i] = abcData[i];
                        //}
                    }
                    catch (Exception)
                    {
                        // Ignore exceptions
                    }
                    finally
                    {
                        if (hdc != IntPtr.Zero)
                        {
                            if (previosObject != IntPtr.Zero) User32.SelectObject(hdc, previosObject);
                            fontGraphics.ReleaseHdc(hdc);
                        }
                    }
                }

                if (character < (char)256) return abcData[character];
                else return new ABC(0, (int)pointSize, 0);
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Generate the data needed to render a character in this font
            /// </summary>
            /// <param name="character"></param>
            /// <returns></returns>
            /// --------------------------------------------------------------------------
            private CharData CreateCharacter(char character)
            {

                int lowestX = int.MaxValue;
                int highestX = 0;
                int lowestY = int.MaxValue;
                int highestY = 0;

                fontGraphics.Clear(Color.Black);
                fontGraphics.DrawString(character.ToString(), internalFont, fontBrush, padding, padding);

                BitmapData data = scratchBuffer.LockBits(new Rectangle(0, 0, scratchBuffer.Width, scratchBuffer.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                Marshal.Copy(data.Scan0, tempBuffer, 0, bitmapPitch * data.Height);
                scratchBuffer.UnlockBits(data);

                for (int i = 0; i < tempBuffer.Length; i++) tempBuffer[i] &= 0xff;

                // figure out the size of character (to minimize storage)
                for (int x = 0; x < bitmapPitch; x++)
                {
                    for (int y = 0; y < size; y++)
                    {
                        if (tempBuffer[x + y * bitmapPitch] != 0)
                        {
                            if (x < lowestX) lowestX = x;
                            if (x > highestX) highestX = x;
                            if (y < lowestY) lowestY = y;
                            if (y > highestY) highestY = y;
                        }
                    }
                }

                if (lowestX == int.MaxValue)
                {
                    lowestX = lowestY = 0;
                }

                // Determine character attributes
                int pixelWidth = (highestX - lowestX) + 1;
                int pixelHeight = (highestY - lowestY) + 1;
                int[] charBits = new int[pixelWidth * pixelHeight];
                int xOffset = lowestX - padding;
                int yOffset = lowestY - padding;

                // Store the character bits
                for (int x = 0; x < pixelWidth; x++)
                {
                    for (int y = 0; y < pixelHeight; y++)
                    {
                        int px = (x + lowestX);
                        int py = (y + lowestY);
                        int pixelValue = tempBuffer[px + py * bitmapPitch];
                        charBits[x + y * pixelWidth] = pixelValue;
                    }
                }

                ABC abc = GetCharABC(character);
                int width = (int)(abc.abcA + abc.abcB + abc.abcC) - 1;

                return new CharData(pixelWidth, pixelHeight, charBits, xOffset, yOffset, width);
            }

            #region class CharData
            /// --------------------------------------------------------------------------
            /// <summary>
            /// This holds data for a specific character
            /// </summary>
            /// --------------------------------------------------------------------------
            [Serializable]
            public class CharData
            {
                internal int pixelWidth;
                internal int pixelHeight;
                internal int xOffset;
                internal int yOffset;
                internal int width;
                internal int[] bits;

                public int BackValue { get { return 1; } }
                public int XOffset { get { return xOffset; } }
                public int YOffset { get { return yOffset; } }
                public int PixelWidth { get { return pixelWidth; } }
                public int PixelHeight { get { return pixelHeight; } }
                public int Width { get { return width; } }
                public int[] Bits { get { return bits; } }

                /// --------------------------------------------------------------------------
                /// <summary>
                /// Constructor
                /// </summary>
                /// <param name="pixelWidth">Width of the bits buffer</param>
                /// <param name="pixelHeight">Height of the bits buffer</param>
                /// <param name="bits">Actual bits representing the character</param>
                /// <param name="xOffset">X Offset to start drawing the character bits</param>
                /// <param name="yOffset">X Offset to start drawing the character bits</param>
                /// <param name="width">Number of pixels to move over to start drawing the next character</param>
                /// --------------------------------------------------------------------------
                public CharData(int pixelWidth, int pixelHeight, int[] bits, int xOffset, int yOffset, int width)
                {
                    this.pixelWidth = pixelWidth;
                    this.pixelHeight = pixelHeight;
                    this.xOffset = xOffset;
                    this.yOffset = yOffset;
                    this.width = width;
                    this.bits = bits;
                }
            }
            #endregion

            // TODO: Add serialization attributes
            /// --------------------------------------------------------------------------
            /// <summary>
            /// Save this (rasterized) font to a file so that we can load it on other 
            /// systems that  might not have this font.
            /// http://www.devhood.com/tutorials/tutorial_details.aspx?tutorial_id=236
            /// </summary>
            /// <param name="fileName">fileName to save to</param>
            /// --------------------------------------------------------------------------
            public void SerializeToFile(string fileName)
            {
                Stream stream = new FileStream(fileName, System.IO.FileMode.Create);
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, this);
                stream.Close();
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Load a font from a previously serialized copy
            /// </summary>
            /// <param name="fileName"></param>
            /// <returns>New dvFont Object</returns>
            /// --------------------------------------------------------------------------
            public static DVFont FromFile(string fileName)
            {
                Stream stream = new FileStream(fileName, System.IO.FileMode.Open);

                return FromFile(stream);
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Load a font from a previously serialized copy
            /// </summary>
            /// <param name="stream">FIle stream</param>
            /// <returns>new dvFont object</returns>
            /// --------------------------------------------------------------------------
            public static DVFont FromFile(Stream stream)
            {
                IFormatter formatter = new BinaryFormatter();
                DVFont newFont = (DVFont)formatter.Deserialize(stream);

                stream.Close();

                newFont.Init();
                return newFont;
            }
        }

    }
}
