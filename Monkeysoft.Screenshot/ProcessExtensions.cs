using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Monkeysoft.Screenshot
{
    internal static class ProcessExtensions
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        public static int GetWindowZOrder(this Process process)
        {
            const uint GW_HWNDPREV = 3;
            const uint GW_HWNDLAST = 1;

            IntPtr hwnd = process.MainWindowHandle;

            var zindex = 0;
            var hwndTmp = GetWindow(hwnd, GW_HWNDLAST);
            while (hwndTmp != IntPtr.Zero)
            {
                if (hwnd == hwndTmp)
                {
                    return zindex;
                }

                hwndTmp = GetWindow(hwndTmp, GW_HWNDPREV);
                zindex++;
            }

            return -1;
        }
    }
}
