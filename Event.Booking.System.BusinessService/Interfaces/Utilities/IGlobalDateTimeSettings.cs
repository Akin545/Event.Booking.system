using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event.Booking.System.BusinessService.Interfaces.Utilities
{
    public interface IGlobalDateTimeSettings
    {
        DateTime CurrentDateTime { get; }
    }

}
