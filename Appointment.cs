namespace BetterOutlookReminder
{
    using System;
    using System.Collections.Generic;

    public class Appointment
    {
        public string ID;
        public DateTime Start;
        public DateTime End;
        public string Subject;
        public string Location;
        public string Organizer;
        public IEnumerable<string> Recipients;

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
    }
}