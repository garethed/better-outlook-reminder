using Meziantou.Framework.Win32;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ExchangeAppointment = Microsoft.Exchange.WebServices.Data.Appointment;

namespace BetterOutlookReminder
{
    internal class OutlookService
    {
        private string[] scopes = new[] { "Calendars.Read" };

        private AppointmentGroup nextAppointments;

        public WebCredentials credentials;

        private bool lastUpdateSucceeded = true;

        IPublicClientApplication app;        

        public OutlookService()
        {
            app = PublicClientApplicationBuilder
                .Create("bff2bbd0-39a1-4263-9e06-f6bb37ce8679")
                .WithDefaultRedirectUri()
                .Build();
        }

        public void GetCredentials()
        {
            var scopes = new[] { "Calendars.Read" };
            /*var accounts = app.GetAccountsAsync().Result;

            AuthenticationResult result;

            try
            {
                result = app.AcquireTokenSilent(scopes, accounts.First()).ExecuteAsync().Result;
            }
            catch (MsalUiRequiredException)
            {
                result = app.AcquireTokenInteractive(scopes).ExecuteAsync().Result;
            }*/

        }

        // Allow autodiscover to follow redirects.
        static bool RedirectionCallback(string url)
        {         
            return url.ToLower().StartsWith("https://");
        }

        public async Task<AppointmentGroup> GetNextAppointments()
        {
            
            try                       
            {
                InteractiveAuthenticationProvider authProvider = new InteractiveAuthenticationProvider(app, scopes);

                GraphServiceClient graphClient = new GraphServiceClient(authProvider);

                var queryOptions = new List<QueryOption>()
                {
                    new QueryOption("startDateTime", DateTime.UtcNow.AddMinutes(-5).ToString("o", CultureInfo.InvariantCulture)),
                    new QueryOption("endDateTime", DateTime.UtcNow.AddDays(1).ToString("o", CultureInfo.InvariantCulture))
                };

                var events = await graphClient.Me.Calendar.CalendarView                    
                    .Request(queryOptions)
                    .Top(20)
                    .GetAsync();

                //System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

                var newAppointments = new AppointmentGroup();

                IEnumerable<Appointment> appointments = events.Select(MakeAppointment);

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

        private Appointment MakeAppointment(Event appointmentItem)
        {
            Appointment newAppointment = appointmentItem == null
                ? null
                : new Appointment(
                    appointmentItem.Id,
                    ConvertDateTime(appointmentItem.Start),
                    ConvertDateTime(appointmentItem.End),
                    appointmentItem.Subject,
                    appointmentItem.Location.DisplayName,
                    appointmentItem.Organizer.EmailAddress.Name,
                    appointmentItem.Attendees.Select(a => a.EmailAddress.Name));

            return newAppointment;
        }

        private DateTime ConvertDateTime(DateTimeTimeZone dttz)
        {
            return TimeZoneInfo.ConvertTime(DateTime.Parse(dttz.DateTime), TimeZoneInfo.FindSystemTimeZoneById(dttz.TimeZone), TimeZoneInfo.Local);
        }
    }
}
