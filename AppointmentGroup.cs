namespace BetterOutlookReminder
{
    using System.Collections.Generic;
    using System.Linq;

    public class AppointmentGroup
    {
        public IEnumerable<Appointment> Next;

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
            return Equals((AppointmentGroup)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Next != null ? Next.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            if (Next.Any())
            {
                return string.Format("[{0} appointments, starting with {1} at {2}]",
                    Next.Count(), Next.First().Subject, Next.First().Start.ToShortTimeString());
            }
            return "[Empty appointment list]";
        }

        protected bool Equals(AppointmentGroup other)
        {
            return Equals(Next, other.Next);
        }
    }
}