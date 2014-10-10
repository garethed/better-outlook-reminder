namespace BetterOutlookReminder
{
    using System;
    using System.Linq;
    using System.Windows.Threading;

    internal class NotificationTimer
    {
        private readonly DispatcherTimer warningTimer = new DispatcherTimer();
        private readonly DispatcherTimer notifyTimer = new DispatcherTimer();
        private AppointmentGroup nextAppointments;

        public NotificationTimer()
        {
            warningTimer.Tick += WarningTimerOnTick;
            notifyTimer.Tick += NotifyTimerOnTick;
        }

        internal delegate void NotificationDueEvent(object sender, NotificationDueEventArgs args);

        public event NotificationDueEvent NotificationDue;

        public void updateNextAppointment(AppointmentGroup appointments)
        {
            warningTimer.Stop();
            notifyTimer.Stop();

            nextAppointments = appointments;

            if (appointments != null && appointments.Next.Any())
            {
                notifyTimer.Interval = (appointments.Next.First().Start - DateTime.Now);
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
        }

        private void WarningTimerOnTick(object sender, EventArgs eventArgs)
        {
            warningTimer.Stop();
        }

        private void fireEvent()
        {
            if (NotificationDue != null)
            {
                NotificationDue(this, new NotificationDueEventArgs { Appointments = nextAppointments });
            }
        }

        internal class NotificationDueEventArgs
        {
            public AppointmentGroup Appointments;
        }
    }
}