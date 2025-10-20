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
using System.Text;
using System.Threading.Tasks;

namespace Event.Booking.System.Repository
{

     public class WaitingListEntryRepository : RepositoryBase<WaitingListEntry>, IWaitingListEntryRepository
    {
        public WaitingListEntryRepository(IConfiguration configuration, ILogger<WaitingListEntry> logger, IServiceScopeFactory scopeFactory)
            : base(configuration, logger, scopeFactory)
        {
        }

        public async Task<int> CountWaitingListAsync(Guid eventId)
        {
            try
            {

                using (var scope = ScopeFactory.CreateScope())
                {
                    using (var databaseContext = scope.ServiceProvider
                                                   .GetRequiredService<AppDbContext>())
                    {
                        CheckConnection(databaseContext);

                        var query = databaseContext.Set<WaitingListEntry>()
                                               .AsQueryable();

                        int counts = await query.Where(r => r.EventId == eventId
                        && r.Notified == false).CountAsync();

                        var typeName = nameof(WaitingListEntry);
                        HealthLogger.LogInformation($" Successfully retrieved {typeName} count");

                        return counts;
                    }
                }
            }
            catch (Exception ex)
            {
                HealthLogger.LogError(ex, " error at Counting  WaitingListEntry");
                throw;
            }
        }

        public async Task<List<WaitingListEntry>> ListAsync(int pageNumber, Guid eventId)
        {
            try
            {
                CurrentPageNumber = pageNumber;

                using (var scope = ScopeFactory.CreateScope())
                {
                    var databaseContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    List<WaitingListEntry> result = await databaseContext.Set<WaitingListEntry>().AsNoTracking()
                                                .Include(r=>r.Event)
                                                .Include(r => r.User)
                                                .Where(r=>r.EventId==eventId && r.Notified==false)
                                                .OrderByDescending(r => r.RequestedAt)
                                                .Skip(SkippedDbRecordSize)
                                                .Take(MaxPageSize)
                                                .ToListAsync();

                    var typeName = nameof(WaitingListEntry);
                    HealthLogger.LogInformation($" Successfully retrieved {typeName}'s List with page number {pageNumber} ");
                    CurrentPageNumber = 0;

                    return result;
                }
            }
            catch (SqlNullValueException s)
            {
                HealthLogger.LogError(s, " SqlNullValueException at ListAsync with page number", pageNumber);

                throw;
            }
            catch (Exception ex)
            {
                HealthLogger.LogError(ex, " error at ListAsync with page number", pageNumber);

                throw;
            }
        }


    }
}
