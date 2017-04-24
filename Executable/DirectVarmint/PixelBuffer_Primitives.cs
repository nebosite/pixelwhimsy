using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace DirectVarmint
{
    /// <summary>
    /// Used by some methods to indicate pixel set method
    /// </summary>
    public enum PixelMode
    {
        SET,
        XOR
    }

    public delegate void DrawHLine(ushort color, int x1, int x2, int y);

    /// --------------------------------------------------------------------------
    /// <summary>
    /// 
    /// </summary>
    /// --------------------------------------------------------------------------
    public partial class PixelBuffer
    {
        int printCursorX = 0;
        int printCursorY = 0;
        int printCarriageReturn = 0;
        public PaletteConverter GetPaletteColor = ColorConverters._5Bit;

        /// <summary>
        /// Current X location of the printing cursor
        /// </summary>
        public int PrintCursorX { get { return this.printCursorX; } set { this.printCursorX = value; } }

        /// <summary>
        /// Current Y location of the printing cursor
        /// </summary>
        public int PrintCursorY { get { return this.printCursorY; } set { this.printCursorY = value; } }

        /// <summary>
        /// Current location of where a carriage return will take the x location of the cursor
        /// </summary>
        public int PrintCarriageReturn { get { return this.printCarriageReturn; } set { this.printCarriageReturn = value; } }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Clear the buffer
        /// </summary>
        /// <param name="color"></param>
        /// --------------------------------------------------------------------------
        public void Clear(Color color)
        {
            DrawFilledRectangle(color, 0, 0, width - 1, height - 1);
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Clear the buffer
        /// </summary>
        /// <param name="color"></param>
        /// --------------------------------------------------------------------------
        public void Clear(ushort color)
        {
            DrawFilledRectangle(PixelMode.SET, color, 0, 0, width - 1, height - 1);
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Draw some text and set cursor location
        /// </summary>
        /// --------------------------------------------------------------------------
        public void Print(ushort color, PixelBuffer.DVFont font, int x, int y, string text)
        {
            printCursorX = x;
            printCarriageReturn = x;
            printCursorY = y;

            Print(color, font, text);
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Draw text from where the cursor left off
        /// </summary>
        /// --------------------------------------------------------------------------
        public void Print(ushort color, PixelBuffer.DVFont font, string text)
        {
            foreach (char c in text)
            {
                PrintCharacter(color, font, c);
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Print a single character to the current cursor location
        /// </summary>
        /// --------------------------------------------------------------------------
        private void PrintCharacter(ushort color, PixelBuffer.DVFont font, char c)
        {
            uint longColorValue = palette[color];
            uint red = (longColorValue >> 16) & 0xff;
            uint green = (longColorValue >> 8) & 0xff;
            uint blue = longColorValue & 0xff;

            if (c == '\r')
            {
                printCursorX = printCarriageReturn;
                return;
            }

            if (c == '\n')
            {
                printCursorX = printCarriageReturn;
                printCursorY += font.Height;
                return;
            }

            DVFont.CharData charData = font[c];
            int[] bits = charData.Bits;

            printCursorX += charData.BackValue;

            //clip
            int iStart = 0;
            int iEnd = charData.PixelWidth;
            int jStart = 0;
            int jEnd = charData.PixelHeight;

            int xStart = printCursorX + charData.XOffset;
            int yStart = printCursorY + charData.YOffset;
            int xEnd = xStart + charData.pixelWidth;
            int yEnd = yStart + charData.pixelHeight;

            if(xStart < 0)
            {
                iStart -= xStart;
                xStart = 0;
            }
            if(yStart < 0)
            {
                jStart -= yStart;
                yStart = 0;
            }
            if(xEnd >= this.width)
            {
                iEnd -= (xEnd - this.width);
            }
            if(yEnd >= this.height)
            {
                jEnd -= (yEnd - this.height);
            }
            

            for (int i = iStart; i < iEnd; i++)
            {
                for (int j = jStart; j < jEnd; j++)
                {
                    int protoValue = bits[i + j * charData.PixelWidth];
                    int x = xStart + i - iStart;
                    int y = yStart + j - jStart;
                    if (protoValue == 255)
                    {
                        mainBuffer[x + y * bufferPitch] = color;
                    }
                    else if (protoValue > 0)
                    {
                        int spot = x + y * bufferPitch;

                        uint underColor = palette[mainBuffer[spot]];
                        uint underRed = (underColor >> 16) & 0xff;
                        uint underGreen = (underColor >> 8) & 0xff;
                        uint underBlue = underColor & 0xff;

                        uint newRed = (uint)(underRed * (256 - protoValue) + red * protoValue);
                        uint newGreen = (uint)(underGreen * (256 - protoValue) + green * protoValue);
                        uint newBlue = (uint)(underBlue * (256 - protoValue) + blue * protoValue);

                        uint realColor =
                            ((newRed & 0xff00) << 8) +
                            ((newGreen & 0xff00)) +
                            (newBlue >> 8);

                        mainBuffer[spot] = GetPaletteColor(realColor);
                    }
                }
            }

            printCursorX += charData.Width;
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Bresenham line
        /// </summary>
        /// --------------------------------------------------------------------------
        public void DrawLine(Color color, int x1, int y1, int x2, int y2)
        {
            DrawLine(GetPaletteColor((uint)color.ToArgb()), x1, y1, x2, y2);
        }

        public void DrawLine(ushort color, int x1, int y1, int x2, int y2)
        {
            if (!clipLineToBuffer(ref x1, ref y1, ref x2, ref y2)) return;
            int temp;
            bool steep = Math.Abs(y2 - y1) > Math.Abs(x2 - x1);
            if(steep)
            {
                temp = y1; y1 = x1;  x1 = temp;
                temp = y2; y2 = x2;  x2 = temp;
            }
            if (x1 > x2)
            {
                temp = x2; x2 = x1; x1 = temp;
                temp = y2; y2 = y1; y1 = temp;
            }

            int deltax = x2 - x1;
            int deltay = Math.Abs(y2 - y1);
            int error = 0;
            int y = y1;
            int ystep = (y1 < y2) ? 1 : -1;
            int drawSpot;

            for (int x = x1; x <= x2; x++)
            {
                if (steep) drawSpot = y + x * bufferPitch; //DrawPixel(colorValue, y, x);
                else drawSpot = x + y * bufferPitch; //DrawPixel(colorValue, x, y);
                mainBuffer[drawSpot] = color;
                error += deltay;
                if ((error * 2) >= deltax)
                {
                    y += ystep;
                    error -= deltax;
                }
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Regular drawpixel.  
        /// </summary>
        /// <param name="color"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// --------------------------------------------------------------------------
        public void DrawPixel(Color color, int x, int y)
        {
            DrawPixel(GetPaletteColor((uint)color.ToArgb()),x, y);
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Faster version of drawpixel
        /// </summary>
        /// <param name="color"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// --------------------------------------------------------------------------
        public void DrawPixel(ushort color, int x, int y)
        {
            if(x >= 0 && y >= 0 && x < width && y < height)
                mainBuffer[x + y * bufferPitch] = color;
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Return the color of the specified Pixel
        /// </summary>
        /// --------------------------------------------------------------------------
        public ushort GetPixel(int x, int y)
        {
            if (x >= 0 && y >= 0 && x < width && y < height)
                return mainBuffer[x + y * bufferPitch];

            else return 0;
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// square
        /// </summary>
        /// --------------------------------------------------------------------------
        public void DrawRectangle(Color color, int x1, int y1, int x2, int y2)
        {
            DrawRectangle(GetPaletteColor((uint)color.ToArgb()), x1, y1, x2, y2);
        }

        public void DrawRectangle(ushort color, int x1, int y1, int x2, int y2)
        {
            DrawLine(color, x1, y1, x1, y2);
            DrawLine(color, x1, y1, x2, y1);
            DrawLine(color, x1, y2, x2, y2);
            DrawLine(color, x2, y1, x2, y2);
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Filled Rectangle 
        /// </summary>
        /// --------------------------------------------------------------------------
        public void DrawFilledRectangle(Color color, int x1, int y1, int x2, int y2)
        {
            DrawFilledRectangle(PixelMode.SET, color, x1, y1, x2, y2);
        }

        public void DrawFilledRectangle(ushort color, int x1, int y1, int x2, int y2)
        {
            DrawFilledRectangle(PixelMode.SET, color, x1, y1, x2, y2);
        }

        public void DrawFilledRectangle(PixelMode mode, Color color, int x1, int y1, int x2, int y2)
        {
            ushort c = GetPaletteColor((uint)color.ToArgb());
            DrawFilledRectangle(mode, c, x1, y1, x2, y2);
        }

        public void DrawFilledRectangle(PixelMode mode, ushort color, int x1, int y1, int x2, int y2)
        {
            switch (mode)
            {
                case PixelMode.XOR: DrawFilledRectangle(DrawHLine_XOR, color, x1, y1, x2, y2); break;
                default:            DrawFilledRectangle(DrawHLine_SET, color, x1, y1, x2, y2); break;
            }
        }

        public void DrawFilledRectangle(DrawHLine HLine, ushort color, int x1, int y1, int x2, int y2)
        {
            if (x1 > x2)
            {
                int tx = x1;
                x1 = x2;
                x2 = tx;
            }

            if (y1 > y2)
            {
                int ty = y1;
                y1 = y2;
                y2 = ty;
            }

            int scanwidth = x2 - x1 + 1;

            for (int j = y1; j <= y2; j++)
            {
                HLine(color, x1, x2, j);
            }

        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Optimized hline method for use with the other drawing functions
        /// </summary>
        /// --------------------------------------------------------------------------
        private void DrawHLine_SET(ushort color, int x1, int x2, int y)
        {
            if (x1 > x2)
            {
                int tx = x1;
                x1 = x2;
                x2 = tx;
            }

            if (x2 < 0 || x1 >= width || y < 0 || y >= height) return;
            if (x1 < 0) x1 = 0;
            if (x2 > width - 1) x2 = width - 1;

            int start = x1 + y * bufferPitch;
            int end = x2 + y * bufferPitch;

            while ((end - start) > 4)
            {
                mainBuffer[start] = color;
                mainBuffer[start + 1] = color;
                mainBuffer[start + 2] = color;
                mainBuffer[start + 3] = color;
                start += 4;
            }

            // catch straglers
            while (end >= start)
                mainBuffer[start++] = color;

        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Optimized hline method for use with the other drawing functions
        /// </summary>
        /// --------------------------------------------------------------------------
        private void DrawHLine_XOR(ushort color, int x1, int x2, int y)
        {
            if (x1 > x2)
            {
                int tx = x1;
                x1 = x2;
                x2 = tx;
            }

            if (x2 < 0 || x1 >= width || y < 0 || y >= height) return;
            if (x1 < 0) x1 = 0;
            if (x2 > width - 1) x2 = width - 1;

            int start = x1 + y * bufferPitch;
            int end = x2 + y * bufferPitch;

            while ((end - start) > 4)
            {
                mainBuffer[start] ^= color;
                mainBuffer[start + 1] ^= color;
                mainBuffer[start + 2] ^= color;
                mainBuffer[start + 3] ^= color;
                start += 4;
            }

            // catch straglers
            while (end >= start)
                mainBuffer[start++] ^= color;

        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Scanned Bresenham circle
        /// </summary>
        /// --------------------------------------------------------------------------
        public void DrawFilledCircle(Color color, int cx, int cy, int radius)
        {
            DrawFilledCircle(PixelMode.SET, color, cx, cy, radius);
        }

        public void DrawFilledCircle(ushort color, int cx, int cy, int radius)
        {
            DrawFilledCircle(PixelMode.SET, color, cx, cy, radius);
        }

        public void DrawFilledCircle(PixelMode mode, Color color, int cx, int cy, int radius)
        {
            ushort c = GetPaletteColor((uint)color.ToArgb());
            DrawFilledCircle(mode, c, cx, cy, radius);
        }

        public void DrawFilledCircle(PixelMode mode, ushort color, int cx, int cy, int radius)
        {
            switch (mode)
            {
                case PixelMode.XOR: DrawFilledCircle(DrawHLine_XOR, color, cx, cy, radius); break;
                default: DrawFilledCircle(DrawHLine_SET, color, cx, cy, radius); break;
            }
        }

        public void DrawFilledCircle(DrawHLine HLine, ushort color, int cx, int cy, int radius)
        {
	        int		x=0,
			        y=radius,
			        d=1-radius,
			        deltaE=3,
			        deltaSE= -2 * radius + 5;
	        int		lasty = y,lastx = x;

	        while(y > x) {
									        // Draw lines acrross the circle
		        if(y != lasty) {
                    HLine(color, cx - lastx, cx + lastx, cy - lasty);
                    HLine(color, cx - lastx, cx + lastx, cy + lasty);
		        }
		        if(x != lastx){
                    HLine(color, cx - lasty, cx + lasty, cy - lastx);
                    if (lastx != 0) HLine(color, cx - lasty, cx + lasty, cy + lastx);
		        }
		        lastx = x;
		        lasty = y;
									        // execute midpoint algorithm
		        if(d<0) {
			        d += deltaE;
			        deltaE += 2;
			        deltaSE +=2;
			        x++;
		        }
		        else {
			        d += deltaSE;
			        deltaE += 2;
			        deltaSE += 4;
			        x++;
			        y--;
		        }
	        }
									        // Finish up
	        if(y != lasty) {
                HLine(color, cx - lastx, cx + lastx, cy - lasty);
                HLine(color, cx - lastx, cx + lastx, cy + lasty);
	        }
	        if(x != lastx){
                HLine(color, cx - lasty, cx + lasty, cy - lastx);
                if (lastx != 0) HLine(color, cx - lasty, cx + lasty, cy + lastx);
	        }

	        if(y == x){
                HLine(color, cx - x, cx + x, cy - y);
                HLine(color, cx - x, cx + x, cy + y);
	        }
        }


        /// --------------------------------------------------------------------------
        /// <summary>
        /// Bresenham circle
        /// </summary>
        /// --------------------------------------------------------------------------
        public void DrawCircle(Color color, int cx, int cy, int radius)
        {
            DrawCircle(GetPaletteColor((uint)color.ToArgb()), cx, cy, radius);   
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Bresenham circle
        /// </summary>
        /// --------------------------------------------------------------------------
        public void DrawCircle(ushort color, int cx, int cy, int radius)
        {
	        int		x=0,
			        y=radius,
			        d=1-radius,
			        deltaE=3,
			        deltaSE= -2 * radius + 5;


	        if(radius == 0) DrawPixel(color,cx,cy);

	        while(y > x) {
		        circlepoints(color,cx,cy,x,y);
		        if(d<0) {
			        d += deltaE;
			        deltaE += 2;
			        deltaSE +=2;
			        x++;
		        }
		        else {
			        d += deltaSE;
			        deltaE += 2;
			        deltaSE += 4;
			        x++;
			        y--;
		        }

	        }
	        if(y == x)circlepoints(color,cx,cy,x,y);

        }

        
        /// --------------------------------------------------------------------------
        /// <summary>
        /// Helper method for drawcircle
        /// </summary>
        /// <param name="color"></param>
        /// <param name="cx"></param>
        /// <param name="cy"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// --------------------------------------------------------------------------
        void circlepoints( ushort color, int cx,int cy,int x, int y)
        {
	        int x1=cx-x,
		        x2=cx-y,
		        x3=cx+y,
		        x4=cx+x,

		        y1=cy-x,
		        y2=cy-y,
		        y3=cy+y,
		        y4=cy+x;

	        if(x != 0 && y != 0) {
		        DrawPixel(color,x3,y4);
		        DrawPixel(color,x2,y1);
		        DrawPixel(color,x1,y3);
		        DrawPixel(color,x4,y2);
	        }

	        if(x == y) return;
	        DrawPixel(color,x4,y3);
	        DrawPixel(color,x2,y4);
	        DrawPixel(color,x1,y2);
	        DrawPixel(color,x3,y1);
        }


       /// <summary>
       /// Codes used for line clipping
       /// </summary>
        [Flags]
        enum OutCode { NONE = 0, LINE_LEFT = 1, LINE_RIGHT = 2, LINE_BOTTOM = 4, LINE_TOP = 8 };

        /// --------------------------------------------------------------------------
        /// <summary>
        /// determine boundary codes for a line
        /// </summary>
        /// --------------------------------------------------------------------------
        private OutCode GetOutCode(int x, int y, int minx, int maxx, int miny, int maxy)
        {
            OutCode code = OutCode.NONE;
            if (y > maxy) code |= OutCode.LINE_TOP;
            if (y < miny) code |= OutCode.LINE_BOTTOM;
            if (x > maxx) code |= OutCode.LINE_RIGHT;
            if (x < minx) code |= OutCode.LINE_LEFT;
            return code;
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Clip a line to a rectangluar buffer
        /// </summary>
        /// --------------------------------------------------------------------------
        private bool clipLineToBuffer(ref int x0, ref int y0, ref int x1, ref int y1)
        {
            int minx = 0;
            int maxx = width-1;
            int miny = 0;
            int maxy = height-1;
            bool accept = false;
            bool done = false;
            OutCode outcode0 = OutCode.NONE, outcode1 = OutCode.NONE, outcodeout = OutCode.NONE;
            int x = 0, y = 0;
            // Check initial end points
            outcode0 = GetOutCode(x0, y0, minx, maxx, miny, maxy);
            outcode1 = GetOutCode(x1, y1, minx, maxx, miny, maxy);


            while (!done)
            {
                // Trivial Accept (line totally inside)
                if ((outcode0 | outcode1) == OutCode.NONE)
                {
                    done = accept = true;
                    continue;
                }
                // Trivial reject (line totally outside)
                if ((outcode0 & outcode1) != OutCode.NONE)
                {
                    done = true;
                    continue;
                }

                // At this point, we need to break up the
                // line and reject parts.
                if (outcode0 != OutCode.NONE) outcodeout = outcode0;
                else outcodeout = outcode1;
                // Find intersection point
                if ((OutCode.LINE_TOP & outcodeout) == OutCode.LINE_TOP)
                {
                    x = x0 + (x1 - x0) * (maxy - y0) / (y1 - y0);
                    y = maxy;
                }
                else if ((OutCode.LINE_BOTTOM & outcodeout) == OutCode.LINE_BOTTOM)
                {
                    x = x0 + (x1 - x0) * (miny - y0) / (y1 - y0);
                    y = miny;
                }
                else if ((OutCode.LINE_RIGHT & outcodeout) == OutCode.LINE_RIGHT)
                {
                    y = y0 + (y1 - y0) * (maxx - x0) / (x1 - x0);
                    x = maxx;
                }
                else if ((OutCode.LINE_LEFT & outcodeout) == OutCode.LINE_LEFT)
                {
                    y = y0 + (y1 - y0) * (minx - x0) / (x1 - x0);
                    x = minx;
                }
                // Move outside point to the intersect and
                // repeat pass.
                if (outcodeout == outcode0)
                {
                    x0 = x;
                    y0 = y;
                    outcode0 = GetOutCode(x0, y0, minx, maxx, miny, maxy);
                }
                else
                {
                    x1 = x;
                    y1 = y;
                    outcode1 = GetOutCode(x1, y1, minx, maxx, miny, maxy);
                }

            }
            return accept;
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// THe class is a holder for methods to convert true color values to palette  colors
        /// </summary>
        /// --------------------------------------------------------------------------
        public static class ColorConverters
        {
            /// --------------------------------------------------------------------------
            /// <summary>
            /// Convert an 8bit RGB color value to a 5 bit palette color value
            /// </summary>
            /// <param name="realColor"></param>
            /// <returns></returns>
            /// --------------------------------------------------------------------------
            public static ushort _5Bit(uint realColor)
            {
                return (ushort)( 
                        ((realColor & 0xF80000) >> 9) +
                        ((realColor & 0xF800) >> 6) +
                        ((realColor & 0xF8) >> 3));
            }


        }
    }
}
