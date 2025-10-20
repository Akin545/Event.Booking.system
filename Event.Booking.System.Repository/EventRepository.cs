using Event.Booking.System.Repository.Interfaces;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event.Booking.System.Repository
{
    public class EventRepository : RepositoryBase<Core.Models.Event>, IEventRepository
    {
        public EventRepository(IConfiguration configuration, ILogger<Core.Models.Event> logger, IServiceScopeFactory scopeFactory)
            : base(configuration, logger, scopeFactory)
        {
        }




        public override async Task<Core.Models.Event> GetAsync(Guid id)
        {
            try
            {
                using (var scope = ScopeFactory.CreateScope())
                {
                    var databaseContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var entity = await databaseContext.Set<Core.Models.Event>()
                                              .Include(r => r.TicketTypes)
                                              .Where(x => x.Id.Equals(id))
                                              .FirstOrDefaultAsync();

                    var typeName = Entity?.GetType()?.Name;


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
    }
}
