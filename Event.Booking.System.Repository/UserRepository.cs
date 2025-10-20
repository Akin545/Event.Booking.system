using Event.Booking.System.Core.Models;
using Event.Booking.System.Repository.Interfaces;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System.Data.SqlTypes;

namespace Event.Booking.System.Repository
{
    public class UserRepository : RepositoryBase<User>, IUserRepository
    {
        public UserRepository(IConfiguration configuration, ILogger<User> logger, IServiceScopeFactory scopeFactory)
            : base(configuration, logger, scopeFactory)
        {
        }


        public async Task<User> GetByEmailAsync(string email)
        {
            try
            {
                using (var scope = ScopeFactory.CreateScope())
                {
                    using (var databaseContext = scope.ServiceProvider
                                                   .GetRequiredService<AppDbContext>())
                    {
                        CheckConnection(databaseContext);

                        var entity = await databaseContext.Set<User>()
                                              .Where(x => x.Email.ToLower().Equals(email.ToLower()))
                                              .FirstOrDefaultAsync();

                        var typeName = nameof(User);

                        HealthLogger.LogInformation($" Successfully retrieved {typeName} from GetByEmailAsync with the Id: '{entity?.Id} ");
                        return entity;
                    }
                }
            }
            catch (SqlNullValueException s)
            {
                HealthLogger.LogError(s, " SqlNullValueException at GetByEmailAsync");

                throw;
            }
            catch (Exception ex)
            {
                HealthLogger.LogError(ex, $" error at GetByEmailAsync from {nameof(UserRepository)} with id: ", email);
                throw;
            }
        }

    }
}
