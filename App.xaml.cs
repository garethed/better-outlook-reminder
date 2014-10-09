namespace BetterOutlookReminder
{
    using Hardcodet.Wpf.TaskbarNotification;
    using System;
    using System.Windows;
    using System.Windows.Threading;

    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly NotificationWindow notificationWindow = new NotificationWindow();
        private readonly DispatcherTimer appointmentTimer = new DispatcherTimer();
        private TaskbarIcon notifyIcon;
        private readonly AppointmentChangePoller poller = new AppointmentChangePoller();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            notifyIcon = (TaskbarIcon)FindResource("NotificationIcon");
            notifyIcon.TrayLeftMouseUp += NotifyIconOnTrayLeftMouseUp;
            poller.NextAppointmentChanged += PollerOnNextAppointmentChanged;
            poller.Start();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            notifyIcon.Dispose();
            base.OnExit(e);
        }

        private void NotifyIconOnTrayLeftMouseUp(object sender, RoutedEventArgs routedEventArgs)
        {
            notificationWindow.ShowIfAvailable();
            poller.Force();
        }

        private void PollerOnNextAppointmentChanged(object sender, AppointmentChangePoller.NextAppointmentChangedEventHandlerArgs args)
        {
            var appt = args.NextAppointment;

            notificationWindow.Show(appt);
            if (appt != null)
            {
                notifyIcon.ToolTipText = string.Format("Next: {0} - {1}",
                    appt.Start.ToShortTimeString(),
                    appt.Subject);

                if (!appt.HasStarted)
                {
                    appointmentTimer.Interval = appt.Start.Subtract(DateTime.Now);
                    appointmentTimer.Tick += AppointmentTimerOnTick;
                    appointmentTimer.Start();
                }
            }
            else
            {
                notifyIcon.ToolTipText = "No further appointments today";
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
    }
}