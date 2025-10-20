using Event.Booking.System.BusinessService.Interfaces;
using Event.Booking.System.BusinessService.Interfaces.Utilities;
using Event.Booking.System.Core.Dtos.Auth;
using Event.Booking.System.Core.Exceptions;
using Event.Booking.System.Core.Models;
using Event.Booking.System.Repository.Interfaces;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event.Booking.System.BusinessService
{
   public class WaitingListEntryBusinessService : BusinessServiceBase<WaitingListEntry, IWaitingListEntryRepository>
       , IWaitingListEntryBusinessService
    {
       
        public WaitingListEntryBusinessService(IWaitingListEntryRepository repository
            , IGlobalDateTimeSettings globalDateTimeBusinessServices
            , ILogger<WaitingListEntry> logger
            , IGlobalService globalService
            , IServiceScopeFactory scopeFactory)
            : base(repository,
                globalDateTimeBusinessServices,
                logger,
                globalService,
                scopeFactory)
        {
          
        }

        public async Task<List<WaitingListEntry>> ListAsync(int pageNumber, Guid eventId)
        {
            var result = await RepositoryManager.ListAsync(pageNumber, eventId);
            return result;
        }

        public override async Task<Guid> AddAsync(WaitingListEntry item)
        {
            CheckIfNull(item);
            CheckIfAddedEntityHasId(item.Id);

            return await RepositoryManager.AddAsync(item);
        }

        public async Task<int> CountWaitingListAsync(Guid eventId)
        {
            if(eventId==default(Guid) || eventId==Guid.Empty)
            {
                var errorMessage = $"Event Id not valid {eventId}";
                HealthLogger.LogError($"{errorMessage}");
                throw new WaitingListEntryException(errorMessage);
            }

            return await RepositoryManager.CountWaitingListAsync(eventId);
        }
    }
}
