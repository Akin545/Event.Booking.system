using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event.Booking.System.Core.Dtos.Bookings
{
    public class BookingDto: Dto
    {
        public DateTime BookingDate { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; } = string.Empty;

        public Guid EventId { get; set; }
        public string EventName { get; set; } = string.Empty;

        public string TicketType { get; set; } = string.Empty;

        public Guid UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
    }
}
