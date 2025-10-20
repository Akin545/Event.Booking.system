using Event.Booking.System.Core.Enums;
using Event.Booking.System.Core.Interface;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event.Booking.System.Core.Dtos.Bookings
{
    public class CreateBookingDto : IDtoNoID
    {
       
        [Required]
        public int Quantity { get; set; }

        [Required]
        public Guid EventId { get; set; }


        [Required]
        public Guid TicketTypeId { get; set; }
    }
}
