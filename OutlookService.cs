namespace BetterOutlookReminder
{
    using Microsoft.Office.Interop.Outlook;
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using Exception = System.Exception;

    internal class OutlookService
    {
        private AppointmentGroup nextAppointments;

        public AppointmentGroup GetNextAppointments()
        {
            try
            {
                var newAppointments = new AppointmentGroup();

                var outlook = new Application();
                var session = outlook.Session;
                var folder = session.GetDefaultFolder(OlDefaultFolders.olFolderCalendar);
                var items = folder.Items;
                items.IncludeRecurrences = true;
                items.Sort("[Start]");

                var from = DateTime.Now.AddMinutes(-5);
                var to = DateTime.Today.AddDays(1);

                var restrictedItems = items.Restrict(string.Format("[Start] >= '{0}' AND [Start] < '{1}'", toOutlookString(from), toOutlookString(to)));

                newAppointments.Next = restrictedItems.Cast<object>().Select(getAppointment).Where(o => o != null && o.Start > DateTime.Now).Select(MakeAppointment);
                nextAppointments = newAppointments;

                return newAppointments;
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                return nextAppointments;
            }
        }

        private Appointment MakeAppointment(AppointmentItem appointmentItem)
        {
            var newAppointment = appointmentItem == null ? null :
                new Appointment
                {
                    ID = appointmentItem.EntryID,
                    Start = appointmentItem.Start,
                    End = appointmentItem.End,
                    Subject = appointmentItem.Subject,
                    Location = stripBracketedParts(appointmentItem.Location),
                    Organizer = stripBracketedParts(appointmentItem.Organizer),
                    Recipients = appointmentItem.Recipients.Cast<Recipient>().Select(r => stripBracketedParts(r.Name)).ToList()
                };

            if (newAppointment != null)
            {
                if (newAppointment.Subject.Length > 50)
                {
                    newAppointment.Subject = newAppointment.Subject.Substring(0, 50);
                }
            }
            return newAppointment;
        }

        private string toOutlookString(DateTime date)
        {
            return date.ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture);
        }

        private string stripBracketedParts(string input)
        {
            if (input == null)
            {
                return null;
            }

            foreach (var bracket in "{[(")
            {
                if (input.Contains(bracket))
                {
                    input = input.Substring(0, input.IndexOf(bracket));
                }
            }
            return input.Trim();
        }

        private AppointmentItem getAppointment(object outlookItem)
        {
            if (outlookItem is AppointmentItem)
            {
                return (AppointmentItem)outlookItem;
            }
            if (outlookItem is MeetingItem)
            {
                return ((MeetingItem)outlookItem).GetAssociatedAppointment(false);
            }
            return null;
        }
    }
}