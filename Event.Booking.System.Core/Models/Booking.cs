using Event.Booking.System.Core.Enums;
using Event.Booking.System.Core.Model;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Event.Booking.System.Core.Models
{
    public class Booking: Entity
    {
        [Required]
        public DateTime BookingDate { get; set; } = DateTime.UtcNow;

        [Required]
        public int Quantity { get; set; }

        [Required]
        public BookingStatus Status { get; set; } = BookingStatus.Confirmed;

        [Required]
        public Guid EventId { get; set; }

        [ForeignKey(nameof(EventId))]
        public virtual Event Event { get; set; } = null!;

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        [Required]
        public Guid TicketTypeId { get; set; }

        [ForeignKey(nameof(TicketTypeId))]
        public virtual TicketType TicketType { get; set; } = null!;
    }
}
