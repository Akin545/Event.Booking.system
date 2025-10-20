using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Event.Booking.System.Core.Dtos.WaitingListEntry
{
    public class WaitingListDto: Dto
    {
        public Guid EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public bool Notified { get; set; }

    }
}
