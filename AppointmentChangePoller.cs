namespace BetterOutlookReminder
{
    using System;
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