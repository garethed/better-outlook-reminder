using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace BetterOutlookReminder
{
    internal class AppointmentChangePoller
    {
        private readonly OutlookService outlookService = new OutlookService();
        private readonly DispatcherTimer pollTimer = new DispatcherTimer();

        private bool firstCheck = true;
        private bool checkInProgress;
        private AppointmentGroup nextAppointments;

        public AppointmentChangePoller()
        {
            pollTimer.Interval = TimeSpan.FromMinutes(5);
            pollTimer.Tick += PollTimerOnTick;
        }

        public AppointmentGroup CurrentValue
        {
            get { return nextAppointments; }
        }

        public event NextAppointmentChangedEventHandler NextAppointmentChanged;

        public async Task Start()
        {
            pollTimer.Start();
            await CheckOutlook();
        }

        public async Task Force()
        {
            Trace.WriteLine("PollTimer.Force");
            await CheckOutlook();
        }

        private async void PollTimerOnTick(object sender, EventArgs eventArgs)
        {
            Trace.WriteLine("PollTimer.tick");
            await CheckOutlook();
        }

        private async Task CheckOutlook()
        {
            // A check can block for a long time waiting on interactive sign-in; don't stack them up.
            if (checkInProgress)
            {
                Trace.WriteLine("PollTimer.skip - check already in progress");
                return;
            }

            checkInProgress = true;
            try
            {
                await CheckOutlookInner();
            }
            finally
            {
                checkInProgress = false;
            }
        }

        private async Task CheckOutlookInner()
        {
            AppointmentGroup newAppointments = await outlookService.GetNextAppointments();
            if (newAppointments != nextAppointments || firstCheck)
            {
                firstCheck = false;
                nextAppointments = newAppointments;
                if (NextAppointmentChanged != null)
                {
                    Trace.WriteLine("AppointmentChange.fire " + newAppointments);
                    NextAppointmentChanged(this,
                        new NextAppointmentChangedEventHandlerArgs {NextAppointments = nextAppointments});
                }
            }
        }

        internal delegate void NextAppointmentChangedEventHandler(
            object sender, NextAppointmentChangedEventHandlerArgs args);

        internal class NextAppointmentChangedEventHandlerArgs
        {
            public AppointmentGroup NextAppointments;
        }
    }
}
