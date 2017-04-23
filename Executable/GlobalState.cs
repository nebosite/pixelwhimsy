using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using DirectVarmint;

namespace PixelWhimsy
{
    /// --------------------------------------------------------------------------
    /// <summary>
    /// A way to identify the brush
    /// </summary>
    /// --------------------------------------------------------------------------
    public enum BrushType
    {
        Pointer,
        Circle,
        Shader,
        Windmill,
        SprayPaint,
        LifePattern,
        PictureStamp,
        LineTarget,
        FloodFill,
        Dragging,

        // These brushes need to be last because there is no icon for them
        Bomb,
    };

    /// <summary>
    /// Ways to paint
    /// </summary>
    public enum PaintingStyle
    {
        Normal,
        Tile,
        Kaleidoscope
    }

    /// --------------------------------------------------------------------------
    /// <summary>
    /// This tracks the global state of the application for the support of 
    /// multiple windows
    /// </summary>
    /// --------------------------------------------------------------------------
    public static class GlobalState
    {
        public static BrushType brushType;
        public static BrushType lastBrushType;
        public static SoundPlayer.SoundInstance BrushSound;
        public static uint[] Palette;
        public static uint[] ToolPalette;
        public static int[,] rgbLookup5bit;
        public static SoundID CurrentNoteSound = SoundID.Note_Whistle;
        static int brushSize;
        public static bool endApplication = false;
        public static bool RandomBrush = false;
        public static bool RunningAsScreenSaver = false;
        public static bool RunningInPreview = false;
        public const int MaxBrushSize = 50;
        public static int TargetBrushSize = 10;
        public const string SettingsKeyName = @"software\Niftibits\PixelWhimsy";
        public const string LogFileName = "pixelWhimsyErrors.log";
        public static bool EasterHeads = false;
        public static ushort RainbowColor = 0xFF80;
        public static int numHeads = 11;
        public static bool Debugging = false;
        public static int ToolBarBorder = 150;
        public static bool AnimatedColorsGenerated = false;


        public static int resolutionX = 400;
        public static int resolutionY = 300;

        public static PaintingStyle PaintingStyle = PaintingStyle.Normal;

        private static int currentDrawingColor;
        private static int previousDrawingColor;

        public static bool EndApplication
        {
            get { return endApplication; }
            set
            {
                endApplication = value;
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Handle setting the brush type 
        /// </summary>
        /// --------------------------------------------------------------------------
        public static BrushType BrushType
        {
            get 
            {
                return brushType;
            }

            set
            {
                if(brushType != BrushType.Bomb) lastBrushType = brushType;
                brushType = value;
                if (BrushSound != null)
                {
                    BrushSound.Finished = true;
                    BrushSound = null;
                }
            }
        }

        /// <summary>
        /// The last brush before this one
        /// </summary>
        public static BrushType LastBrushType { get { return lastBrushType; } }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Brush size accessor
        /// </summary>
        /// --------------------------------------------------------------------------
        public static int BrushSize
        {
            get
            {
                if (brushSize < 0) brushSize = 0;
                if (brushSize > MaxBrushSize) brushSize = MaxBrushSize;
                return brushSize;
            }

            set
            {
                brushSize = value;
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Accessor for current drawing color.  
        /// </summary>
        /// --------------------------------------------------------------------------
        public static ushort CurrentDrawingColor
        {
            get
            {
                return (ushort)(currentDrawingColor);
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Accessor for previous drawing color.  
        /// </summary>
        /// --------------------------------------------------------------------------
        public static ushort PreviousDrawingColor
        {
            get
            {
                return (ushort)(previousDrawingColor);
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Set the global drawing color
        /// </summary>
        /// <param name="colorValue"></param>
        /// --------------------------------------------------------------------------
        public static void SetCurrentDrawingColor(int colorValue)
        {
            SetCurrentDrawingColor((ushort)colorValue);
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Set the global drawing color
        /// </summary>
        /// <param name="colorValue"></param>
        /// --------------------------------------------------------------------------
        public static void SetCurrentDrawingColor(ushort colorValue)
        {
            previousDrawingColor = currentDrawingColor;
            currentDrawingColor = colorValue;
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Static Constructor
        /// </summary>
        /// --------------------------------------------------------------------------
        static GlobalState()
        {
            BrushType = BrushType.Pointer;
            BrushSize = 10;
            currentDrawingColor = Color.White.ToArgb();
            Palette = new uint[0x10000];
            ToolPalette = new uint[0x10000];
            rgbLookup5bit = new int[0x10000, 3];
            for (int i = 0; i < 0x8000; i++)
            {
                int red = (i >> 10) & 0x1f;
                int green = (i >> 5) & 0x1f;
                int blue = (i >> 0) & 0x1f;
                rgbLookup5bit[i, 0] = red;
                rgbLookup5bit[i, 1] = green;
                rgbLookup5bit[i, 2] = blue;
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Set a regular RGB palette
        /// </summary>
        /// --------------------------------------------------------------------------
        public static void SetRGBPalette()
        {

            // Generate a 5 bit rgb palette
            double colorFactor = 255.0 / 31.0;

            for (int r = 0; r < 32; r++)
            {
                int redValue = (int)(r * colorFactor);

                for (int g = 0; g < 32; g++)
                {
                    int greenValue = (int)(g * colorFactor);

                    for (int b = 0; b < 32; b++)
                    {
                        int blueValue = (int)(b * colorFactor);

                        uint c = (uint)((r << 10) + (g << 5) + b);
                        GlobalState.Palette[c] = 0xff000000 | (uint)((redValue << 16) + (greenValue << 8) + blueValue);
                    }
                }
            }
            GlobalState.Palette[0] = 0xff000000;

            AnimatedColorsGenerated = false;
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Make the palette smoothly transition through colors for the screen decay
        /// mode.
        /// </summary>
        /// --------------------------------------------------------------------------
        public static void SetSmoothPalette()
        {
            double r = 0;
            double g = 0;
            double b = 0;
            uint tr = 0;
            uint tg = 0;
            uint tb = 0;
            double rm = 0, gm = 0, bm = 0;
            int t = 0;
            int jumpsize = 20;

            for (int i = 0; i < 0x10000; i++)
            {
                if (i >= t)
                {
                    uint newColor = Utilities.PickRandomRGBColor(false, 2, 2, 0);
                    t += Utilities.Rand(jumpsize * 3) + jumpsize;
                    tr = (newColor >> 16) & 0xff;
                    tg = (newColor >> 8) & 0xff;
                    tb = newColor & 0xff;
                    rm = (tr - r) / (t - i);
                    gm = (tg - g) / (t - i);
                    bm = (tb - b) / (t - i);
                    jumpsize += 50;
                }

                uint ir = ((uint)r) & 0xff;
                uint ig = ((uint)g) & 0xff;
                uint ib = ((uint)b) & 0xff;
                GlobalState.Palette[i] = 0xff000000 + (ir << 16) + (ig << 8) + ib;
                r += rm;
                b += bm;
                g += gm;
            }
        }
    }
}
