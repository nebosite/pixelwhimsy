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
        /// decay/blur the literal palette indexes
        /// </summary>
        /// --------------------------------------------------------------------------
        public class ScreenDecay : Animation
        {
            SoundPlayer.SoundInstance sound;

            /// <summary>
            /// stop the sound
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
                    ushort[] buffer = dvWindow.MainBuffer.RawBuffer;

                    for (int i = 0; i < buffer.Length; i++)
                    {
                        uint c = buffer[i];
                        buffer[i] = dvWindow.MainBuffer.GetPaletteColor(GlobalState.Palette[c]);
                    }
                    GlobalState.SetRGBPalette();
                    base.IsDone = value;
                }
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Constructor
            /// </summary>
            /// --------------------------------------------------------------------------
            public ScreenDecay(DVWindow window)
                : base(window)
            {
                sound = MediaBag.Play(SoundID.Loop_Wahwah, .7, .03, true);
                GlobalState.SetSmoothPalette();
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Render
            /// </summary>
            /// --------------------------------------------------------------------------
            public override void Render() 
            {
                if (IsDone)
                {
                    return;
                }

                ushort[] data = dvWindow.MainBuffer.RawBuffer;
                int pitch = dvWindow.MainBuffer.BufferPitch;
                int height = dvWindow.MainBuffer.Height;
                int width = dvWindow.MainBuffer.Width;

                ushort[] line0 = new ushort[pitch + 2];
                ushort[] line1 = new ushort[pitch + 2];
                ushort[] line2 = new ushort[pitch + 2];
                ushort[] tempLine = null;

                Array.Copy(data, 0, line1, 1, pitch);
                Array.Copy(data, pitch, line2, 1, pitch);

                for (int y = 0; y < height; y++)
                {
                    int writeSpot = y * pitch;
                    int total;

                    for (int x = 1; x <= width; x++)
                    {
                        total = line0[x] + line1[x-1] + line1[x]*4 + line1[x+1] + line2[x];
                        
                        data[writeSpot++] = (ushort)((total * 100)/810);
                    }

                    tempLine = line0;
                    line0 = line1;
                    line1 = line2;
                    line2 = tempLine;
                    if (y+2 < height - 1)
                    {
                        Array.Copy(data, pitch * (y +2), line2, 1, pitch);
                    }
                    else
                    {
                        for (int x = 0; x < line2.Length; x++) line2[x] = 0;
                    }
                }
            }
        }
    }
}
