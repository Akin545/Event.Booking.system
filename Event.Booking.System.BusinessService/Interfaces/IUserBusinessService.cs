using Event.Booking.System.Core.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event.Booking.System.BusinessService.Interfaces
{
    public interface IUserBusinessService : IBusinessServiceBase<User>
    {
        Task<User> GetByEmailAsync(string email);
        Task<Guid> AddAsync(User entity);
        Task<string> LoginAsync(User item);
    }
}
