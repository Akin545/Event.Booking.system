using Event.Booking.System.Core.Model;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Event.Booking.System.Core.Models
{
    public class TicketType:Entity
    {
        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty; // Regular, VIP, etc.

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public string Status { get; set; } = "Active";

        [Required]
        public int QuantityAvailable { get; set; }

        [Required]
        public Guid EventId { get; set; }

        [ForeignKey(nameof(EventId))]
        public virtual Event Event { get; set; } = null!;
    }
}
