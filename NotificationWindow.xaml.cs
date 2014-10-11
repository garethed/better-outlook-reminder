namespace BetterOutlookReminder
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using System.Windows.Threading;
    using WpfScreenHelper;

    /// <summary>
    ///     Interaction logic for NotificationWindow.xaml
    /// </summary>
    public partial class NotificationWindow : Window
    {
        private readonly DispatcherTimer hideTimer = new DispatcherTimer();
        private readonly NotificationWindow parent;
        private readonly List<NotificationWindow> children = new List<NotificationWindow>();

        private Point mouseDownPosition;

        public NotificationWindow(NotificationWindow parent)
        {
            this.parent = parent;
            InitializeComponent();
            if (this.parent == null)
            {
                hideTimer.Tick += HideTimerOnTick;
                new PositionPersister().Persist(this);
            }
        }

        public void Show(AppointmentGroup appointments)
        {
            Trace.WriteLine("Show " + appointments);

            if (appointments == null || !appointments.Next.Any())
            {
                return;
            }

            var distinctTimes = 1;
            var prev = appointments.Next.First();
            var first = prev;
            var prevWindow = this;
            Show(prev);

            foreach (var appointment in appointments.Next.Skip(1))
            {
                if (appointment.Start > prev.Start)
                {
                    if (distinctTimes++ > 2 && appointment.Start.Subtract(first.Start).TotalMinutes > 15)
                    {
                        break;
                    }
                }

                var child = new NotificationWindow(this);
                children.Add(child);
                child.Top = prevWindow.Top + prevWindow.Height + 5;
                child.Left = Left;
                child.Show(appointment);
                prevWindow = child;
            }

            EnsureChildrenOnScreen();
        }

        private void EnsureChildrenOnScreen()
        {
            if (children.Any())
            {
                var screen = Screen.FromPoint(new Point(Left, Top));
                var deltaBottom = (children.Last().Top + children.Last().Height) - screen.WorkingArea.Bottom + 10;
                if (deltaBottom > 0)
                {
                    var deltaTop = Math.Max(Top - screen.WorkingArea.Top - 10, 0);

                    Top -= Math.Min(deltaBottom, deltaTop);
                }
            }
        }

        private void Show(Appointment appointment)
        {
            Location.Text = Trim((appointment.Location != null ? (appointment.Location + ", ") : "")
                                 + (int)(appointment.End - appointment.Start).TotalMinutes + " mins", 50);

            People.Text = Trim(string.Join(", ", appointment.Recipients.Concat(new[] { appointment.Organizer }).Distinct()), 50);

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
                Background = new SolidColorBrush(
                    (appointment.Start > DateTime.Now.AddHours(0.5))
                        ? Color.FromRgb(210, 170, 30) : Color.FromRgb(203, 83, 0));
            }
            hideTimer.Start();
            Show();
        }

        private string Trim(string s, int length)
        {
            return s == null || s.Length < length ? s : s.Substring(0, length);
        }

        private void HideTimerOnTick(object sender, EventArgs eventArgs)
        {
            HideWindow();
        }

        private void HideWindow(bool fast = false)
        {
            hideTimer.Stop();
            var anim = new DoubleAnimation(0, new Duration(TimeSpan.FromSeconds(fast ? 0.2d : 1d)));
            anim.Completed += (s, _) =>
            {
                Hide();
                BeginAnimation(OpacityProperty, null);
                Opacity = 1;

                if (parent == null)
                {
                    foreach (var child in children)
                    {
                        child.Close();
                    }
                    children.Clear();
                }
            };
            BeginAnimation(OpacityProperty, anim);

            foreach (var child in children)
            {
                child.BeginAnimation(OpacityProperty, anim);
            }
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (mouseDownPosition == GetPosition())
            {
                if (parent != null)
                {
                    parent.HideWindow(true);
                }
                else
                {
                    HideWindow(true);
                }
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                mouseDownPosition = GetPosition();
                if (parent == null)
                {
                    DragMove();
                }
            }
        }

        private Point GetPosition()
        {
            return new Point(Left, Top);
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            if (parent == null)
            {
                for (var i = 0; i < children.Count; i++)
                {
                    children[i].Top = Top + (Height + 8) * (i + 1);
                    children[i].Left = Left;
                }
            }
        }
    }
}