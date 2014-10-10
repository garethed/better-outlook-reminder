namespace BetterOutlookReminder
{
    using System.Collections.Generic;

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

        protected bool Equals(AppointmentGroup other)
        {
            return Equals(Next, other.Next);
        }
    }
}