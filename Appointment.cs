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

        private static readonly IEqualityComparer<Appointment> IdStartComparerInstance = new IdStartEqualityComparer();

        public static IEqualityComparer<Appointment> IdStartComparer
        {
            get { return IdStartComparerInstance; }
        }

        public bool HasStarted
        {
            get { return Start < DateTime.Now; }
        }

        private sealed class IdStartEqualityComparer : IEqualityComparer<Appointment>
        {
            public bool Equals(Appointment x, Appointment y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }
                if (ReferenceEquals(x, null))
                {
                    return false;
                }
                if (ReferenceEquals(y, null))
                {
                    return false;
                }
                if (x.GetType() != y.GetType())
                {
                    return false;
                }
                return string.Equals(x.ID, y.ID) && x.Start.Equals(y.Start);
            }

            public int GetHashCode(Appointment obj)
            {
                unchecked
                {
                    return ((obj.ID != null ? obj.ID.GetHashCode() : 0) * 397) ^ obj.Start.GetHashCode();
                }
            }
        }
    }
}