using System;
using System.Collections.Generic;
using System.Text;
using DirectVarmint;
using System.Drawing;

namespace PixelWhimsy
{
    public enum ScreenFlowSimpleMode
    {
        Up,
        Down,
        Left,
        Right,
        UpDown,
        LeftRight,
        Slash,
        Whack,
        SlashWhack,
        SpiralIn,
        SpiralOut,
        SnakeDown,
        SnakeUp,
        SnakeLeft,
        SnakeRight,
        MaxCount
    }

    public abstract partial class Animation
    {
        public static Dictionary<ScreenFlowSimpleMode, uint[]> flowTemplates = new Dictionary<ScreenFlowSimpleMode, uint[]>();
        /// --------------------------------------------------------------------------
        /// <summary>
        /// Pixels flow
        /// </summary>
        /// --------------------------------------------------------------------------
        public class ScreenFlowSimple : Animation
        {
            SoundPlayer.SoundInstance sound;
            uint[] flowTemplate;
            ScreenFlowSimpleMode flowMode;
            static ushort[] tempBuffer = null;

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
            public ScreenFlowSimple(DVWindow window, ScreenFlowSimpleMode flowMode)
                : base(window)
            {
                sound = MediaBag.Play(SoundID.Loop_Squeak, .8, .03, true);
                this.flowMode = flowMode;

                GenerateFlowTemplate(flowMode);
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Create a flow template for the given mode
            /// </summary>
            /// --------------------------------------------------------------------------
            private void GenerateFlowTemplate(ScreenFlowSimpleMode flowMode)
            {
                int pitch = dvWindow.MainBuffer.BufferPitch;
                int w = dvWindow.MainBuffer.Width;
                int h = dvWindow.MainBuffer.Height;
                flowTemplate = new uint[pitch * h];
                tempBuffer = new ushort[h * pitch];
                int size = pitch * h;

                switch (flowMode)
                {
                    case ScreenFlowSimpleMode.SnakeUp: CreateSnake(w, h, 0, 1, pitch); break;
                    case ScreenFlowSimpleMode.SnakeDown: CreateSnake(w, h, 0, -1, pitch); break;
                    case ScreenFlowSimpleMode.SnakeLeft: CreateSnake(w, h, -1, 0, pitch); break;
                    case ScreenFlowSimpleMode.SnakeRight: CreateSnake(w, h, 1, 0, pitch); break;
                    case ScreenFlowSimpleMode.SpiralIn: CreateSpiral(-1, w, h, pitch); break;
                    case ScreenFlowSimpleMode.SpiralOut: CreateSpiral(1, w, h, pitch); break;
                    case ScreenFlowSimpleMode.SlashWhack:
                        for (int y = 0; y < h; y++)
                        {
                            for (int x = 0; x < w; x++)
                            {
                                int xm = -1;
                                int ym = -1;
                                if ((x + y) % 2 == 0)
                                {
                                    xm = ym = 1;
                                }

                                int fromx = ((x + w) + xm) % w;
                                int fromy = ((y + h) + ym) % h;
                                flowTemplate[x + y * pitch] = (uint)(fromx + fromy * pitch);
                            }
                        }
                        break;
                    case ScreenFlowSimpleMode.LeftRight:
                        for (int y = 0; y < h; y++)
                        {
                            for (int x = 0; x < w; x++)
                            {
                                int xm = (y % 2) == 0 ? 1 : -1;
                                int ym = 0;
                                int fromx = ((x + w) + xm) % w;
                                int fromy = ((y + h) + ym) % h;
                                flowTemplate[x + y * pitch] = (uint)(fromx + fromy * pitch);
                            }
                        }
                        break;
                    case ScreenFlowSimpleMode.UpDown:
                        for (int y = 0; y < h; y++)
                        {
                            for (int x = 0; x < w; x++)
                            {
                                int xm = 0;
                                int ym = (x % 2) == 0 ? 1 : -1;
                                int fromx = ((x + w) + xm) % w;
                                int fromy = ((y + h) + ym) % h;
                                flowTemplate[x + y * pitch] = (uint)(fromx + fromy * pitch);
                            }
                        }
                        break;
                    case ScreenFlowSimpleMode.Right: CreateSimpleTemplate(pitch, w, h, -1, 0); break;
                    case ScreenFlowSimpleMode.Left:     CreateSimpleTemplate(pitch, w, h,  1,  0); break;
                    case ScreenFlowSimpleMode.Up:       CreateSimpleTemplate(pitch, w, h,  0,  1); break;
                    case ScreenFlowSimpleMode.Down:     CreateSimpleTemplate(pitch, w, h,  0, -1); break;
                    case ScreenFlowSimpleMode.Slash:    CreateSimpleTemplate(pitch, w, h, -1,  -1); break;
                    case ScreenFlowSimpleMode.Whack:    CreateSimpleTemplate(pitch, w, h,  1,   1); break;
                    default: break;
                }
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Create a sprial - either in or out
            /// </summary>
            /// --------------------------------------------------------------------------
            private void CreateSnake(int w, int h, int xm, int ym, int pitch)
            {
                int x = 0, y = 0;
                int direction = 0;
                if (xm == 0)
                {
                    direction = ym;
                    y = (ym == 1 ? 0 : h - 1);
                    x = 0;
                    xm = 1;
                    ym = 0;
                }
                else
                {
                    direction = xm;
                    x = (xm == 1 ? 0 : w - 1);
                    y = 0;
                    ym = 1;
                    xm = 0;
                }

                int startx = x;
                int starty = y;
                int pixelsToGo = w * h;
                int left = 0;
                int right = w-1;
                int top = 0;
                int bottom = h-1;
                uint lastSpot = 0;
                bool movex = false;
                bool movey = false;


                while (pixelsToGo > 0)
                {
                    flowTemplate[x + y * pitch] = lastSpot;
                    lastSpot = (uint)(x + y * pitch);
                    pixelsToGo--;

                    if (movex)
                    {
                        x += direction;
                        movex = false;
                        continue;
                    }
                    else if (movey)
                    {
                        y += direction;
                        movey = false;
                        continue;
                    }
                    else
                    {
                        x += xm;
                        y += ym;
                    }

                    if (xm != 0 && (x == left || x == right))
                    {
                        xm = -xm;
                        movey = true;
                        //if (y < 0 || y >= h) break;
                    }
                    else if (ym != 0 && (y == top || y == bottom))
                    {
                        ym = -ym;
                        movex = true;
                        //if (x < 0 || x >= w) break;
                    }
                }

                flowTemplate[startx + starty * pitch] = lastSpot;
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Create a sprial - either in or out
            /// </summary>
            /// --------------------------------------------------------------------------
            private void CreateSpiral(int direction, int w, int h, int pitch)
            {
                int x = w / 2;
                int y = h / 2;
                int pixelsToGo = w * h;
                int left = x - 1;
                int right = x + 1;
                int top = y - 1;
                int bottom = y + 1;
                int xm = direction;
                int ym = 0;
                uint lastSpot = 0;

                while (pixelsToGo > 0)
                {
                    if (x >= 0 && x < w && y >= 0 && y < h)
                    {
                        if (direction == 1)
                        {
                            flowTemplate[x + y * pitch] = lastSpot;
                            lastSpot = (uint)(x + y * pitch);
                        }
                        else
                        {
                            flowTemplate[lastSpot] = (uint)(x + y * pitch);
                            lastSpot = (uint)(x + y * pitch);
                        }
                        pixelsToGo--;
                    }

                    x += xm;
                    y += ym;

                    if (x == left)
                    {
                        left--;
                        xm = 0;
                        ym = -direction;
                    }
                    else if (x == right)
                    {
                        right++;
                        xm = 0;
                        ym = direction;
                    }
                    else if (y == top)
                    {
                        top--;
                        xm = direction;
                        ym = 0;
                    }
                    else if (y == bottom)
                    {
                        bottom++;
                        xm = -direction;
                        ym = 0;
                    }
                }
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Create a simple template that just moves in one direction
            /// </summary>
            /// --------------------------------------------------------------------------
            private void CreateSimpleTemplate(int pitch, int w, int h, int xm, int ym)
            {
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        int fromx = ((x + w) + xm) % w;
                        int fromy = ((y + h) + ym) % h;
                        flowTemplate[x + y * pitch] = (uint)(fromx + fromy * pitch);
                    }
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

                ushort[] data = dvWindow.MainBuffer.RawBuffer;
                int height = dvWindow.MainBuffer.Height;
                int width = dvWindow.MainBuffer.Width;
                int pitch = dvWindow.MainBuffer.BufferPitch;
                int size = height * pitch;

                for (int y = 0; y < height; y++)
                {
                    int writeSpot = y * pitch;
                    for (int x = 0; x < width; x++)
                    {
                        uint source = flowTemplate[writeSpot];
                        if(source != size)
                        {
                            tempBuffer[writeSpot] = data[source];
                        }
                        writeSpot++;
                    }
                }

                Array.Copy(tempBuffer, 0, data, 0, height * pitch);

            }
        }
    }
}
