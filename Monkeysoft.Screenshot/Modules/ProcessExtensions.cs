using Monkeysoft.Screenshot.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Monkeysoft.Screenshot.Modules
{
    internal static class ProcessExtensions
    {
        public static bool IsWindowValidForCapture(this IntPtr hwnd)
        {
            if (hwnd.ToInt32() == 0)
            {
                return false;
            }

            if (!NativeMethods.IsWindowVisible(hwnd))
            {
                return false;
            }

            if (NativeMethods.GetAncestor(hwnd, GetAncestorFlags.GETROOT) != hwnd)
            {
                return false;
            }

            if (!NativeMethods.IsWindowEnabled(hwnd))
            {
                return false;
            }

            var hrTemp = NativeMethods.DwmGetWindowAttribute(hwnd, (int)DWMWINDOWATTRIBUTE.DWMWA_CLOAKED, out bool cloaked, Marshal.SizeOf<bool>());
            if (hrTemp == 0 && cloaked)
            {
                return false;
            }

            return true;
        }
    }
}
