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
    /// Interaction logic for NotificationWindow.xaml
    /// </summary>
    public partial class NotificationWindow : Window
    {
        private static Color BackgroundOverdue = Color.FromRgb(203, 0, 0);
        private static Color BackgroundSoon = Color.FromRgb(203, 83, 0);
        private static Color BackgroundLater = Color.FromRgb(210, 170, 30);

        private readonly DispatcherTimer hideTimer = new DispatcherTimer();
        private readonly DispatcherTimer updateTimer = new DispatcherTimer();
        private readonly NotificationWindow parent;
        private readonly List<NotificationWindow> children = new List<NotificationWindow>();

        private Appointment appointment;

        private Point mouseDownPosition;
        public NotificationWindow(NotificationWindow parent)
        {
            this.parent = parent;
            InitializeComponent();
            if (this.parent == null)
            {
                hideTimer.Tick += HideTimerOnTick;
                updateTimer.Tick += UpdateTimerOnTick;
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

        private Color GetBackground(Appointment appointment) {
            if (appointment.HasStarted) {
                return BackgroundOverdue;
            }
            else if (appointment.Start > DateTime.Now.AddHours(0.5)) {
                return BackgroundLater;
            }
            return BackgroundSoon;
        }

        private void Show(Appointment appointment)
        {
            this.appointment = appointment;

            Location.Text = Trim((!string.IsNullOrEmpty(appointment.Location) ? (appointment.Location + ", ") : "")
                                 + (int)(appointment.End - appointment.Start).TotalMinutes + " mins", 50);

            People.Text = Trim(string.Join(", ", appointment.Recipients.Concat(new[] { appointment.Organizer }).Distinct()), 50);

            Heading.Text = appointment.Start.ToString("HH:mm") + " - " + appointment.Subject;

            if (appointment.ButtonLink != null)
            {
                ButtonJoin.Content = appointment.ButtonText;
            }

            var interval = appointment.End - DateTime.Now;
            Background = new SolidColorBrush(GetBackground(appointment));

            if (appointment.HasStarted)
            {
                if (interval.TotalSeconds < 30)
                {
                    interval = TimeSpan.FromSeconds(30);
                }

                hideTimer.Interval = interval;

            }
            else
            {
                hideTimer.Interval = TimeSpan.FromSeconds(10);
            }

            UpdateTiming();

            if (interval.TotalMinutes > 1)
            {
                updateTimer.Interval = TimeSpan.FromMinutes(1);
                updateTimer.Start();
            }


            hideTimer.Start();
            Show();
        }

        private void UpdateTiming()
        {
            LabelTime.FontSize = 60;
            LabelTime.Margin = new Thickness(0, -5, 0, 15);

            LabelIn.Foreground = Background;
            LabelTime.Foreground = Background;
            LabelMinutes.Foreground = Background;
            
            ButtonJoin.Foreground = Background;
            ButtonJoin.Visibility = appointment.ButtonLink != null ? Visibility.Visible : Visibility.Hidden;

            LabelMinutes.Visibility = System.Windows.Visibility.Visible;
            LabelIn.Visibility = System.Windows.Visibility.Visible;

            var ago = appointment.Start - DateTime.Now;

            if (!appointment.HasStarted)
            {
                if (ago.TotalHours < 1)
                {
                    LabelMinutes.Text = "MINUTE";
                    LabelTime.Text = ago.TotalMinutes.ToString("0");
                    if (ago.TotalMinutes >= 2)
                    {
                        LabelMinutes.Text += "S";

                    }
                }
                else
                {
                    LabelMinutes.Text = "HOUR";
                    // round up a bit - e.g. 2h50m should say "in 3h"
                    var hours = (((int)(ago.TotalMinutes + 10) / 30) / 2f);
                    LabelTime.Text = hours.ToString();

                    if (hours > 1)
                    {
                        LabelMinutes.Text += "S";

                    }

                }

            }
            else
            {
                LabelIn.Visibility = System.Windows.Visibility.Hidden;
                ago = -ago;


                if (ago.TotalMinutes < 2)
                {
                    LabelTime.Text = "NOW";
                    LabelTime.FontSize = 38;
                    LabelTime.Margin = new Thickness(0, 14, 0, -4);
                    LabelMinutes.Visibility = System.Windows.Visibility.Hidden;
                }
                else
                {
                    LabelTime.Text = ago.TotalMinutes.ToString("0");
                    LabelMinutes.Text = "MINUTES AGO";
                }

            }

        }

        private string Trim(string s, int length)
        {
            return s == null || s.Length < length ? s : s.Substring(0, length);
        }

        private void HideTimerOnTick(object sender, EventArgs eventArgs)
        {
            HideWindow();
        }

        private void UpdateTimerOnTick(object sender, EventArgs eventArgs)
        {
            UpdateTiming();
            foreach (var child in children)
            {
                child.UpdateTiming();
            }
        }


        private void HideWindow(bool fast = false)
        {
            if (parent != null)
            {
                parent.HideWindow(fast);
                return;
            }

            hideTimer.Stop();
            updateTimer.Stop();
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
                HideWindow(true);
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

        private void ButtonJoin_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(appointment.ButtonLink);
            HideWindow();
        }
    }
}
