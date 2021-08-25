using System;
using System.Runtime.InteropServices;

namespace Youtube_Music
{
    class NativeMethods
    {
        public const int HWND_BROADCAST = 0xffff;
        //public const int WM_HOTKEY = 0x0312;
        public static readonly int WM_SHOWME = RegisterWindowMessage("WM_SHOWME");

        [DllImport("user32")]
        public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);
        [DllImport("user32.dll")]
        public static extern bool SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);
        
        [DllImport("user32", CharSet = CharSet.Unicode)]
        public static extern int RegisterWindowMessage(string message);
    }
}
