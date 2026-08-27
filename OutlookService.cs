using Meziantou.Framework.Win32;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Broker;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
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

        private readonly SemaphoreSlim authLock = new SemaphoreSlim(1, 1);

        private System.Windows.Window authOwnerWindow;

        public OutlookService()
        {
            // The broker (WAM) can often satisfy Entra's daily re-auth requirement from the
            // Windows sign-in itself, so the user sees an account picker at worst rather than a
            // full login. MSAL falls back to the browser if the broker isn't available.
            app = PublicClientApplicationBuilder
                .Create("bff2bbd0-39a1-4263-9e06-f6bb37ce8679")
                .WithBroker(new BrokerOptions(BrokerOptions.OperatingSystems.Windows))
                .WithParentActivityOrWindow(GetAuthOwnerWindow)
                .WithDefaultRedirectUri()
                .Build();

            TokenCacheHelper.EnableSerialization(app.UserTokenCache);
        }

        public async Task<AppointmentGroup> GetNextAppointments()
        {
            
            try                       
            {
                GraphServiceClient graphClient = new GraphServiceClient(
                    new DelegateAuthenticationProvider(async request =>
                    {
                        string token = await GetToken();
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    }));

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

        /// <summary>
        /// Only ever one interactive sign-in at a time: everything else queues on authLock and
        /// retries silently, so a token expiring can't spawn a pile of login windows.
        /// </summary>
        private async Task<string> GetToken()
        {
            var silent = await TryAcquireSilent();
            if (silent != null) return silent;

            await authLock.WaitAsync();
            try
            {
                // Someone else may have signed in while we waited.
                silent = await TryAcquireSilent();
                if (silent != null) return silent;

                var result = await app.AcquireTokenInteractive(scopes).ExecuteAsync();
                return result.AccessToken;
            }
            finally
            {
                authLock.Release();
            }
        }

        private async Task<string> TryAcquireSilent()
        {
            var account = (await app.GetAccountsAsync()).FirstOrDefault();
            if (account == null) return null;

            try
            {
                var result = await app.AcquireTokenSilent(scopes, account).ExecuteAsync();
                return result.AccessToken;
            }
            catch (MsalUiRequiredException)
            {
                return null;
            }
        }

        /// <summary>
        /// WAM needs an owner HWND, and auth can fire from a background poll at any moment - so we
        /// own the dialog with our own invisible window rather than whatever app happens to be in
        /// the foreground. The notification window is no good: it's topmost and moves around.
        /// </summary>
        private IntPtr GetAuthOwnerWindow()
        {
            return System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (authOwnerWindow == null)
                {
                    authOwnerWindow = new System.Windows.Window
                    {
                        Width = 0,
                        Height = 0,
                        WindowStyle = System.Windows.WindowStyle.None,
                        ShowInTaskbar = false,
                        ShowActivated = false,
                        Visibility = System.Windows.Visibility.Hidden
                    };
                }

                return new System.Windows.Interop.WindowInteropHelper(authOwnerWindow).EnsureHandle();
            });
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
