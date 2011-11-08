using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Essential.Diagnostics
{
    static class NativeMethods
    {
        public static HandleRef NullHandleRef = new HandleRef(null, IntPtr.Zero);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern int GetModuleFileName(HandleRef hModule, StringBuilder buffer, int length);
    }
}
