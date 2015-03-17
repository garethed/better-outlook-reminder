using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Linq;
using System.Windows;

namespace BetterOutlookReminder
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly NotificationWindow notificationWindow = new NotificationWindow(null);
        private readonly AppointmentChangePoller poller = new AppointmentChangePoller();
        private readonly NotificationTimer timer = new NotificationTimer();
        private bool first = true;
        private TaskbarIcon notifyIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            notifyIcon = (TaskbarIcon) FindResource("NotificationIcon");
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
            // qq should really be able to see current appointment when clicking here
            poller.Force();
            notificationWindow.Show(poller.CurrentValue);
        }

        private void PollerOnNextAppointmentChanged(object sender,
            AppointmentChangePoller.NextAppointmentChangedEventHandlerArgs args)
        {
            AppointmentGroup appts = args.NextAppointments;

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
            Appointment next = appointments.Next.FirstOrDefault(a => a.Start > DateTime.Now);
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
