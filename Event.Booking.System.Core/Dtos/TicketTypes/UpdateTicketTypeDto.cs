using Event.Booking.System.Core.Interface;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event.Booking.System.Core.Dtos.TicketTypes
{
    public class UpdateTicketTypeDto : IDtoNoID
    {
        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty; // Regular, VIP, etc.

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required]
        public int QuantityAvailable { get; set; }
    }
}
