using Event.Booking.System.Core.Enums;
using Event.Booking.System.Core.Interface;
using Event.Booking.System.Core.Model;
using Event.Booking.System.Core.Models;
using Event.Booking.System.Repository.Interfaces;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Event.Booking.System.Repository
{
   public class BookingRepository : RepositoryBase<Core.Models.Booking>, IBookingRepository
    {
        public BookingRepository(IConfiguration configuration, ILogger<Core.Models.Booking> logger, IServiceScopeFactory scopeFactory)
            : base(configuration, logger, scopeFactory)
        {
        }


        public override async Task<Core.Models.Booking> GetAsync(Guid id)
        {
            try
            {
                using (var scope = ScopeFactory.CreateScope())
                {
                    var databaseContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var entity = await databaseContext.Set<Core.Models.Booking>()
                                              .Include(r => r.Event)
                                              .Include(r => r.User)
                                              .Include(r => r.TicketType)
                                              .Where(x => x.Id.Equals(id))
                                              .SingleOrDefaultAsync();

                    var typeName = nameof(Core.Models.Booking);

                    HealthLogger.LogInformation($" Successfully retrieved {typeName} with the Id: '{entity?.Id} ");

                    return entity;
                }
            }
            catch (Exception ex)
            {
                HealthLogger.LogError(ex, " error at GetAsync with page number", id);

                throw;
            }
        }

        public async Task<Guid> AddAsync(Core.Models.Booking entity, 
            TicketType updateTicketQty, WaitingListEntry waitingListEntry)
        {
            try
            {
                using (var scope = ScopeFactory.CreateScope())
                {
                    var databaseContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    await databaseContext.Set<Core.Models.Booking>().AddAsync(entity);

                    if (updateTicketQty != null)
                    {
                        var databaseContextTicket = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        await databaseContextTicket.TicketTypes
                        .Where(p => p.Id == updateTicketQty.Id)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(p => p.QuantityAvailable, updateTicketQty.QuantityAvailable));
                    }

                    if (waitingListEntry != null)
                    {
                        waitingListEntry.BookingId = entity.Id;
                        await databaseContext.Set<WaitingListEntry>().AddAsync(waitingListEntry);
                    }

                    await databaseContext.SaveChangesAsync();
                }

                var typeName = nameof(Core.Models.Booking);
                HealthLogger.LogInformation($" Successfully Added {typeName}'s Id: '{entity?.Id} ");

                var _ = entity ?? throw new NullReferenceException();
                return entity.Id;
            }
            catch (Exception ex)
            {
                HealthLogger.LogError(ex, " error at AddAsync with page number", entity);

                throw;
            }
        }

        public async Task<Core.Models.Booking> GetBookingUserAsync(Guid eventId, Guid userId)
        {
            try
            {

                using (var scope = ScopeFactory.CreateScope())
                {
                    var databaseContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var result = await databaseContext.Set<Core.Models.Booking>().AsNoTracking()
                                                .FirstOrDefaultAsync(r => r.EventId == eventId
                                                && r.UserId == userId);

                    var typeName = nameof(Core.Models.Booking);
                    HealthLogger.LogInformation($" Successfully retrieved {typeName} ");

                    return result;
                }
            }
            catch (SqlNullValueException s)
            {
                HealthLogger.LogError(s, " SqlNullValueException at GetBookingUserAsync with page number");

                throw;
            }
            catch (Exception ex)
            {
                HealthLogger.LogError(ex, " error at GetBookingUserAsync with page number");

                throw;
            }
        }

        public async Task<Core.Models.Booking> GetLatestWaitingUserAsync(int qty, Guid id, Guid ticketId)
        {
            try
            {

                using (var scope = ScopeFactory.CreateScope())
                {
                    var databaseContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var result = await databaseContext.Set<Core.Models.Booking>().AsNoTracking()
                                                .Include(r => r.Event)
                                                .OrderByDescending(r => r.BookingDate)
                                                .FirstOrDefaultAsync(r => r.Status == BookingStatus.Pending
                                                && r.EventId==id && r.Quantity<=qty
                                                && r.TicketTypeId==ticketId);

                    var typeName = nameof(Core.Models.Booking);
                    HealthLogger.LogInformation($" Successfully retrieved {typeName} ");

                    return result;
                }
            }
            catch (SqlNullValueException s)
            {
                HealthLogger.LogError(s, " SqlNullValueException at GetLatestWaitingUserAsync with page number");

                throw;
            }
            catch (Exception ex)
            {
                HealthLogger.LogError(ex, " error at GetLatestWaitingUserAsync with page number");

                throw;
            }
        }

        public async Task<Guid> UpdateAllTablesRelatedAsync(Core.Models.Booking item, TicketType ticket, 
            WaitingListEntry updateWaitingList, Core.Models.Booking updateBookinWaiter)
        {
            try
            {
                using (var scope = ScopeFactory.CreateScope())
                {
                    //on a standard app we use transaction here
                    var databaseContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    await databaseContext.Bookings
                    .Where(p => p.Id == item.Id)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(p => p.Status, item.Status));

                    if(updateWaitingList != null)
                    {
                        var databaseContextWaitingList = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        await databaseContextWaitingList.WaitingListEntries
                        .Where(p => p.BookingId == updateWaitingList.BookingId)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(p => p.Notified, true));
                    }

                    if (updateBookinWaiter != null)
                    {
                        var databaseContextBookinWaiter = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        await databaseContextBookinWaiter.Bookings
                        .Where(p => p.Id == updateBookinWaiter.Id)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(p => p.Status, updateBookinWaiter.Status));
                    }

                    if (ticket != null)
                    {
                        var databaseContextTicket = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        await databaseContextTicket.TicketTypes
                        .Where(p => p.Id == ticket.Id)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(p => p.QuantityAvailable, ticket.QuantityAvailable));
                    }

                }

                var typeName = nameof(Core.Models.Booking);
                HealthLogger.LogInformation($" Successfully Updated {typeName}'s Id: '{item?.Id} ");

                return item.Id;
            }
            catch (Exception ex)
            {
                HealthLogger.LogError(ex, " error at UpdateAsync ", item);

                throw;
            }
        }
    }
}
