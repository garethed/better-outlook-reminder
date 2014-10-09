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
        private Appointment nextAppointment;

        public Appointment GetNextAppointment()
        {
            try
            {
                var outlook = new Application();
                var session = outlook.Session;
                var folder = session.GetDefaultFolder(OlDefaultFolders.olFolderCalendar);
                var items = folder.Items;
                items.IncludeRecurrences = true;
                items.Sort("[Start]");

                var from = DateTime.Now;
                var to = nextAppointment != null && nextAppointment.Start > from ? nextAppointment.Start : DateTime.Today.AddDays(1);

                var restrictedItems = items.Restrict(string.Format("[Start] >= '{0}' AND [Start] < '{1}'", toOutlookString(from), toOutlookString(to)));
                //restrictedItems.Sort("[Start]");

                var next = restrictedItems.Cast<object>().Select(getAppointment).FirstOrDefault(o => o != null && o.Start > DateTime.Now.AddSeconds(1));

                var newAppointment = next == null ? null :
                    new Appointment
                    {
                        ID = next.EntryID,
                        Start = next.Start,
                        End = next.End,
                        Subject = next.Subject,
                        Location = stripBracketedParts(next.Location),
                        Organizer = stripBracketedParts(next.Organizer),
                        Recipients = next.Recipients.Cast<Recipient>().Select(r => stripBracketedParts(r.Name)).ToList()
                    };

                if (newAppointment != null)
                {
                    if (newAppointment.Subject.Length > 50)
                    {
                        newAppointment.Subject = newAppointment.Subject.Substring(0, 50);
                    }
                }

                nextAppointment = newAppointment;

                return newAppointment;
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                return nextAppointment;
            }
        }

        private string toOutlookString(DateTime date)
        {
            return date.ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture);
        }

        private string stripBracketedParts(string input)
        {
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