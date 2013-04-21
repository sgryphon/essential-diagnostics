using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Essential.Diagnostics
{
    static class NativeMethods
    {
        public static HandleRef NullHandleRef = new HandleRef(null, IntPtr.Zero);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA5122:PInvokesShouldNotBeSafeCriticalFxCopRule", Justification = "Rule should only apply to .NET 4.0. See http://connect.microsoft.com/VisualStudio/feedback/details/729254/bogus-ca5122-warning-about-p-invoke-declarations-should-not-be-safe-critical")]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern int GetModuleFileName(HandleRef hModule, StringBuilder buffer, int length);
    }
}
