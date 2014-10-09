namespace BetterOutlookReminder
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using System.Windows.Threading;

    /// <summary>
    ///     Interaction logic for NotificationWindow.xaml
    /// </summary>
    public partial class NotificationWindow : Window
    {
        private readonly DispatcherTimer hideTimer = new DispatcherTimer();
        private Appointment appointment;

        private Point mouseDownPosition;

        public NotificationWindow()
        {
            InitializeComponent();
            hideTimer.Tick += HideTimerOnTick;
            new PositionPersister().Persist(this);
        }

        public void Show(Appointment appointment)
        {
            if (appointment == null)
            {
                return;
            }

            this.appointment = appointment;

            Location.Text = appointment.Location;
            People.Text = string.Join(", ", appointment.Recipients.Concat(new[] { appointment.Organizer }).Distinct());

            if (appointment.HasStarted)
            {
                hideTimer.Interval = appointment.End - DateTime.Now;
                Heading.Text = "NOW: " + appointment.Subject;
                Background = new SolidColorBrush(Color.FromRgb(203, 0, 0));
            }
            else
            {
                Heading.Text = "Next: " + appointment.Start.ToString("HH:mm") + " - " + appointment.Subject;
                hideTimer.Interval = TimeSpan.FromSeconds(10);
                Background = new SolidColorBrush(Color.FromRgb(203, 83, 0));
            }
            hideTimer.Start();
            Show();
        }

        public void ShowIfAvailable()
        {
            Show(appointment);
        }

        private void HideTimerOnTick(object sender, EventArgs eventArgs)
        {
            HideWindow();
        }

        private void HideWindow()
        {
            var anim = new DoubleAnimation(0, new Duration(TimeSpan.FromSeconds(1)));
            anim.Completed += (s, _) =>
            {
                Hide();
                BeginAnimation(OpacityProperty, null);
                Opacity = 1;
            };
            BeginAnimation(OpacityProperty, anim);
            Hide();
            hideTimer.Stop();
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (mouseDownPosition == GetPosition())
            {
                HideWindow();
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                mouseDownPosition = GetPosition();
                DragMove();
            }
        }

        private Point GetPosition()
        {
            return new Point(Left, Top);
        }
    }
}