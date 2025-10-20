using Event.Booking.System.Core.Model;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Event.Booking.System.Core.Models
{
    public class WaitingListEntry:Entity
    {
        [Required]
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public bool Notified { get; set; } = false;

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        [Required]
        public Guid EventId { get; set; }

        [Required]
        public Guid BookingId { get; set; }

        [ForeignKey(nameof(EventId))]
        public virtual Event Event { get; set; } = null!;
    }
}
