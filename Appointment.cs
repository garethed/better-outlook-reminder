namespace BetterOutlookReminder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Appointment
    {
        public readonly string ID;

        public readonly DateTime Start;
        public readonly DateTime End;
        public readonly string Subject;
        public readonly string Location;
        public readonly string Organizer;
        public readonly IEnumerable<string> Recipients;

        public Appointment(string id, DateTime start, DateTime end, string subject, string location, string organizer, IEnumerable<string> recipients)
        {
            ID = id;
            Start = start;
            End = end;
            Subject = subject;
            Location = stripBracketedParts(location);
            Organizer = stripBracketedParts(organizer);
            Recipients = recipients.Select(stripBracketedParts).ToList();
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
    }
}