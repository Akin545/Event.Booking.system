using Event.Booking.System.Core.Models;

using Microsoft.Extensions.Logging;

using System.Net.Sockets;

namespace Event.Booking.System.Repository.Interfaces
{
    public interface IBookingRepository : IRepositoryBase<Core.Models.Booking>
    {
        Task<Core.Models.Booking> GetLatestWaitingUserAsync(int qty, Guid eventId, Guid ticketId);
        Task<Core.Models.Booking> GetBookingUserAsync(Guid eventId, Guid userId);
        Task<Guid> AddAsync(Core.Models.Booking item, TicketType updateTicketQty, 
            WaitingListEntry waitingListEntry);
        Task<Guid> UpdateAllTablesRelatedAsync(Core.Models.Booking item, TicketType ticket,
            WaitingListEntry updateWaitingList, Core.Models.Booking updateBookinWaiter);
    }
}
