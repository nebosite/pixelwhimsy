using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;

delegate IntPtr LowLevelProc(int nCode, IntPtr wParam, IntPtr lParam);

[StructLayout(LayoutKind.Sequential)]
public struct POINT
{
    public int X;
    public int Y;

    public POINT(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }

    public static implicit operator System.Drawing.Point(POINT p)
    {
        return new System.Drawing.Point(p.X, p.Y);
    }

    public static implicit operator POINT(System.Drawing.Point p)
    {
        return new POINT(p.X, p.Y);
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct MSLLHOOKSTRUCT
{
    public POINT pt;
    public uint mouseData;
    public int flags;
    public int time;
    public IntPtr dwExtraInfo;
}

    /// --------------------------------------------------------
    /// <summary>
    /// This class is for handling system events in a clean way.
    /// One of the main points of the program is to capture
    /// events that can cause the program to lose focus.
    /// </summary>
    /// --------------------------------------------------------
    class MySystemHandler
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;
        private const int WM_QUERYENDSESSION = 0x0011;
        private const int WM_POWERBROADCAST = 0x0218;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_KEYUP = 257;

        private const int WM_MOUSEFIRST =      0x0200;
        private const int WM_MOUSEMOVE =       0x0200;
        private const int WM_LBUTTONDOWN =     0x0201;
        private const int WM_LBUTTONUP =       0x0202;
        private const int WM_LBUTTONDBLCLK  =  0x0203;
        private const int WM_RBUTTONDOWN =     0x0204;
        private const int WM_RBUTTONUP  =      0x0205;
        private const int WM_RBUTTONDBLCLK  =  0x0206;
        private const int WM_MBUTTONDOWN =     0x0207;
        private const int WM_MBUTTONUP  =      0x0208;
        private const int WM_MBUTTONDBLCLK  =  0x0209;
        private const int WM_MOUSELAST =       0x0209;
        private const int WM_MOUSEWHEEL =      0x020A;


        private LowLevelProc shutdownProc = null;
        private LowLevelProc keyboardProc = null;
        private LowLevelProc mouseProc = null;
        private List<IntPtr> hookIDs = new List<IntPtr>();
        private Dictionary<Control, EventHandler> keyUpHandlers = new Dictionary<Control, EventHandler>();
        private Dictionary<Control, EventHandler> keyDownHandlers = new Dictionary<Control, EventHandler>();
        private Dictionary<Control, EventHandler> mouseMoveHandlers = new Dictionary<Control, EventHandler>();
        private Dictionary<Control, EventHandler> mouseUpHandlers = new Dictionary<Control, EventHandler>();
        private Dictionary<Control, EventHandler> mouseDownHandlers = new Dictionary<Control, EventHandler>();
        Control lastActiveControl = null;
        bool insideAnOwner = true;
        bool seizeNonOwnerEvents = false;


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
        LowLevelProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
        IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Constructor
        /// </summary>
        /// --------------------------------------------------------------------------
        public MySystemHandler(bool seizeNonOwnerEvents)
        {
            shutdownProc = this.ShutdownHookCallback;   
            keyboardProc = this.KeyboardHookCallback;
            mouseProc = this.MouseHookCallback;
            this.seizeNonOwnerEvents = seizeNonOwnerEvents;
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Add a key handler to call when a key is pressed
        /// </summary>
        /// --------------------------------------------------------------------------
        public void AddKeyHandlers(Control ownerControl, System.EventHandler upHandler, System.EventHandler downHandler)
        {
            if (upHandler != null) keyUpHandlers[ownerControl] = upHandler;
            if (downHandler != null) keyDownHandlers[ownerControl] = downHandler;
            lastActiveControl = ownerControl;
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Add mouse handlers
        /// </summary>
        /// <param name="mouseMoveHandler"></param>
        /// --------------------------------------------------------------------------
        public void AddMouseHandlers(Control ownerControl, 
            System.EventHandler mouseMoveHandler,
            System.EventHandler mouseUpHandler,
            System.EventHandler mouseDownHandler)
        {
            if (mouseMoveHandler != null) mouseMoveHandlers[ownerControl] = mouseMoveHandler;
            if (mouseUpHandler != null) mouseUpHandlers[ownerControl] = mouseUpHandler;
            if (mouseDownHandler != null) mouseDownHandlers[ownerControl] = mouseDownHandler;
            lastActiveControl = ownerControl;
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Reset the keyboard hooks
        /// </summary>
        /// --------------------------------------------------------------------------
        public void ClearHooks()
        {
            foreach (IntPtr hookID in hookIDs)
            {
                UnhookWindowsHookEx(hookID);
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Install keyboard hooks to trap system key events
        /// </summary>
        /// --------------------------------------------------------------------------
        public void SetHooks()
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                hookIDs.Add(SetWindowsHookEx(WH_MOUSE_LL, mouseProc, GetModuleHandle(curModule.ModuleName), 0));
                hookIDs.Add(SetWindowsHookEx(WH_KEYBOARD_LL, keyboardProc, GetModuleHandle(curModule.ModuleName), 0));
                hookIDs.Add(SetWindowsHookEx(WM_SYSKEYDOWN, keyboardProc, GetModuleHandle(curModule.ModuleName), 0));
                hookIDs.Add(SetWindowsHookEx(WM_QUERYENDSESSION, shutdownProc, GetModuleHandle(curModule.ModuleName), 0));
                hookIDs.Add(SetWindowsHookEx(WM_POWERBROADCAST, shutdownProc, GetModuleHandle(curModule.ModuleName), 0));
            }
        }

        private enum KeyAction
        {
            KeyDown,
            KeyUp
        }

        private Dictionary<Keys, bool> keyStates = new Dictionary<Keys, bool>();
        private KeysConverter keysConvert = new KeysConverter();

       

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Handle all keystroke messages here
        /// </summary>
        /// --------------------------------------------------------------------------
        private IntPtr ShutdownHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            // Don't allow System Shutdown events
            if (wParam == (IntPtr)WM_QUERYENDSESSION)
            {
                return new IntPtr(0);
            }

            //if (wParam == (IntPtr)WM_POWERBROADCAST)
            //{
            //    return new IntPtr(0);
            //}

            return new IntPtr(1);
        }


        /// --------------------------------------------------------------------------
        /// <summary>
        /// Handle all keystroke messages here
        /// </summary>
        /// --------------------------------------------------------------------------
        private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            Keys code = Keys.None;

            if (nCode >= 0 &&
                (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_KEYUP || wParam == (IntPtr)WM_SYSKEYDOWN))
            {
                int vkCode = Marshal.ReadInt32(lParam);

                //if (wParam == (IntPtr)WM_SYSKEYDOWN)
                //{
                //    code = Keys.Alt;
                //}
                //else 
                code = (Keys)vkCode;
                bool downEvent = (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN);
                if(insideAnOwner) HandleKey(downEvent ? KeyAction.KeyDown : KeyAction.KeyUp, code);
            }
            if (!seizeNonOwnerEvents && !insideAnOwner)
            {
                return new IntPtr(0);
            }
            else
            {
                return code == Keys.PrintScreen ? new IntPtr(0) : new IntPtr(1);
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Handle all keystroke messages here
        /// </summary>
        /// --------------------------------------------------------------------------
        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            MSLLHOOKSTRUCT mouseData = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
            MouseButtons mButtons = MouseButtons.None;
            Dictionary<Control, EventHandler> handlerList = mouseMoveHandlers;

            switch (wParam.ToInt32())
            {
                case WM_LBUTTONDOWN: mButtons = MouseButtons.Left; handlerList = mouseDownHandlers; break;
                case WM_MBUTTONDOWN: mButtons = MouseButtons.Middle; handlerList = mouseDownHandlers; break;
                case WM_RBUTTONDOWN: mButtons = MouseButtons.Right; handlerList = mouseDownHandlers; break;
                case WM_LBUTTONUP: mButtons = MouseButtons.Left; handlerList = mouseUpHandlers; break;
                case WM_MBUTTONUP: mButtons = MouseButtons.Middle; handlerList = mouseUpHandlers; break;
                case WM_RBUTTONUP: mButtons = MouseButtons.Right; handlerList = mouseUpHandlers; break;
                default: break;
            }


            insideAnOwner = false;
            if (seizeNonOwnerEvents) insideAnOwner = true;

            foreach (Control owner in handlerList.Keys)
            {
                int borderSize = (owner.Width - owner.ClientRectangle.Width) / 2;
                int menuGripSize = (owner.Height - owner.ClientRectangle.Height - borderSize);

                int mouseX = mouseData.pt.X - owner.Location.X - borderSize;
                int mouseY = mouseData.pt.Y - owner.Location.Y - menuGripSize;

                //Point formClientScreenLocation =
                //  parent.PointToScreen(
                //    new Point(parent.ClientRectangle.Left, parent.ClientRectangle.Top));
                //int x = formClientScreenLocation.X - parent.DesktopLocation.X + this.Location.X;
                //int y = formClientScreenLocation.Y - parent.DesktopLocation.Y + this.Location.Y;
                //region.Translate(x, y);              

                if (mouseX >= 0 && mouseX < owner.ClientRectangle.Width && mouseY >= 0 && mouseY < owner.ClientRectangle.Height)
                {
                    lastActiveControl = owner;
                    insideAnOwner = true;
                }

                int delta = 0;

                if (wParam.ToInt32() == (int)WM_MOUSEWHEEL)
                {
                    delta = (int)((short)(mouseData.mouseData >> 16)/ 24);
                }

                MouseEventArgs mouseArgs = new MouseEventArgs(mButtons, 0, mouseX, mouseY, delta);
                if(insideAnOwner) handlerList[owner](null, mouseArgs);
            }


            if (!seizeNonOwnerEvents && !insideAnOwner)
            {
                return new IntPtr(0);
            }
            else
            {
                return (wParam.ToInt32() == (int)WM_MOUSEMOVE) ? new IntPtr(0) : new IntPtr(1);
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Shuffle keystrokes off to other event handlers here.
        /// </summary>
        /// --------------------------------------------------------------------------
        private void HandleKey(KeyAction action, Keys code)
        {
            keyStates[code] = (action == KeyAction.KeyDown);
            KeyEventArgs args = new KeyEventArgs(code);
          
            Dictionary<Control, EventHandler> handlers = keyUpHandlers ;
            if (action == KeyAction.KeyDown) handlers = keyDownHandlers;

            if (handlers != null && lastActiveControl != null && handlers.ContainsKey(lastActiveControl))
            {
                handlers[lastActiveControl](null, args);
            }
        }
    }

