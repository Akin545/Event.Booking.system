using Event.Booking.System.Core.Dtos.TicketTypes;
using Event.Booking.System.Core.Enums;
using Event.Booking.System.Core.Interface;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event.Booking.System.Core.Dtos.Event
{
    public class EventDto : Dto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Venue { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Capacity { get; set; }
        public int AvailableSeats { get; set; }
        public EventStatusEnum Status { get; set; } 

        public List<TicketTypeDto> TicketTypes { get; set; }
    }
}
