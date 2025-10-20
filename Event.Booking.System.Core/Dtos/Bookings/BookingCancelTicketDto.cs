using Event.Booking.System.Core.Interface;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event.Booking.System.Core.Dtos.Bookings
{
    public class BookingCancelTicketDto : IDtoNoID
    {

        [Required]
        public Guid BookingId { get; set; }

    }
}
