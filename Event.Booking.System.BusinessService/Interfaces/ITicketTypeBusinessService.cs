using Event.Booking.System.Core.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event.Booking.System.BusinessService.Interfaces
{
    public interface ITicketTypeBusinessService : IBusinessServiceBase<TicketType>
    {
        Task<TicketType> GetEventByIdAsync(Guid id);
    }
}
