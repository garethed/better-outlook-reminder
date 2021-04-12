namespace BetterOutlookReminder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class Appointment
    {
        public readonly string ID;

        public readonly DateTime Start;
        public readonly DateTime End;
        public readonly string Subject;
        public readonly string Location;
        public readonly string Organizer;
        public readonly IEnumerable<string> Recipients;
        public readonly string ButtonLink;
        public readonly string ButtonText;

        public Appointment(string id, DateTime start, DateTime end, string subject, string location, string organizer, IEnumerable<string> recipients, string body)
        {
            ID = id;
            Start = start;
            End = end;
            Subject = subject;
            Location = stripBracketedParts(location);
            Organizer = stripBracketedParts(organizer);
            Recipients = recipients.Select(stripBracketedParts).ToList();
            (ButtonLink, ButtonText) = FindJoinLink(location, body);
        }

        public bool HasStarted
        {
            get { return Start <= DateTime.Now.AddSeconds(10); }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((Appointment)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (ID != null ? ID.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Start.GetHashCode();
                hashCode = (hashCode * 397) ^ (Subject != null ? Subject.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Location != null ? Location.GetHashCode() : 0);
                return hashCode;
            }
        }

        protected bool Equals(Appointment other)
        {
            return string.Equals(ID, other.ID) && Start.Equals(other.Start) && string.Equals(Subject, other.Subject) && string.Equals(Location, other.Location);
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

        private Tuple<string,string> FindJoinLink(string location, string body) 
        {
            try
            {
                var hrefmatch = @"href\s*=\s*(?:[""'](?<1>[^""']*)[""']|(?<1>\S+))";

                 var m = Regex.Match(body, hrefmatch,
                        RegexOptions.IgnoreCase | RegexOptions.Compiled);

                while (m.Success)
                {
                    var href = m.Groups[1].Value;

                    // Teams
                    // https://teams.microsoft.com/l/meetup-join/19%3ameeting_M2EzNTQ2M2QtOTAxMC00OTExLWI5MjQtNzNlYjgyOTVjNDRm%40thread.v2/0?context=%7b%22Tid%22%3a%22390c54b8-e374-4842-a54b-40de8f33588d%22%2c%22Oid%22%3a%22618101ea-1eba-4e3f-9c7a-b217d3559703%22%7d
                    if (href.ToLowerInvariant().Contains("teams.microsoft.com"))
                    {
                        return Tuple.Create(href, "Join in Teams");
                    }
                    // Zoom: https://us02web.zoom.us/j/85729412466?pwd=eE94TDdQUjd3blVoTmlmOXF0RVBHUT09 
                    else if (href.ToLowerInvariant().Contains("web.zoom.us"))
                    {
                        return Tuple.Create(href, "Join on Zoom");
                    }

                    m = m.NextMatch();
                }

                var hashtagmatch = @"#[a-z0-9-_]+";
                m = Regex.Match(location, hashtagmatch, RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    return Tuple.Create(
                        "https://slack.com/app_redirect?team=T02MTJKG5&channel=" + m.Groups[0].Value.Substring(1),
                        "Join in Slack");
                }

                // https://slack.com/app_redirect?team=T02MTJKG5&channel=release-notes

            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine("Error parsing meeting link: " + e);
            }

            return Tuple.Create((string)null, (string)null);
        }
    }
}