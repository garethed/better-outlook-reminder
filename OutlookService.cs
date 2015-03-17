using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using ExchangeAppointment = Microsoft.Exchange.WebServices.Data.Appointment;

namespace BetterOutlookReminder
{
    internal class OutlookService
    {
        private AppointmentGroup nextAppointments;

        public AppointmentGroup GetNextAppointments()
        {
            try
            {
                var newAppointments = new AppointmentGroup();

                var service = new ExchangeService();
                service.UseDefaultCredentials = true;
                service.AutodiscoverUrl(UserPrincipal.Current.EmailAddress);


                DateTime from = DateTime.Now.AddMinutes(-5);
                DateTime to = DateTime.Today.AddDays(1);


                IEnumerable<Appointment> appointments =
                    service.FindAppointments(WellKnownFolderName.Calendar, new CalendarView(from, to))
                        .Select(MakeAppointment);

                newAppointments.Next =
                    appointments.Where(o => o != null && o.Start >= DateTime.Now)
                        .OrderBy(o => o.Start).ToList();

                nextAppointments = newAppointments;
                return newAppointments;
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                return nextAppointments;
            }
        }

        private Appointment MakeAppointment(ExchangeAppointment appointmentItem)
        {
            Appointment newAppointment = appointmentItem == null
                ? null
                : new Appointment(
                    appointmentItem.Id.UniqueId,
                    appointmentItem.Start,
                    appointmentItem.End,
                    appointmentItem.Subject,
                    appointmentItem.Location,
                    appointmentItem.Organizer.Name,
                    appointmentItem.RequiredAttendees.Union(appointmentItem.OptionalAttendees).Select(a => a.Name));

            return newAppointment;
        }
    }
}
