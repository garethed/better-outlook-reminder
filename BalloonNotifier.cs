using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace BetterOutlookReminder
{
  public partial class BalloonNotifier : Component
  {
    private DateTime? nextTime;
    private Outlook.AppointmentItem appointment;
    

    public BalloonNotifier()
    {
      InitializeComponent();  
      PollForChanges();
    }

    private void Quit_Click(object sender, EventArgs e)
    {
      Application.Exit();
    }

    private void PollForChanges()
    {
      var outlook = new Outlook.Application();
      var session = outlook.Session;
      var folder = session.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderCalendar);
      var items = folder.Items;
      items.IncludeRecurrences = true;
      items.Sort("[Start]");

      var from = DateTime.Now;
      var to = nextTime != null && nextTime > from ? nextTime.Value : DateTime.Today.AddDays(1);

      var restrictedItems = items.Restrict(string.Format("[Start] >= '{0}' AND [Start] < '{1}'", toOutlookString(from), toOutlookString(to)));
      //restrictedItems.Sort("[Start]");

      setNext(restrictedItems.Cast<object>().Select(getAppointment).FirstOrDefault(o => o != null && o.Start > DateTime.Now.AddSeconds(1)));
    }

    private void setNext(Outlook.AppointmentItem appointment)
    {
      if (this.appointment == null || this.appointment.EntryID != appointment.EntryID)
      {
        notifyTimer.Stop();
        warningTimer.Stop();

        this.appointment = appointment;
        notifyTimer.Interval = (int)(appointment.Start - DateTime.Now).TotalMilliseconds;
        notifyTimer.Start();

        notifyIcon.Text = string.Format("Next: {0} - {1}", appointment.Start.ToShortTimeString(),
                                        appointment.Subject.Shorten(50));

        if (notifyTimer.Interval > 300000)
        {
          warningTimer.Interval = notifyTimer.Interval - 300000;
          warningTimer.Start();
        }

      }
    }

    private string toOutlookString(DateTime date)
    {
      return date.ToString("MM/dd/yyyy hh:mm tt", CultureInfo.InvariantCulture);
    }

    private Outlook.AppointmentItem getAppointment(object outlookItem)
    {
      if (outlookItem is Outlook.AppointmentItem)
      {
        return (Outlook.AppointmentItem) outlookItem;
      }
      else if (outlookItem is Outlook.MeetingItem)
      {
        return ((Outlook.MeetingItem) outlookItem).GetAssociatedAppointment(false);
      }
      return null;

    }

    private void pollTimer_Tick(object sender, EventArgs e)
    {
      PollForChanges();
    }

    private void notifyTimer_Tick(object sender, EventArgs e)
    {
      showBalloon(false);
      notifyTimer.Stop();      
    }

    private void showBalloon(bool warningOnly)
    {
      var when = appointment.Start.AddMinutes(-1) < DateTime.Now
                   ? "NOW -"
                   : "In " + ((int) (appointment.Start - DateTime.Now).TotalMinutes).ToString() + " mins -";

      //qq formatting
      notifyIcon.ShowBalloonTip(warningOnly? 10000 : 60000, when + " " + appointment.Subject.Shorten(30), formatLocation(appointment.Location) + appointment.Body.Shorten(500), warningOnly ? ToolTipIcon.None : ToolTipIcon.Error);      
    }

    private void warningTimer_Tick(object sender, EventArgs e)
    {
      showBalloon(true);
    }

    private void notifyIcon_Click(object sender, EventArgs e)
    {
      showBalloon(true);
    }

    private string formatLocation(string location)
    {
      if (string.IsNullOrWhiteSpace(location))
      {
        return "";
      }
      if (location.Contains("(Softwire, "))
      {
        return location.Substring(0, location.IndexOf("(Softwire, ") - 1) + "\n";
      }
      return location + "\n";
    }
  }
}
