using Event.Booking.System.Core.Enums;
using Event.Booking.System.Core.Model;

using System.ComponentModel.DataAnnotations;

namespace Event.Booking.System.Core.Models
{
    public class Event:Entity
    {
        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required, MaxLength(200)]
        public string Venue { get; set; } = string.Empty;

        [Required]
        public int Capacity { get; set; }
        public EventStatusEnum Status { get; set; } = EventStatusEnum.NotStarted;


        public ICollection<TicketType> TicketTypes { get; set; } = new List<TicketType>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<WaitingListEntry> WaitingListEntries { get; set; } = new List<WaitingListEntry>();
    }
}
