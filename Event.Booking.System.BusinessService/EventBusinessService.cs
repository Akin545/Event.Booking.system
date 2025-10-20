using Event.Booking.System.BusinessService.Interfaces;
using Event.Booking.System.BusinessService.Interfaces.Utilities;
using Event.Booking.System.Core.Enums;
using Event.Booking.System.Core.Exceptions;
using Event.Booking.System.Core.Models;
using Event.Booking.System.Repository.Interfaces;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Event.Booking.System.BusinessService
{
    public class EventBusinessService : BusinessServiceBase<Core.Models.Event, IEventRepository>
       , IEventBusinessService
    {

        public EventBusinessService(IEventRepository repository
            , IGlobalDateTimeSettings globalDateTimeBusinessServices
            , ILogger<Core.Models.Event> logger
            , IGlobalService globalService
            , IServiceScopeFactory scopeFactory)
            : base(repository,
                globalDateTimeBusinessServices,
                logger,
                globalService,
                scopeFactory)
        {

        }

        public override async Task<Guid> AddAsync(Core.Models.Event item)
        {
            CheckIfNull(item);
            CheckIfAddedEntityHasId(item.Id);

            if (item.EndDate < item.StartDate)
            {
                var errorMessage = $"Invalid date setup. End date must be lessthan start date";
                HealthLogger.LogError($"{errorMessage}");
                throw new EventException(errorMessage);
            }

            if (GlobalService.Roles != "Admin")
            {
                var errorMessage = $"You don't have {nameof(Core.Models.Event)} create permission";
                HealthLogger.LogError($"{errorMessage}");
                throw new EventException(errorMessage);
            }

            item.Status = EventStatusEnum.NotStarted;

            return await RepositoryManager.AddAsync(item);
        }

        public override async Task UpdateAsync(Core.Models.Event item)
        {
            CheckIfNull(item);
            ValidateId(item?.Id);

            if(item.EndDate<item.StartDate)
            {
                var errorMessage = $"Invalid date setup. End date must be lessthan start date";
                HealthLogger.LogError($"{errorMessage}");
                throw new EventException(errorMessage);
            }

            if (GlobalService.Roles != "Admin")
            {
                var errorMessage = $"You don't have {nameof(TicketType)} update permission";
                HealthLogger.LogError($"{errorMessage}");
                throw new EventException(errorMessage);
            }

            await RepositoryManager.UpdateAsync(item);
        }

        public override async Task<Core.Models.Event> GetAsync(Guid id)
        {
            ValidateId(id);
            var result = await RepositoryManager.GetAsync(id);

            return result;
        }

    }
}
