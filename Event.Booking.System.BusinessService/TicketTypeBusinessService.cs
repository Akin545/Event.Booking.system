using Event.Booking.System.BusinessService.Interfaces;
using Event.Booking.System.BusinessService.Interfaces.Utilities;
using Event.Booking.System.Core.Exceptions;
using Event.Booking.System.Core.Models;
using Event.Booking.System.Repository.Interfaces;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event.Booking.System.BusinessService
{
   public class TicketTypeBusinessService : BusinessServiceBase<TicketType, ITicketTypeRepository>
       , ITicketTypeBusinessService
    {

        public TicketTypeBusinessService(ITicketTypeRepository repository
            , IGlobalDateTimeSettings globalDateTimeBusinessServices
            , ILogger<TicketType> logger
            , IGlobalService globalService
            , IServiceScopeFactory scopeFactory)
            : base(repository,
                globalDateTimeBusinessServices,
                logger,
                globalService,
                scopeFactory)
        {

        }

        public override async Task<Guid> AddAsync(TicketType item)
        {
            CheckIfNull(item);
            CheckIfAddedEntityHasId(item.Id);

            if (GlobalService.Roles!="Admin")
            {
                var errorMessage = $"You don't have {nameof(TicketType)} create permission";
                HealthLogger.LogError($"{errorMessage}");
                throw new TicketTypeException(errorMessage);
            }

            return await RepositoryManager.AddAsync(item);
        }

        public override async Task UpdateAsync(TicketType item)
        {
            CheckIfNull(item);
            ValidateId(item?.Id);

            if (GlobalService.Roles != "Admin")
            {
                var errorMessage = $"You don't have {nameof(TicketType)} update permission";
                HealthLogger.LogError($"{errorMessage}");
                throw new TicketTypeException(errorMessage);
            }

            await RepositoryManager.UpdateAsync(item);
        }

        public async Task<TicketType> GetEventByIdAsync(Guid id)
        {
            ValidateId(id);

            var result = await RepositoryManager.GetEventByIdAsync(id);
            return result;
        }

    }
}
