using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using DirectVarmint;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace PixelWhimsy
{
    public partial class Slate
    {
        Dictionary<Keys, char[]> keyTranslations = new Dictionary<Keys, char[]>();
        Dictionary<char, Keys> keyReverseTranslations = new Dictionary<char, Keys>();
        string whimsyPicsPath = "";
        string whimsyDataPath = "";

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Save the current screen to a disk file
        /// </summary>
        /// --------------------------------------------------------------------------
        private void SaveCurrentScreen()
        {
            int w = dvWindow.MainBuffer.Width;
            int h = dvWindow.MainBuffer.Height;
            Utilities.SetupDataFolders(out whimsyPicsPath, out whimsyDataPath);

            // Save the current screen to a png file
            Bitmap saveMe = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            BitmapData data = saveMe.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int totalBytes = data.Stride * saveMe.Height;

            byte[] tempData = new byte[totalBytes];

            for (int y = 0; y < dvWindow.MainBuffer.Height; y++)
            {
                int writeSpot = y * data.Stride;
                for (int x = 0; x < dvWindow.MainBuffer.Width; x++)
                {
                    int readSpot = x + y * w;
                    ushort localIndex = currentScreen.FrameData[0][readSpot];
                    uint localColor = GlobalState.Palette[localIndex];
                    tempData[writeSpot + x * 3 + 2] = (byte)((localColor >> 16) & 0xff);
                    tempData[writeSpot + x * 3 + 1] = (byte)((localColor >> 8) & 0xff);
                    tempData[writeSpot + x * 3 + 0] = (byte)((localColor >> 0) & 0xff);
                }
            }
            System.Runtime.InteropServices.Marshal.Copy(tempData, 0, data.Scan0, totalBytes);
            saveMe.UnlockBits(data);

            string fullName = GetPictureFileName(screenSlot);
            SafeDelete(fullName);
            try
            {
                saveMe.Save(fullName, ImageFormat.Png);
            }
            catch (Exception) { }


            // Save the screen to a file that preserves animated palette
            saveMe = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format16bppRgb565);
            data = saveMe.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, PixelFormat.Format16bppRgb565);
            totalBytes = data.Stride * saveMe.Height;
            
            tempData = new byte[totalBytes];

            for (int y = 0; y < dvWindow.MainBuffer.Height; y++)
            {
                int writeSpot = y * data.Stride;
                for (int x = 0; x < dvWindow.MainBuffer.Width; x++)
                {
                    int readSpot = x + y * w;
                    ushort localIndex = currentScreen.FrameData[0][readSpot];

                    tempData[writeSpot + x * 2 + 1] = (byte)((localIndex >> 8) & 0xff);
                    tempData[writeSpot + x * 2 + 0] = (byte)((localIndex >> 0) & 0xff);
                }
            }
            System.Runtime.InteropServices.Marshal.Copy(tempData, 0, data.Scan0, totalBytes);
            saveMe.UnlockBits(data);

            fullName = GetDataFileName(screenSlot);
            SafeDelete(fullName);
            try
            {
                saveMe.Save(fullName, ImageFormat.Png);
            }
            catch (Exception) { }

            // make a new thumbnail
            for (int x = 0; x < w / 4; x++)
            {
                for (int y = 0; y < h / 4; y++)
                {
                    dvWindow.OverlayBuffer.DrawPixel(currentScreen.FrameData[0][(y * 4 * w) + x * 4], x, y);
                }
            }
            savedScreens[screenSlot] = dvWindow.OverlayBuffer.CaptureSprite(0, 0, w / 4, h / 4);

            MediaBag.Play(SoundID.Click_Camera1);
            dvWindow.OverlayBuffer.Clear(0);
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Safely delete a file that might be in use by another process.
        /// </summary>
        /// --------------------------------------------------------------------------
        private static void SafeDelete(string fullName)
        {
            DateTime doneTrying = DateTime.Now.AddSeconds(4);
            while (DateTime.Now < doneTrying && File.Exists(fullName))
            {
                try
                {
                    File.Delete(fullName);
                }
                catch (IOException) { }
                Thread.Sleep(100);
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Load a picture into the mainbuffer
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="factor"></param>
        /// --------------------------------------------------------------------------
        void LoadPicture(string fileName, PixelBuffer buffer, int factor)
        {
            if (!File.Exists(fileName)) return;
            Bitmap readMe = new Bitmap(fileName);
            BitmapData data = readMe.LockBits(new Rectangle(0, 0, readMe.Width, readMe.Height), ImageLockMode.ReadWrite, PixelFormat.Format16bppRgb565);
            int totalBytes = readMe.Height * data.Stride;
            byte[] tempData = new byte[totalBytes];
            System.Runtime.InteropServices.Marshal.Copy( data.Scan0, tempData, 0, totalBytes);

            double bmxfactor = readMe.Width / (double)buffer.Width;
            double bmyfactor = readMe.Height / (double)buffer.Height;

            for (int y = 0; y < buffer.Height / factor; y++)
            {
                for (int x = 0; x < buffer.Width/factor; x++)
                {
                    int bmx = (int)(x * factor * bmxfactor);
                    int bmy = (int)(y * factor * bmyfactor);
                    int readSpot = bmy * data.Stride + bmx * 2;
                    ushort color = (ushort)((tempData[readSpot + 1] << 8) + tempData[readSpot + 0]);
                    buffer.DrawPixel(color, x, y );
                }
            }

            readMe.UnlockBits(data);

        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Get the name for a data file
        /// </summary>
        /// --------------------------------------------------------------------------
        public string GetDataFileName(int slot)
        {
            string fileName = "slot" + slot.ToString("00") + ".png";
            return Path.Combine(whimsyDataPath, fileName);
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Get the name for a picture file
        /// </summary>
        /// --------------------------------------------------------------------------
        public string GetPictureFileName(int slot)
        {
            string fileName = "picture" + slot.ToString("00") + ".png";
            return Path.Combine(whimsyPicsPath, fileName);
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Commit the current screen action
        /// </summary>
        /// --------------------------------------------------------------------------
        private void ScreenCommit()
        {
            switch (screenMode)
            {
                case ScreenMode.Normal: HandleMouse_Normal(); break;
                case ScreenMode.SaveScreen:
                    if(Settings.Registered) SaveCurrentScreen();
                    EndScreenIO();
                    break;
                case ScreenMode.LoadScreen:
                    EndScreenIO();
                    if (Settings.Registered)
                    {
                        LoadPicture(GetDataFileName(screenSlot), dvWindow.MainBuffer, 1);
                        MediaBag.Play(SoundID.Slide_Laugh);
                    }
                    break;
                default: break;
            }
        }


    }
}
