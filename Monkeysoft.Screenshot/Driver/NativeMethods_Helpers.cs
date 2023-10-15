#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright (c) 2007-2023 ShareX Team

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)

using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace Monkeysoft.Screenshot.Driver
{
    public static partial class NativeMethods
    {
        public static string? GetForegroundWindowText()
        {
            IntPtr handle = GetForegroundWindow();
            return GetWindowText(handle);
        }

        public static string? GetWindowText(IntPtr handle)
        {
            if (handle.ToInt32() > 0)
            {
                try
                {
                    int length = GetWindowTextLength(handle);

                    if (length > 0)
                    {
                        StringBuilder sb = new StringBuilder(length + 1);

                        if (GetWindowText(handle, sb, sb.Capacity) > 0)
                        {
                            return sb.ToString();
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Write(e);
                }
            }

            return null;
        }

        public static Process? GetForegroundWindowProcess()
        {
            IntPtr handle = GetForegroundWindow();
            return GetProcessByWindowHandle(handle);
        }

        public static string? GetForegroundWindowProcessName()
        {
            using (Process? process = GetForegroundWindowProcess())
            {
                return process?.ProcessName;
            }
        }

        public static Process? GetProcessByWindowHandle(IntPtr hwnd)
        {
            if (hwnd.ToInt32() > 0)
            {
                try
                {
                    GetWindowThreadProcessId(hwnd, out uint processID);
                    return Process.GetProcessById((int)processID);
                }
                catch (Exception e)
                {
                    Debug.Write(e);
                }
            }

            return null;
        }

        public static string? GetClassName(IntPtr handle)
        {
            if (handle.ToInt32() > 0)
            {
                StringBuilder sb = new StringBuilder(256);

                if (GetClassName(handle, sb, sb.Capacity) > 0)
                {
                    return sb.ToString();
                }
            }

            return null;
        }

        public static IntPtr GetClassLongPtrSafe(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size > 4)
            {
                return GetClassLongPtr(hWnd, nIndex);
            }

            return new IntPtr(GetClassLong(hWnd, nIndex));
        }

        public static IntPtr GetWindowLong(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 4)
            {
                return GetWindowLong32(hWnd, nIndex);
            }

            return GetWindowLongPtr64(hWnd, nIndex);
        }

        public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 4)
            {
                return SetWindowLongPtr32(hWnd, nIndex, dwNewLong);
            }

            return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
        }

        private static Icon? GetBigApplicationIcon(IntPtr handle)
        {
            SendMessageTimeout(handle, (int)WindowsMessages.GETICON, NativeConstants.ICON_BIG, 0, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 1000, out IntPtr iconHandle);

            if (iconHandle == IntPtr.Zero)
            {
                iconHandle = GetClassLongPtrSafe(handle, NativeConstants.GCL_HICON);
            }

            if (iconHandle != IntPtr.Zero)
            {
                return Icon.FromHandle(iconHandle);
            }

            return null;
        }

        public static bool GetBorderSize(IntPtr handle, out Size size)
        {
            WINDOWINFO wi = WINDOWINFO.Create();
            bool result = GetWindowInfo(handle, ref wi);

            if (result)
            {
                size = new Size((int)wi.cxWindowBorders, (int)wi.cyWindowBorders);
            }
            else
            {
                size = Size.Empty;
            }

            return result;
        }

        public static bool GetWindowRegion(IntPtr hWnd, out Region region)
        {
            IntPtr hRgn = CreateRectRgn(0, 0, 0, 0);
            RegionType regionType = (RegionType)GetWindowRgn(hWnd, hRgn);
            region = Region.FromHrgn(hRgn);
            return regionType != RegionType.ERROR && regionType != RegionType.NULLREGION;
        }

        public static bool GetExtendedFrameBounds(IntPtr handle, out Rectangle rectangle)
        {
            int result = DwmGetWindowAttribute(handle, (int)DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS, out RECT rect, Marshal.SizeOf(typeof(RECT)));
            rectangle = rect;
            return result == 0;
        }

        public static bool GetNCRenderingEnabled(IntPtr handle)
        {
            int result = DwmGetWindowAttribute(handle, (int)DWMWINDOWATTRIBUTE.DWMWA_NCRENDERING_ENABLED, out bool enabled, sizeof(bool));
            return result == 0 && enabled;
        }

        public static void SetNCRenderingPolicy(IntPtr handle, DWMNCRENDERINGPOLICY renderingPolicy)
        {
            int attrValue = (int)renderingPolicy;
            DwmSetWindowAttribute(handle, (int)DWMWINDOWATTRIBUTE.DWMWA_NCRENDERING_POLICY, ref attrValue, sizeof(int));
        }

        public static void SetWindowCornerPreference(IntPtr handle, DWM_WINDOW_CORNER_PREFERENCE cornerPreference)
        {
            int attrValue = (int)cornerPreference;
            DwmSetWindowAttribute(handle, (int)DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE, ref attrValue, sizeof(int));
        }

        public static Rectangle GetWindowRect(IntPtr handle)
        {
            GetWindowRect(handle, out RECT rect);
            return rect;
        }

        public static Rectangle GetClientRect(IntPtr handle)
        {
            GetClientRect(handle, out RECT rect);
            Point position = rect.Location;
            ClientToScreen(handle, ref position);
            return new Rectangle(position, rect.Size);
        }

        public static Rectangle MaximizedWindowFix(IntPtr handle, Rectangle windowRect)
        {
            if (GetBorderSize(handle, out Size size))
            {
                windowRect = new Rectangle(windowRect.X + size.Width, windowRect.Y + size.Height, windowRect.Width - (size.Width * 2), windowRect.Height - (size.Height * 2));
            }

            return windowRect;
        }

        /// <summary>
        /// .NET replacement of mmioFOURCC macros. Converts four characters to code.
        /// </summary>
        ///
        /// <param name="str">Four characters string.</param>
        ///
        /// <returns>Returns the code created from provided characters.</returns>
        public static int mmioFOURCC(string str)
        {
            return (
                (byte)(str[0]) |
                ((byte)(str[1]) << 8) |
                ((byte)(str[2]) << 16) |
                ((byte)(str[3]) << 24));
        }

        /// <summary>
        /// Inverse to <see cref="mmioFOURCC"/>. Converts code to fout characters string.
        /// </summary>
        ///
        /// <param name="code">Code to convert.</param>
        ///
        /// <returns>Returns four characters string.</returns>
        public static string decode_mmioFOURCC(int code)
        {
            char[] chs = new char[4];

            for (int i = 0; i < 4; i++)
            {
                chs[i] = (char)(byte)((code >> (i << 3)) & 0xFF);
                if (!char.IsLetterOrDigit(chs[i]))
                    chs[i] = ' ';
            }
            return new string(chs);
        }

        public static float GetScreenScalingFactor()
        {
            float scalingFactor;

            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                IntPtr desktop = g.GetHdc();
                int LogicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.VERTRES);
                int PhysicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES);
                int logpixelsy = GetDeviceCaps(desktop, (int)DeviceCap.LOGPIXELSY);
                float screenScalingFactor = (float)PhysicalScreenHeight / LogicalScreenHeight;
                float dpiScalingFactor = logpixelsy / 96f;
                scalingFactor = Math.Max(screenScalingFactor, dpiScalingFactor);
                g.ReleaseHdc(desktop);
            }

            return scalingFactor;
        }
    }
}