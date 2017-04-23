using System;
using System.Collections.Generic;
using System.Text;
using DirectVarmint;
using System.Drawing;

namespace PixelWhimsy
{
    /// <summary>
    /// Special chars
    /// </summary>
    public static class TextKey
    {
        public const char IncreaseFont = (char)1;
        public const char DecreaseFont = (char)2;
        public const char NextTypeFace = (char)3;
        public const char PrevTypeFace = (char)4;
    }

    public abstract partial class Animation
    {
        /// --------------------------------------------------------------------------
        /// <summary>
        /// Allow the user to enter some text
        /// </summary>
        /// --------------------------------------------------------------------------
        public class TextEntry : Animation
        {
            int frame;
            ushort color;
            StringBuilder text = new StringBuilder("");
            PixelBuffer.DVFont font;
            static int fontSize = 16;
            public static int FontID = 1;
            
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

            public string Text { get { return text.ToString(); } }
            /// --------------------------------------------------------------------------
            /// <summary>
            /// Constructor
            /// </summary>
            /// --------------------------------------------------------------------------
            public TextEntry(DVWindow window, ushort color, int x, int y)
                : base(window)
            {
                this.mousex = x;
                this.mousey = y;
                this.color = color;
                SetFont();
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Set the current font
            /// </summary>
            /// --------------------------------------------------------------------------
            private void SetFont()
            {
                bool antiAlias = true;
                if (this.color >= 0x8000) antiAlias = false;
                font = new PixelBuffer.DVFont(MediaBag.InstalledFonts[FontID], fontSize, FontStyle.Regular, antiAlias);
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Render
            /// </summary>
            /// --------------------------------------------------------------------------
            public override void Render()
            {
                if (IsDone) return;

                if (IsDone) return;
                this.frame++;
                dvWindow.OverlayBuffer.Clear(0);
                RenderToBuffer(dvWindow.OverlayBuffer, true);
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Render the text to a particular buffer
            /// </summary>
            /// --------------------------------------------------------------------------
            private void RenderToBuffer(PixelBuffer writeBuffer, bool renderCursor)
            {
                writeBuffer.PrintCursorX = mousex;
                writeBuffer.PrintCursorY = mousey;
                writeBuffer.PrintCarriageReturn = mousex;
                for (int i = 0; i < text.Length; i++)
                {
                    string output = new string(text[i], 1);
                    if ((int)font.Measure(output).Width + writeBuffer.PrintCursorX > writeBuffer.Width)
                    {
                        writeBuffer.Print(color, font, "\n");
                    }
                    writeBuffer.Print(color, font, output);
                }

                if (renderCursor)
                {
                    ushort cursorColor = (ushort)0x01;
                    if ((frame / 10) % 2 == 0)
                    {
                        cursorColor = MediaBag.color_White;
                    }
                    writeBuffer.Print(cursorColor, font, "_");
                }
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Write the text to the main buffer
            /// </summary>
            /// --------------------------------------------------------------------------
            public void WriteToMainBuffer()
            {
                RenderToBuffer(dvWindow.MainBuffer, false);
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Handle the user typing a character
            /// </summary>
            /// --------------------------------------------------------------------------
            public void HandleChar(char c)
            {
                switch (c)
                {
                    case (char)127:
                        if (text.Length > 0)
                        {
                            text.Remove(text.Length - 1, 1);
                        }
                        break;
                    case TextKey.IncreaseFont:
                        fontSize++;
                        if (fontSize > 50) fontSize = 50;
                        SetFont();
                        break;
                    case TextKey.DecreaseFont:
                        fontSize--;
                        if (fontSize < 4) fontSize = 4;
                        SetFont();
                        break;
                    case TextKey.NextTypeFace:
                        FontID++;
                        FontID = (FontID + MediaBag.InstalledFonts.Count)  % MediaBag.InstalledFonts.Count;
                        SetFont();
                        break;
                    case TextKey.PrevTypeFace:
                        FontID--;
                        FontID = (FontID + MediaBag.InstalledFonts.Count) % MediaBag.InstalledFonts.Count;
                        SetFont();
                        break;
                    default: 
                        text.Append(c);
                        break;
                }

            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Handle the user typing a character
            /// </summary>
            /// --------------------------------------------------------------------------
            public void SetText(string newText)
            {
                string[] textParts = newText.Split(' ');
                int maxWidth = (int)(dvWindow.Width * .8);
                this.text = new StringBuilder(textParts[0]);

                for (int i = 1; i < textParts.Length; i++)
                {
                    string test = text.ToString() + " " + textParts[i];
                    if (font.Measure(test).Width > maxWidth)
                    {
                        text.Append("\n");
                    }
                    else text.Append(" ");

                    text.Append(textParts[i]);
                }

                this.mousey = (int)((dvWindow.Height - font.Measure(text.ToString()).Height) / 2);

            }
        }
    }
}
