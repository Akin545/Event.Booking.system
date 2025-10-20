using Event.Booking.System.Core.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event.Booking.System.Repository.Interfaces
{
    public interface IWaitingListEntryRepository : IRepositoryBase<WaitingListEntry>
    {
        Task<List<WaitingListEntry>> ListAsync(int pageNumber, Guid eventId);
        Task<int> CountWaitingListAsync(Guid eventId);
    }
}
