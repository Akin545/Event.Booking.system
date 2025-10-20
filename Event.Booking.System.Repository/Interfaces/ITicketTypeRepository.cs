using Event.Booking.System.Core.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event.Booking.System.Repository.Interfaces
{
    public interface ITicketTypeRepository : IRepositoryBase<TicketType>
    {
        Task<TicketType> GetEventByIdAsync(Guid id);
    }
}
