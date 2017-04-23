using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using DirectVarmint;

namespace PixelWhimsy
{
    public partial class Slate
    {
        int cpTop = 50;
        int cpBottom;
        int cpLeft = 50;
        int cpRight;

        const int framesPerAnimatedColor = 64;
        int columnsInColorPicker = 5;


        /// --------------------------------------------------------------------------
        /// <summary>
        /// Show current color under the mouse
        /// </summary>
        /// --------------------------------------------------------------------------
        private void HandleMouseOnColorPicker(int mouseX, int mouseY)
        {
            ushort mouseColor = dvWindow.MainBuffer.GetPixel(mouseX, mouseY);
            dvWindow.MainBuffer.DrawFilledRectangle(mouseColor, cpLeft + 10, cpTop + 10, cpRight - 10, cpTop + 50);
            dvWindow.MainBuffer.DrawFilledRectangle(GlobalState.CurrentDrawingColor, cpLeft + 50, cpTop + 15, cpRight - 15, cpTop + 45);
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Draw the colorpicker the slow hard way
        /// </summary>
        /// --------------------------------------------------------------------------
        private void DrawRawColorPicker(int cpx, int cpy)
        {
            int w = dvWindow.MainBuffer.Width;

            cpTop = cpy;
            cpBottom = cpy + 204;
            cpLeft = cpx;
            cpRight = cpx + 115;

            colorPickerSprite = dvWindow.MainBuffer.CaptureSprite(cpLeft, cpTop, cpRight, cpBottom);

            dvWindow.MainBuffer.DrawFilledRectangle(Color.Gray, cpLeft+1, cpTop+1, cpRight-1, cpBottom-1);
            for (uint i = 1; i < 8; i++)
            {
                uint root = i * 18 + 127;
                ushort brightColor = dvWindow.MainBuffer.GetPaletteColor((root << 16) + (root << 8) + root);
                root -= 127;
                ushort darkColor = dvWindow.MainBuffer.GetPaletteColor((root << 16) + (root << 8) + root);
                dvWindow.MainBuffer.DrawLine(brightColor, (int)(cpLeft + i), (int)(cpTop + i), (int)(cpLeft + i), (int)(cpBottom - i));
                dvWindow.MainBuffer.DrawLine(brightColor, (int)(cpLeft + i), (int)(cpTop + i), (int)(cpRight - i), (int)(cpTop + i));
                dvWindow.MainBuffer.DrawLine(darkColor, (int)(cpRight - i), (int)(cpBottom - i), (int)(cpRight - i), (int)(cpTop + i));
                dvWindow.MainBuffer.DrawLine(darkColor, (int)(cpRight - i), (int)(cpBottom - i), (int)(cpLeft + i), (int)(cpBottom - i));
            }

            // Draw animated colors
            for (int i = 0; i < 512; i++)
            {

                int x = cpx + (i % columnsInColorPicker) * 20 + 10;
                int y = cpBottom - 45 - (i / columnsInColorPicker);
                for (int j = 0; j < 16; j++)
                {
                    dvWindow.MainBuffer.DrawPixel((ushort)(0x8000 + i * 64 + j * 4), x + j, y);
                }
            }

            // Solid color palette
            uint[] midColor = new uint[6];
            for (uint i = 0; i < 16; i++)
            {
                uint lc = 255 - i * 17;
                uint rc = i * 17;
                midColor[0] = (255 << 16) + (rc << 8);//RY
                midColor[1] = (255 << 8) + (lc << 16);//YG
                midColor[2] = (255 << 8) + (rc << 0);//GC
                midColor[3] = (255 << 0) + (lc << 8);//CB
                midColor[4] = (255 << 0) + (rc << 16);//BV
                midColor[5] = (255 << 16) + (lc << 0);//VR

                for (uint j = 1; j < 16; j++)
                {
                    for (uint k = 0; k < 6; k++)
                    {
                        uint whiteComponent = (255 * j) / 15;
                        uint redComponent = (((midColor[k] >> 16) & 0xff) * (15 - j)) / 15;
                        uint greenComponent = (((midColor[k] >> 8) & 0xff) * (15 - j)) / 15;
                        uint blueComponent = (((midColor[k] >> 0) & 0xff) * (15 - j)) / 15;

                        uint topColor =
                            ((whiteComponent + redComponent) << 16) +
                            ((whiteComponent + greenComponent) << 8) +
                            ((whiteComponent + blueComponent) << 0);
                        uint bottomColor =
                            ((redComponent) << 16) +
                            ((greenComponent) << 8) +
                            ((blueComponent) << 0);

                        int x = (int)(cpx + i + k * 16) + 12;
                        dvWindow.MainBuffer.DrawPixel(
                            dvWindow.MainBuffer.GetPaletteColor(topColor),
                            x,
                            (int)(cpBottom - 25 - j));
                        dvWindow.MainBuffer.DrawPixel(
                            dvWindow.MainBuffer.GetPaletteColor(midColor[k]),
                            x,
                            cpBottom - 25);
                        dvWindow.MainBuffer.DrawPixel(
                            dvWindow.MainBuffer.GetPaletteColor(bottomColor),
                            x,
                            (int)(cpBottom - 25 + j));
                    }
                }
            }
            
            dvWindow.MainBuffer.CaptureSpriteFrame(colorPickerSprite, cpx, cpy);
        }


        /// --------------------------------------------------------------------------
        /// <summary>
        /// Render the color picker
        /// </summary>
        /// --------------------------------------------------------------------------
        private void ToggleColorPicker(int cpx, int cpy)
        {

            colorPicker = !colorPicker;

            if (colorPicker)
            {
                if (colorPickerSprite == null)
                {
                    DrawRawColorPicker(cpx, cpy);
                }
                else
                {
                    dvWindow.MainBuffer.CaptureSpriteFrame(colorPickerSprite, cpx, cpy, 0);
                    dvWindow.MainBuffer.DrawSprite(colorPickerSprite, 1, cpx, cpy);
                }
            }
            else
            {
                dvWindow.MainBuffer.DrawSprite(colorPickerSprite, 0, cpx, cpy);
            }

        }

        static uint lastAnimatedFrame = 99999;

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Handle animated colors
        /// </summary>
        /// --------------------------------------------------------------------------
        public void AnimatePalette()
        {
            if (frame == lastAnimatedFrame) return;
            if (AnimationExists(typeof(Animation.ScreenDecay))) return;

            lastAnimatedFrame = frame;
            uint shortFrame = frame % framesPerAnimatedColor;

            int levelsPerBit4 = 255 / 3;
            int levelsPerBit5 = 255 / 4;

            if (!GlobalState.AnimatedColorsGenerated)
            {
                for (int r = 0; r < 4; r++)
                {
                    for (int g = 0; g < 5; g++)
                    {
                        for (int b = 0; b < 5; b++)
                        {
                            int colorNum = r * 25 + g * 5 + b;
                            uint color = (uint)(((r * levelsPerBit4) << 16) + ((g * levelsPerBit5) << 8) + b * levelsPerBit5);
                            CreateWalkingColorOnBlack(colorNum * columnsInColorPicker, shortFrame, color);
                            CreateWalkingBlackOnColor(colorNum * columnsInColorPicker + 1, shortFrame, color);
                            CreateWalkingWhiteOnColor(colorNum * columnsInColorPicker + 2, shortFrame, color);
                            CreateWalkingSinusoidalColor(colorNum * columnsInColorPicker + 3, shortFrame, color);
                            CreateWalkingBlockComplimentaryColor(colorNum * columnsInColorPicker + 4, shortFrame, color);
                        }
                    }
                }

                CreateChaosColor(500, (uint)Color.Red.ToArgb());
                CreateChaosColor(501, (uint)Color.Yellow.ToArgb());
                CreateChaosColor(502, (uint)Color.Green.ToArgb());
                CreateChaosColor(503, (uint)Color.Cyan.ToArgb());
                CreateChaosColor(504, (uint)Color.Blue.ToArgb());
                CreateChaosColor(505, (uint)Color.Magenta.ToArgb());
                CreateChaosColor(506, (uint)Color.White.ToArgb());
                CreateChaosColor(507, (uint)Color.Gray.ToArgb());
                CreateChaosColor(508, (uint)Color.Chartreuse.ToArgb());
                CreateChaosColor(509, (uint)Color.Maroon.ToArgb());
                CreateRegularRainbow(510);
                CreateRandomRainbow(511);
                

                for(int i = 0x8000; i < 0x10000; i++)
                {
                    GlobalState.rgbLookup5bit[i, 0] = (int)((GlobalState.Palette[i] >> 19) & 0x1f);
                    GlobalState.rgbLookup5bit[i, 1] = (int)((GlobalState.Palette[i] >> 11) & 0x1f);
                    GlobalState.rgbLookup5bit[i, 2] = (int)((GlobalState.Palette[i] >> 3) & 0x1f);
                }

                GlobalState.AnimatedColorsGenerated = true;
            }
            else
            {
                for (int i = 0; i < 0x8000 / framesPerAnimatedColor; i++)
                {
                    int offSet = 0x8000 + i * framesPerAnimatedColor;
                    uint tempColor = GlobalState.Palette[offSet + framesPerAnimatedColor - 1];
                    for (int j = framesPerAnimatedColor - 1; j > 0; j--)
                    {
                        GlobalState.Palette[offSet + j] = GlobalState.Palette[offSet + j - 1];
                    }
                    GlobalState.Palette[offSet] = tempColor;
                }
            }

            DVTools.FixPalette(GlobalState.Palette);
        }


        /// --------------------------------------------------------------------------
        /// <summary>
        /// Color stripe moving in a black background
        /// </summary>
        /// --------------------------------------------------------------------------
        private void CreateWalkingColorOnBlack(int slot, uint frame, uint color)
        {
            uint r = (color >> 16) & 0xff;
            uint g = (color >> 8) & 0xff;
            uint b = (color >> 0) & 0xff;

            int index = 0x8000 + (slot * framesPerAnimatedColor);

            for (int i = 0; i < framesPerAnimatedColor; i++) GlobalState.Palette[index + i] = 0;

            GlobalState.Palette[index + frame] = (uint)((r << 16) + (g << 8) + b);

            for (int i = 1; i < 10; i++)
            {
                double factor = (10.0 - i) / 10.0;
                uint c = (uint)(((int)(r * factor) << 16) + ((int)(g * factor) << 8) + (b * factor));
                GlobalState.Palette[index + ((frame + i) % framesPerAnimatedColor)] = c;
                GlobalState.Palette[index + ((frame + framesPerAnimatedColor - i) % framesPerAnimatedColor)] = c;
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Black stripe moving on a color background
        /// </summary>
        /// --------------------------------------------------------------------------
        private void CreateWalkingBlackOnColor(int slot, uint frame, uint color)
        {
            uint r = (color >> 16) & 0xff;
            uint g = (color >> 8) & 0xff;
            uint b = (color >> 0) & 0xff;

            int index = 0x8000 + (slot * framesPerAnimatedColor);

            for (int i = 0; i < framesPerAnimatedColor; i++)
            {
                double factor = (Math.Sin(((i * Math.PI) / (framesPerAnimatedColor))) + 1) / 2.0;
                factor *= factor * factor;
                int nr = (int)(r * factor);
                int ng = (int)(g * factor);
                int nb = (int)(b * factor);
                uint c = (uint)((nr << 16) + (ng << 8) + nb);
                GlobalState.Palette[index + ((frame + i) % framesPerAnimatedColor)] = c;
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// White stripe moving on a color background
        /// </summary>
        /// --------------------------------------------------------------------------
        private void CreateWalkingWhiteOnColor(int slot, uint frame, uint color)
        {
            uint r = (color >> 16) & 0xff;
            uint g = (color >> 8) & 0xff;
            uint b = (color >> 0) & 0xff;

            int index = 0x8000 + (slot * framesPerAnimatedColor);

            for (int i = 0; i < framesPerAnimatedColor; i++) GlobalState.Palette[index + i] = color;

            GlobalState.Palette[index + frame] = 0xffffffff;

            for (int i = 1; i < 10; i++)
            {
                double factor = (10.0 - (10 - i)) / 10.0;
                int nr = (int)(255 * (1 - factor) + r * factor);
                int ng = (int)(255 * (1 - factor) + g * factor);
                int nb = (int)(255 * (1 - factor) + b * factor);
                uint c = (uint)((nr << 16) + (ng << 8) + nb);
                GlobalState.Palette[index + ((frame + i) % framesPerAnimatedColor)] = c;
                GlobalState.Palette[index + ((frame + framesPerAnimatedColor - i) % framesPerAnimatedColor)] = c;
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// White stripe moving on a color background
        /// </summary>
        /// --------------------------------------------------------------------------
        private void CreateWalkingSinusoidalColor(int slot, uint frame, uint color)
        {
            uint r = (color >> 16) & 0xff;
            uint g = (color >> 8) & 0xff;
            uint b = (color >> 0) & 0xff;

            int index = 0x8000 + (slot * framesPerAnimatedColor);

            for (int i = 0; i < framesPerAnimatedColor; i++)
            {
                double factor = (double)i / framesPerAnimatedColor;
                int nr = (int)(r * factor);
                int ng = (int)(g * factor);
                int nb = (int)(b * factor);
                uint c = (uint)((nr << 16) + (ng << 8) + nb);
                GlobalState.Palette[index + ((frame + i) % framesPerAnimatedColor)] = c;
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// White stripe moving on a color background
        /// </summary>
        /// --------------------------------------------------------------------------
        private void CreateChaosColor(int slot, uint color)
        {
            uint r = (color >> 16) & 0xff;
            uint g = (color >> 8) & 0xff;
            uint b = (color >> 0) & 0xff;

            int index = 0x8000 + (slot * framesPerAnimatedColor);

            for (int i = 0; i < framesPerAnimatedColor; i++)
            {
                double factor = Utilities.DRand(1);
                int nr = (int)(r * factor);
                int ng = (int)(g * factor);
                int nb = (int)(b * factor);
                uint c = (uint)((nr << 16) + (ng << 8) + nb);
                GlobalState.Palette[index + ((frame + i) % framesPerAnimatedColor)] = c;
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// White stripe moving on a color background
        /// </summary>
        /// --------------------------------------------------------------------------
        private void CreateRegularRainbow(int slot)
        {

            int index = 0x8000 + (slot * framesPerAnimatedColor);


            FillRange(GlobalState.Palette, index,      11, (uint)Color.Red.ToArgb(), (uint)Color.Yellow.ToArgb());
            FillRange(GlobalState.Palette, index + 11, 11, (uint)Color.Yellow.ToArgb(), (uint)Color.Green.ToArgb());
            FillRange(GlobalState.Palette, index + 22, 11, (uint)Color.Green.ToArgb(), (uint)Color.Cyan.ToArgb());
            FillRange(GlobalState.Palette, index + 33, 11, (uint)Color.Cyan.ToArgb(), (uint)Color.Blue.ToArgb());
            FillRange(GlobalState.Palette, index + 44, 10, (uint)Color.Blue.ToArgb(), (uint)Color.Magenta.ToArgb());
            FillRange(GlobalState.Palette, index + 54, 10, (uint)Color.Magenta.ToArgb(), (uint)Color.Red.ToArgb());
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// White stripe moving on a color background
        /// </summary>
        /// --------------------------------------------------------------------------
        private void CreateRandomRainbow(int slot)
        {

            int index = 0x8000 + (slot * framesPerAnimatedColor);

            uint rc1 = Utilities.PickRandomRGBColor(true, 4, 4, 0);
            uint rc2 = Utilities.PickRandomRGBColor(true, 4, 4, 0);
            uint originalColor = rc1;

            FillRange(GlobalState.Palette, index, 11, rc1, rc2);

            rc1 = rc2;
            rc2 = Utilities.PickRandomRGBColor(true, 4, 4, 0);
            FillRange(GlobalState.Palette, index + 11, 11, rc1, rc2);

            rc1 = rc2;
            rc2 = Utilities.PickRandomRGBColor(true, 4, 4, 0);
            FillRange(GlobalState.Palette, index + 22, 11, rc1, rc2);

            rc1 = rc2;
            rc2 = Utilities.PickRandomRGBColor(true, 4, 4, 0);
            FillRange(GlobalState.Palette, index + 33, 11, rc1, rc2);

            rc1 = rc2;
            rc2 = Utilities.PickRandomRGBColor(true, 4, 4, 0);
            FillRange(GlobalState.Palette, index + 44, 10, rc1, rc2);

            rc1 = rc2;
            rc2 = originalColor;
            FillRange(GlobalState.Palette, index + 54, 10, rc1, rc2);
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Helper method to fill an interpolated range of colors.
        /// </summary>
        /// --------------------------------------------------------------------------
        private void FillRange(uint[] colorArray, int indexFrom, int count, uint color1, uint color2)
        {
            int r1 = (int)((color1 >> 16) & 0xff);
            int g1 = (int)((color1 >> 8) & 0xff);
            int b1 = (int)((color1 >> 0) & 0xff);
            int r2 = (int)((color2 >> 16) & 0xff);
            int g2 = (int)((color2 >> 8) & 0xff);
            int b2 = (int)((color2 >> 0) & 0xff);

            float rstep = (r2 - r1) / (float)(count - 1);
            float gstep = (g2 - g1) / (float)(count - 1);
            float bstep = (b2 - b1) / (float)(count - 1);

            for (int i = 0; i < count; i++)
            {
                uint nr = (uint)(r1 + rstep * i);
                uint ng = (uint)(g1 + gstep * i);
                uint nb = (uint)(b1 + bstep * i);
                uint c = (uint)((nr << 16) + (ng << 8) + nb);
                colorArray[indexFrom + i] = c;
            }
        }


        /// --------------------------------------------------------------------------
        /// <summary>
        /// White stripe moving on a color background
        /// </summary>
        /// --------------------------------------------------------------------------
        private void CreateWalkingBlockComplimentaryColor(int slot, uint frame, uint color)
        {
            uint r = (color >> 16) & 0xff;
            uint g = (color >> 8) & 0xff;
            uint b = (color >> 0) & 0xff;

            int index = 0x8000 + (slot * framesPerAnimatedColor);
            int nr;
            int ng;
            int nb;

            for (int i = 0; i < framesPerAnimatedColor; i++)
            {
                if ((i & 0x7) == 0)
                {
                    nr = ng = nb = 127;
                }
                else if ((i & 0x8) == 0)
                {
                    nr = (int)(r);
                    ng = (int)(g);
                    nb = (int)(b);
                }
                else
                {
                    nr = (int)((255 - r));
                    ng = (int)((255 - g));
                    nb = (int)((255 - b));
                }

                uint c = (uint)((nr << 16) + (ng << 8) + nb);
                GlobalState.Palette[index + ((frame + i) % framesPerAnimatedColor)] = c;
            }
        }
    }
}
