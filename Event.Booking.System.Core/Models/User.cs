using Event.Booking.System.Core.Enums;
using Event.Booking.System.Core.Model;

using System.ComponentModel.DataAnnotations;

namespace Event.Booking.System.Core.Models
{
    public class User: Entity
    {
        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, MaxLength(100), EmailAddress]
        public string Email { get; set; } = string.Empty;
        public RoleTypeEnum Roles { get; set; } = RoleTypeEnum.User;

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }
        public string PasswordHash { get; set; } = string.Empty;

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<WaitingListEntry> WaitingListEntries { get; set; } = new List<WaitingListEntry>();
    }
}
