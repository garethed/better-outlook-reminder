namespace BetterOutlookReminder
{
    using Hardcodet.Wpf.TaskbarNotification;
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Threading;

    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly NotificationWindow notificationWindow = new NotificationWindow(null);
        private readonly DispatcherTimer appointmentTimer = new DispatcherTimer();
        private readonly AppointmentChangePoller poller = new AppointmentChangePoller();
        private readonly NotificationTimer timer = new NotificationTimer();
        private TaskbarIcon notifyIcon;
        private bool first = true;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            notifyIcon = (TaskbarIcon)FindResource("NotificationIcon");
            notifyIcon.TrayLeftMouseUp += NotifyIconOnTrayLeftMouseUp;
            poller.NextAppointmentChanged += PollerOnNextAppointmentChanged;
            poller.Start();
            timer.NotificationDue += TimerOnNotificationDue;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            notifyIcon.Dispose();
            base.OnExit(e);
        }

        private void TimerOnNotificationDue(object sender, NotificationTimer.NotificationDueEventArgs args)
        {
            notificationWindow.Show(args.Appointments);
            UpdateTooltip(args.Appointments);
        }

        private void NotifyIconOnTrayLeftMouseUp(object sender, RoutedEventArgs routedEventArgs)
        {
            notificationWindow.ShowIfAvailable();
            poller.Force();
        }

        private void PollerOnNextAppointmentChanged(object sender, AppointmentChangePoller.NextAppointmentChangedEventHandlerArgs args)
        {
            var appts = args.NextAppointments;

            timer.updateNextAppointment(appts);
            UpdateTooltip(appts);

            if (first)
            {
                notificationWindow.Show(appts);
                first = false;
            }
        }

        private void AppointmentTimerOnTick(object sender, EventArgs eventArgs)
        {
            appointmentTimer.Stop();
            notificationWindow.ShowIfAvailable();
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            Shutdown();
        }

        private void UpdateTooltip(AppointmentGroup appointments)
        {
            var next = appointments.Next.FirstOrDefault(a => a.Start > DateTime.Now);
            if (next != null)
            {
                notifyIcon.ToolTipText = string.Format("Next: {0} - {1}",
                    next.Start.ToShortTimeString(),
                    next.Subject);
            }
            else
            {
                notifyIcon.ToolTipText = "No further appointments today";
            }
        }
    }
}