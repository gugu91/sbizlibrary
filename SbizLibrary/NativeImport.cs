using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;

namespace Sbiz.Client
{
    public delegate void HandleSpecialKeys(KeyEventArgs e, int down_or_up);

    static class NativeImport
    {
        #region Keyboard
        #region Keyboard Native Const
        public const int WM_KEYDOWN = 0X100;
        public const int WM_KEYUP = 0X101;
        public const int WM_SYSKEYDOWN = 0X104;
        public const int WM_SYSKEYUP = 0X105;
        private const int WH_KEYBOARD_LL = 13;
        #endregion

        /* Code to Disable WinKey, Alt+Tab, Ctrl+Esc Starts Here */

        //Declaring Global objects     
        private static IntPtr ptrHook;
        private static LowLevelKeyboardProc objKeyboardProcess;
        private static HandleSpecialKeys _special_keys_handler;

        #region Hook Imports
        // Structure contain information about low-level keyboard input event 
        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public System.Windows.Forms.Keys key;
            public int scanCode;
            public int flags;
            public int time;
            public IntPtr extra;
        }
        //System level functions to be used for hook and unhook keyboard input  
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int id, LowLevelKeyboardProc callback, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hook);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hook, int nCode, IntPtr wp, IntPtr lp);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string name);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern short GetAsyncKeyState(System.Windows.Forms.Keys key);
        #endregion

        #region Hooking Methods
        private static IntPtr captureKey(int nCode, IntPtr wp, IntPtr lp) //wp information about the event(es. WM_SYSKEYDOWN), lp the KBDLLHOOKSTRUCT
        {
            if (nCode >= 0)
            {
                KBDLLHOOKSTRUCT objKeyInfo = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lp, typeof(KBDLLHOOKSTRUCT));

                // Disabling Windows keys TODO Add all required

                if (objKeyInfo.key == Keys.RWin || objKeyInfo.key == Keys.LWin || //Super
                    objKeyInfo.key == Keys.Tab && HasAltModifier(objKeyInfo.flags) || //Alt+Tab
                    objKeyInfo.key == Keys.Escape && (Control.ModifierKeys & Keys.Control) == Keys.Control) //Ctrl+Esc
                {
                    if (_special_keys_handler != null) _special_keys_handler(new KeyEventArgs(objKeyInfo.key | Control.ModifierKeys), wp.ToInt32());
                    return (IntPtr)1; // if 0 is returned then All the above keys will be enabled
                }
            }
            return CallNextHookEx(ptrHook, nCode, wp, lp);
        }

        static bool HasAltModifier(int flags)
        {
            return (flags & 0x20) == 0x20;
        }    
        #endregion

        #region Public Interface
        public static void HookSpecialKeys(HandleSpecialKeys del)
        {
            _special_keys_handler = del;
            ProcessModule objCurrentModule = Process.GetCurrentProcess().MainModule;
            objKeyboardProcess = new LowLevelKeyboardProc(captureKey);
            ptrHook = SetWindowsHookEx(WH_KEYBOARD_LL, objKeyboardProcess, GetModuleHandle(objCurrentModule.ModuleName), 0);
        }
        public static void UnhookSpecialKeys()
        {
            _special_keys_handler = null;
            UnhookWindowsHookEx(ptrHook);
        }
        #endregion

        /* Code to Disable WinKey, Alt+Tab, Ctrl+Esc Ends Here */
        #endregion

        #region Clipboard
        /// <summary>
        /// Sent when the contents of the clipboard have changed.
        /// </summary>
        public const int WM_CLIPBOARDUPDATE = 0x031D;

        #region Clipboard Imports
        /// <summary>
        /// Places the given window in the system-maintained clipboard format listener list.
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AddClipboardFormatListener(IntPtr hwnd);

        /// <summary>
        /// Removes the given window from the system-maintained clipboard format listener list.
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RemoveClipboardFormatListener(IntPtr hwnd);
        #endregion
        #endregion
    }
}
