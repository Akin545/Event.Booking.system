using Event.Booking.System.Core.Enums;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event.Booking.System.Core.Dtos.User
{
    public class UserDto : Dto
    {
        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public string FullName { get; set; } = string.Empty;

        public RoleTypeEnum Roles { get; set; } = RoleTypeEnum.User;

    }
}
