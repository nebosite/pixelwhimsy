
using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace DirectVarmint
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DEVMODE1
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmDeviceName;
        public short dmSpecVersion;
        public short dmDriverVersion;
        public short dmSize;
        public short dmDriverExtra;
        public int dmFields;

        public short dmOrientation;
        public short dmPaperSize;
        public short dmPaperLength;
        public short dmPaperWidth;

        public short dmScale;
        public short dmCopies;
        public short dmDefaultSource;
        public short dmPrintQuality;
        public short dmColor;
        public short dmDuplex;
        public short dmYResolution;
        public short dmTTOption;
        public short dmCollate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmFormName;
        public short dmLogPixels;
        public short dmBitsPerPel;
        public int dmPelsWidth;
        public int dmPelsHeight;

        public int dmDisplayFlags;
        public int dmDisplayFrequency;

        public int dmICMMethod;
        public int dmICMIntent;
        public int dmMediaType;
        public int dmDitherType;
        public int dmReserved1;
        public int dmReserved2;

        public int dmPanningWidth;
        public int dmPanningHeight;
    };

    [Serializable]
    struct ABC
    {
        public int abcA;  // Distance to add to the current position before drawing character
        public uint abcB; // Width of the drawn portion
        public int abcC;  // distance to add to the current position to provide white space to the right of the character 

        public ABC(int a, int b, int c)
        {
            abcA = a;
            abcB = (uint)b;
            abcC = c;
        }
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }


    /// --------------------------------------------------------------------------
    /// <summary>
    /// User32 access funtions
    /// </summary>
    /// --------------------------------------------------------------------------
    class User32
    {
        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern bool IsWindow(IntPtr hWnd);

        [DllImport("gdi32.dll")]
        public static extern bool GetCharABCWidths(IntPtr hdc, uint uFirstChar,
           uint uLastChar, [Out] ABC[] lpabc);

        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("user32.dll")]
        public static extern int EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE1 devMode);

        [DllImport("user32.dll")]
        public static extern int ChangeDisplaySettings(ref DEVMODE1 devMode, int flags);

        public const int ENUM_CURRENT_SETTINGS = -1;
        public const int CDS_UPDATEREGISTRY = 0x01;
        public const int CDS_TEST = 0x02;
        public const int DISP_CHANGE_SUCCESSFUL = 0;
        public const int DISP_CHANGE_RESTART = 1;
        public const int DISP_CHANGE_FAILED = -1;

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Change the monitor resolution for a screen
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// --------------------------------------------------------------------------
        public static void ChangeResolution(Screen screen, int newWidth, int newHeight)
        {
            DEVMODE1 deviceMode = new DEVMODE1();
            deviceMode.dmDeviceName = new String(new char[32]);
            deviceMode.dmFormName = new String(new char[32]);
            deviceMode.dmSize = (short)Marshal.SizeOf(deviceMode);

            if (0 != EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref deviceMode))
            {
                deviceMode.dmPelsWidth = newWidth;
                deviceMode.dmPelsHeight = newHeight;

                int returnValue = ChangeDisplaySettings(ref deviceMode, CDS_TEST);

                if (returnValue == DISP_CHANGE_FAILED)
                {
                    throw new ApplicationException("Could not change to desired resolution.");
                }
                else
                {
                    returnValue = ChangeDisplaySettings(ref deviceMode, CDS_UPDATEREGISTRY);


                    if (returnValue != DISP_CHANGE_SUCCESSFUL)
                    {
                        throw new ApplicationException("Could not change resolution.  Code = " + returnValue);
                    }
                }
            }
            else
            {
                throw new ApplicationException("Could not enumerate display settings");
            }
        }
    }
}