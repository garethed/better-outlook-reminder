﻿using Meziantou.Framework.Win32;
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

            TokenCacheHelper.EnableSerialization(app.UserTokenCache);
        }

        public async Task<AppointmentGroup> GetNextAppointments()
        {
            
            try                       
            {
                InteractiveAuthenticationProvider authProvider = new InteractiveAuthenticationProvider(app, scopes);

                GraphServiceClient graphClient = new GraphServiceClient(authProvider);

                var queryOptions = new List<QueryOption>()
                {
                    new QueryOption("startDateTime", DateTime.UtcNow.AddMinutes(-15).ToString("o", CultureInfo.InvariantCulture)),
                    new QueryOption("endDateTime", DateTime.Today.ToUniversalTime().AddDays(1).ToString("o", CultureInfo.InvariantCulture))
                };

                var events = await graphClient.Me.Calendar.CalendarView                    
                    .Request(queryOptions)
                    .Top(20)
                    .GetAsync();

                var newAppointments = new AppointmentGroup();

                IEnumerable<Appointment> appointments = events.Select(MakeAppointment);

                newAppointments.Next =
                    appointments.Where(o => o != null && o.End >= DateTime.Now && o.Start >= DateTime.Now.AddMinutes(-29))
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
                    appointmentItem.Attendees.Select(a => a.EmailAddress.Name),
                    appointmentItem.Body.Content);

            return newAppointment;
        }

        private DateTime ConvertDateTime(DateTimeTimeZone dttz)
        {
            return TimeZoneInfo.ConvertTime(DateTime.Parse(dttz.DateTime), TimeZoneInfo.FindSystemTimeZoneById(dttz.TimeZone), TimeZoneInfo.Local);
        }
    }
}
