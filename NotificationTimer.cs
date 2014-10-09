using System;

namespace BetterOutlookReminder
{
    using System.Windows.Threading;

    class NotificationTimer
    {
        private DispatcherTimer warningTimer = new DispatcherTimer();
        private DispatcherTimer notifyTimer = new DispatcherTimer();
        private Appointment nextAppointment;
        private NotificationWindow notificationWindow = new NotificationWindow();

        public NotificationTimer()
        {
            warningTimer.Tick += WarningTimerOnTick;
            notifyTimer.Tick += NotifyTimerOnTick;
        }

        public void updateNextAppointment(Appointment appointment)
        {
            warningTimer.Stop();
            notifyTimer.Stop();

            nextAppointment = appointment;

            if (appointment != null)
            {

                notifyTimer.Interval = (appointment.Start - DateTime.Now);
                notifyTimer.Start();


                if (notifyTimer.Interval > TimeSpan.FromMinutes(5))
                {
                    warningTimer.Interval = notifyTimer.Interval.Subtract(TimeSpan.FromMinutes(5));
                    warningTimer.Start();
                }
            }
        }
        private void NotifyTimerOnTick(object sender, EventArgs eventArgs)
        {
            notifyTimer.Stop();
            notificationWindow.Show(nextAppointment);
        }

        private void WarningTimerOnTick(object sender, EventArgs eventArgs)
        {
            notifyTimer.Stop();
            notificationWindow.Show(nextAppointment);
        }
    }
}
