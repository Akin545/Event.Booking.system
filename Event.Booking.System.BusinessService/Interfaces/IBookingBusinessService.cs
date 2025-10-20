using Event.Booking.System.Core.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event.Booking.System.BusinessService.Interfaces
{
    public interface IBookingBusinessService : IBusinessServiceBase<Core.Models.Booking>
    {
        Task<Core.Models.Booking> GetLatestWaitingUserAsync(int qty, Guid eventId, Guid ticketId);
        Task<Guid> UpdateAsync(Core.Models.Booking item);
    }
}
