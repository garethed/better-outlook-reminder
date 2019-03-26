using Meziantou.Framework.Win32;
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

        public WebCredentials credentials;

        private bool lastUpdateSucceeded = true;

        public OutlookService()
        {
            GetCredentials();
        }

        public void GetCredentials()
        {
            var cred = CredentialManager.ReadCredential("BetterOutlookReminder");

            if (cred == null || !lastUpdateSucceeded)
            {
                var credresult = CredentialManager.PromptForCredentials(IntPtr.Zero, "Enter your Outlook365 credentials (email address and password)", "Better Outlook Reminders", null, CredentialSaveOption.Hidden);
                credentials = new Microsoft.Exchange.WebServices.Data.WebCredentials(credresult.UserName, credresult.Password);
                CredentialManager.WriteCredential("BetterOutlookReminder", credresult.UserName, credresult.Password, CredentialPersistence.LocalMachine);
            }
            else
            {
                credentials = new Microsoft.Exchange.WebServices.Data.WebCredentials(cred.UserName, cred.Password);
            }
        }

        // Allow autodiscover to follow redirects.
        static bool RedirectionCallback(string url)
        {         
            return url.ToLower().StartsWith("https://");
        }

        public AppointmentGroup GetNextAppointments()
        {
            try
            {
                //System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

                var newAppointments = new AppointmentGroup();

                var service = new ExchangeService();
                service.Credentials = credentials;
                service.TraceEnabled = true;
                service.TraceFlags = TraceFlags.All;
                service.TraceEnablePrettyPrinting = true;
                service.AutodiscoverUrl(UserPrincipal.Current.EmailAddress, RedirectionCallback);


                DateTime from = DateTime.Now.AddMinutes(-5);
                DateTime to = DateTime.Today.AddDays(1);


                IEnumerable<Appointment> appointments =
                    service.FindAppointments(WellKnownFolderName.Calendar, new CalendarView(from, to))
                        .Select(MakeAppointment);

                newAppointments.Next =
                    appointments.Where(o => o != null && o.Start >= DateTime.Now)
                        .OrderBy(o => o.Start).ToList();

                nextAppointments = newAppointments;
                lastUpdateSucceeded = true;

                return newAppointments;
            }
            catch (Exception e)
            {
                lastUpdateSucceeded = false;
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
