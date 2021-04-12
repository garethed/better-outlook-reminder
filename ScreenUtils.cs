using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WpfScreenHelper;

namespace BetterOutlookReminder
{
    static class ScreenUtils
    {

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(HandleRef hWnd, [In, Out] ref RECT rect);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();


        private static bool IsAnyProgramFullScreen => WpfScreenHelper.Screen.AllScreens.Any(DoesScreenHaveFullScreenProgram);
        
        private static bool DoesScreenHaveFullScreenProgram(Screen screen)
        { 

            RECT rect = new RECT();
            IntPtr hWnd = (IntPtr)GetForegroundWindow();


            GetWindowRect(new HandleRef(null, hWnd), ref rect);

            /* in case you want the process name:
            uint procId = 0;
            GetWindowThreadProcessId(hWnd, out procId);
            var proc = System.Diagnostics.Process.GetProcessById((int)procId);
            Console.WriteLine(proc.ProcessName);
            */


            return (screen.Bounds.Width == (rect.right - rect.left) && screen.Bounds.Height == (rect.bottom - rect.top));
        }

        private static bool inFullScreen;

        public static event EventHandler<EventArgs> PausedForFullScreen;
        public static event EventHandler<EventArgs> ResumedFromFullScreen;
        public static DateTime lastNotification = DateTime.MinValue;

        public static bool CanShowPopup()
        {
            if (IsAnyProgramFullScreen)
            {
                if (!inFullScreen)
                {
                    inFullScreen = true;
                    if ((DateTime.Now - lastNotification).TotalMinutes > 5)
                    {
                        PausedForFullScreen?.Invoke(null, EventArgs.Empty);
                        lastNotification = DateTime.Now;
                    }
                }

                return false;

            }
            else if (inFullScreen)
            {
                inFullScreen = false;

                if ((DateTime.Now - lastNotification).TotalMinutes > 5)
                {
                    ResumedFromFullScreen?.Invoke(null, EventArgs.Empty);
                    lastNotification = DateTime.Now;
                }
            }

            return true;
        }

    }
}
