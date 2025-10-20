using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event.Booking.System.BusinessService.Interfaces
{
    public interface IGlobalService
    {
        bool IsValidEmail(string email);
        bool IsValidPhoneNumber(string phoneNumber);
        string? Email { get; }
        string? Roles { get; }
        string? Name { get; }
        string? Id { get; }
    }
}
