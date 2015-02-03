namespace BetterOutlookReminder
{
    using BetterOutlookReminder.Properties;
    using System;
    using System.Linq;
    using System.Windows;
    using WpfScreenHelper;

    public class PositionPersister
    {
        private Window window;
        private bool loaded;

        private bool BoundsAreOnScreen
        {
            get
            {
                var bounds = new Rect(Settings.Default.Left, Settings.Default.Top, window.Width, window.Height);
                return Screen.AllScreens.Any(s => s.Bounds.Contains(bounds));
            }
        }

        public void Persist(Window window)
        {
            this.window = window;

            window.Activated += form_Load;
            window.LocationChanged += form_Save;

            form_Load(this, EventArgs.Empty);
        }

        private void form_Save(object sender, EventArgs e)
        {
            if (loaded)
            {
                Settings.Default.Top = window.Top;
                Settings.Default.Left = window.Left;
                Settings.Default.Save();
            }
        }

        private void form_Load(object sender, EventArgs e)
        {
            if (BoundsAreOnScreen)
            {
                window.Top = Settings.Default.Top;
                window.Left = Settings.Default.Left;
            }
            loaded = true;
        }
    }
}
