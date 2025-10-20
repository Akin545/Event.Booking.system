using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event.Booking.System.Core.Exceptions
{
    public class EventException : PermissionBaseException
    {
        public EventException(string message) : base(message)
        {

        }
    }
}
