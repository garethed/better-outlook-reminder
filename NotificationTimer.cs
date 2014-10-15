namespace BetterOutlookReminder
{
    using System;
    using System.Diagnostics;
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
                var interval = (appointments.Next.First().Start - DateTime.Now);

                if (interval.TotalSeconds > 1)
                {
                    notifyTimer.Interval = interval;
                    notifyTimer.Start();

                    if (notifyTimer.Interval > TimeSpan.FromMinutes(5))
                    {
                        warningTimer.Interval = notifyTimer.Interval.Subtract(TimeSpan.FromMinutes(5));
                        warningTimer.Start();
                    }
                }
            }
        }

        private void NotifyTimerOnTick(object sender, EventArgs eventArgs)
        {
            Trace.WriteLine("NotifyTimer.tick " + nextAppointments);
            fireEvent();
            notifyTimer.Stop();
        }

        private void WarningTimerOnTick(object sender, EventArgs eventArgs)
        {
            Trace.WriteLine("WarningTimer.tick " + nextAppointments);
            fireEvent();
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