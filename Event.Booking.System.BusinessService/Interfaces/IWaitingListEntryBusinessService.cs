using Event.Booking.System.Core.Models;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event.Booking.System.BusinessService.Interfaces
{
    public interface IWaitingListEntryBusinessService : IBusinessServiceBase<WaitingListEntry>
    {
        Task<List<WaitingListEntry>> ListAsync(int pageNumber, Guid eventId);
        Task<int> CountWaitingListAsync(Guid eventId);
    }
}
