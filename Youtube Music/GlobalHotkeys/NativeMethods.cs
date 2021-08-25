using System;
using System.Runtime.InteropServices;

namespace GKeys
{
    class NativeMethods
    {
        [DllImport("User32.dll")]
        public static extern bool RegisterHotKey([In] IntPtr hWnd, [In] int id, [In] int fsModifiers, [In] int vk);
        [DllImport("User32.dll")]
        public static extern bool UnregisterHotKey([In] IntPtr hWnd, [In] int id);

        public const int WM_HOTKEY = 0x0312;
    }
}
