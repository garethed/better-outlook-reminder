using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;

namespace BetterOutlookReminder
{
    /// <summary>
    /// Owns the broker's sign-in dialogs. WAM needs an owner HWND, and auth can fire from a
    /// background poll, so we give it a window of our own rather than whatever app happened to be
    /// in the foreground. The handle is created once, on the UI thread, at startup: MSAL asks for
    /// it again from its own threads mid-sign-in, and marshalling back to a UI thread that is
    /// blocked inside the broker call deadlocks.
    /// </summary>
    internal static class AuthDialogOwner
    {
        private static Window window;

        public static IntPtr Handle { get; private set; }

        public static void Initialize()
        {
            window = new Window
            {
                Width = 0,
                Height = 0,
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = false,
                ShowActivated = false,
                Visibility = Visibility.Hidden
            };

            Handle = new WindowInteropHelper(window).EnsureHandle();
            Trace.WriteLine("AuthDialogOwner.handle " + Handle);
        }
    }
}
