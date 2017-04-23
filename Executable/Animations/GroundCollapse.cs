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
        /// Scorched-earth style ground collapse
        /// </summary>
        /// --------------------------------------------------------------------------
        public class GroundCollapse : Animation
        {
            SoundPlayer.SoundInstance sound;
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
            public GroundCollapse(DVWindow window)
                : base(window)
            {
                sound = MediaBag.Play(SoundID.Loop_Rumble2, 2, 0.1, true);
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Render
            /// </summary>
            /// --------------------------------------------------------------------------
            public override void Render()
            {
                if (IsDone) return;

                ushort[] buffer = dvWindow.MainBuffer.RawBuffer;
                int bufferPitch = dvWindow.MainBuffer.BufferPitch;

                for (int x = 0; x < dvWindow.MainBuffer.Width; x++)
                {
                    int y = dvWindow.MainBuffer.Height - 1;
                    int spotTop = y * bufferPitch + x;

                    // Find the top
                    for (; y >= 0; y--)
                    {
                        if (buffer[spotTop] == 0) break;
                        spotTop -= bufferPitch;
                    }

                    y--;
                    int spotCopy = y * bufferPitch + x;

                    // Handle singletons
                    for (; y >= 0; y--)
                    {
                        if (buffer[spotCopy] == 0) break;
                        buffer[spotTop] = buffer[spotCopy];
                        buffer[spotCopy] = 0;
                        spotTop = spotCopy;
                        spotCopy -= bufferPitch;
                    }

                    // Drop by 2
                    y--;
                    spotCopy = y * bufferPitch + x;
                    for (; y >= 0; y--)
                    {
                        buffer[spotTop] = buffer[spotCopy];
                        buffer[spotCopy] = 0;
                        spotTop -= bufferPitch;
                        spotCopy -= bufferPitch;
                    }

                }
            }
        }
    }
}
