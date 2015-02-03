using System.Threading;

namespace BetterOutlookReminder
{
    using Hardcodet.Wpf.TaskbarNotification;
    using System;
    using System.Linq;
    using System.Windows;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly NotificationWindow notificationWindow = new NotificationWindow(null);
        private readonly AppointmentChangePoller poller = new AppointmentChangePoller();
        private readonly NotificationTimer timer = new NotificationTimer();
        private TaskbarIcon notifyIcon;
        private bool first = true;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            notifyIcon = (TaskbarIcon) FindResource("NotificationIcon");
            notifyIcon.TrayLeftMouseUp += NotifyIconOnTrayLeftMouseUp;
            poller.NextAppointmentChanged += PollerOnNextAppointmentChanged;

            // If the app runs too quickly at Windows startup then it can show a nasty security
            // warning. A short delay fixes this. (The app can run without outlook - we use the
            // absence of outlook running as a good-enough indicator that the system is just booting)
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                Thread.Sleep(TimeSpan.FromSeconds(10));
            }

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
            // qq should really be able to see current appointment when clicking here
            poller.Force();
            notificationWindow.Show(poller.CurrentValue);
        }

        private void PollerOnNextAppointmentChanged(object sender,
            AppointmentChangePoller.NextAppointmentChangedEventHandlerArgs args)
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
