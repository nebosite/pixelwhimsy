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
        /// Diffuse the image by swapping pixels
        /// </summary>
        /// --------------------------------------------------------------------------
        public class PixelDiffuser : Animation
        {
            SoundPlayer.SoundInstance sound;
            int w, h, pitch;
            ushort[] data;
            public double milliseconds;

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
            public PixelDiffuser(DVWindow window)
                : base(window)
            {
                sound = MediaBag.Play(SoundID.Loop_Hiss, 1.0, 0.02, true);
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Render
            /// </summary>
            /// --------------------------------------------------------------------------
            public override void Render()
            {
                if (IsDone) return;

                w = dvWindow.MainBuffer.Width;
                h = dvWindow.MainBuffer.Height;
                pitch = dvWindow.MainBuffer.BufferPitch;
                data = dvWindow.MainBuffer.RawBuffer;

                for (int i = 0; i < 2000; i++)
                {
                    MoveOnePixel();
                }
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// 
            /// </summary>
            /// --------------------------------------------------------------------------
            private void MoveOnePixel()
            {
                int x1, y1, x2, y2;

                x2 = x1 = Rand(w);
                y2 = y1 = Rand(h);

                int r = Rand(8);

                switch (r)
                {
                    case 0:
                        x2 = x1 + 1;
                        y2 = y1;
                        break;
                    case 1:
                        x2 = x1 + 1;
                        y2 = y1 + 1;
                        break;
                    case 2:
                        x2 = x1;
                        y2 = y1 + 1;
                        break;
                    case 3:
                        x2 = x1 - 1;
                        y2 = y1 + 1;
                        break;
                    case 4:
                        x2 = x1 - 1;
                        y2 = y1;
                        break;
                    case 5:
                        x2 = x1 - 1;
                        y2 = y1 - 1;
                        break;
                    case 6:
                        x2 = x1;
                        y2 = y1 - 1;
                        break;
                    default:
                        x2 = x1 + 1;
                        y2 = y1 - 1;
                        break;
                }

                // Old code:  these two lines allow the diffusion to "wrap"
                //x2 = ((x2 + w) % w);
                //y2 = ((y2 + h) % h);
                if (x2 < 0 || y2 < 0 || x2 >= w || y2 >= h) return;
                int spot1 = x2 + y2 * pitch;
                int spot2 = x1 + y1 * pitch;
                if (spot1 >= data.Length) spot1 = data.Length - 1;
                if (spot2 >= data.Length) spot2 = data.Length - 1;


                ushort temp = data[spot1];
                data[spot1] = data[spot2];
                data[spot2] = temp;
            }
        }
    }
}
