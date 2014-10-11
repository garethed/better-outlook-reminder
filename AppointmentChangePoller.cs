namespace BetterOutlookReminder
{
    using System;
    using System.Diagnostics;
    using System.Windows.Threading;

    internal class AppointmentChangePoller
    {
        private readonly OutlookService outlookService = new OutlookService();
        private readonly DispatcherTimer pollTimer = new DispatcherTimer();

        private AppointmentGroup nextAppointments;
        private bool firstCheck = true;

        public AppointmentChangePoller()
        {
            pollTimer.Interval = TimeSpan.FromMinutes(5);
            pollTimer.Tick += PollTimerOnTick;
        }

        internal delegate void NextAppointmentChangedEventHandler(object sender, NextAppointmentChangedEventHandlerArgs args);

        public event NextAppointmentChangedEventHandler NextAppointmentChanged;

        public AppointmentGroup CurrentValue
        {
            get { return nextAppointments; }
        }

        public void Start()
        {
            pollTimer.Start();
            CheckOutlook();
        }

        public void Force()
        {
            CheckOutlook();
        }

        private void PollTimerOnTick(object sender, EventArgs eventArgs)
        {
            Trace.WriteLine("PollTimer.tick");
            CheckOutlook();
        }

        private void CheckOutlook()
        {
            var newAppointments = outlookService.GetNextAppointments();
            if (newAppointments != nextAppointments || firstCheck)
            {
                firstCheck = false;
                nextAppointments = newAppointments;
                if (NextAppointmentChanged != null)
                {
                    Trace.WriteLine("AppointmentChange.fire " + newAppointments);
                    NextAppointmentChanged(this, new NextAppointmentChangedEventHandlerArgs { NextAppointments = nextAppointments });
                }
            }
        }

        internal class NextAppointmentChangedEventHandlerArgs
        {
            public AppointmentGroup NextAppointments;
        }
    }
}