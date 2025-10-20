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
    public class TicketTypeRepository : RepositoryBase<TicketType>, ITicketTypeRepository
    {
        public TicketTypeRepository(IConfiguration configuration, ILogger<TicketType> logger, IServiceScopeFactory scopeFactory)
            : base(configuration, logger, scopeFactory)
        {
        }

        public async Task<TicketType> GetEventByIdAsync(Guid id)
        {
            try
            {
                using (var scope = ScopeFactory.CreateScope())
                {
                    var databaseContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var entity = await databaseContext.Set<TicketType>()
                                              .Include(r=>r.Event)
                                              .Where(x => x.EventId.Equals(id))
                                              .FirstOrDefaultAsync();

                    var typeName = nameof(TicketType);


                    HealthLogger.LogInformation($" Successfully retrieved {typeName} with the Id: '{entity?.Id} ");

                    return entity;
                }
            }
            catch (SqlNullValueException s)
            {
                HealthLogger.LogError(s, " SqlNullValueException at GetEventByIdAsync");

                throw;
            }
            catch (Exception ex)
            {
                HealthLogger.LogError(ex, " error at GetEventByIdAsync with page number", id);

                throw;
            }
        }
    }
}
