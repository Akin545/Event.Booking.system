using AutoMapper;

using Event.Booking.System.BusinessService.Interfaces;
using Event.Booking.System.BusinessService.Interfaces.Utilities;
using Event.Booking.System.Core.Dtos.Bookings;
using Event.Booking.System.Core.Dtos.WaitingListEntry;
using Event.Booking.System.Core.Exceptions;
using Event.Booking.System.Core.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Swashbuckle.AspNetCore.Annotations;

using static Event.Booking.system.Controllers.BookingSystemControllerBase;

namespace Event.Booking.system.Controllers
{
  public class WaitingListEntryController : BookingControllerBase<WaitingListDto, WaitingListEntry, IWaitingListEntryBusinessService>
  {

        public WaitingListEntryController(ILogger<WaitingListEntry> logger
           , IWaitingListEntryBusinessService businessService
           , IGlobalDateTimeSettings globalDateTimeBusinessServices
           , IMapper mapper
           , IConfiguration configuration
           , IGlobalService globalService
           , IServiceScopeFactory scopeFactory)
           : base(logger, businessService, globalDateTimeBusinessServices, mapper, configuration, globalService, scopeFactory)
        {

        }

       

        [HttpGet("{eventId}")]
        [Authorize]
        [SwaggerOperation(
        Summary = "View and get current waiting list based on event id with total count",

        Tags = new[] { "WaitingListEntry" }
        )]
        public async Task<IActionResult> ListAsync(int pageNumber,Guid eventId)
        {
            var name = nameof(WaitingListEntry);

            try
            {

                if (User.Identity.IsAuthenticated)
                {
                    CurrentPageNumber = pageNumber;

                    var entities = await BusinessServiceManager.ListAsync(pageNumber, eventId);
                    CurrentPageNumber = 0;

                    if (entities == null || !entities.Any())
                    {
                        return NotFound();
                    }

                    int count = await BusinessServiceManager.CountWaitingListAsync(eventId);

                    var result = MapperManager.Map<List<WaitingListDto>>(entities);

                    WaitingListResponseDTO<WaitingListDto> response = new WaitingListResponseDTO<WaitingListDto>
                    {
                        MaxLength = count,
                        Results = result
                    };

                    return Ok(response);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (UserException ex)
            {
                var errorMessage = $"error at ListAsync of {nameof(WaitingListEntryController)}  ";
                HealthLogger.LogError(ex, errorMessage);
                return BadRequest(ex.Message);
            }
            catch (PermissionBaseException ex)
            {
                var errorMessage = $"error at ListAsync of {nameof(WaitingListEntryController)} ";
                HealthLogger.LogError(ex, errorMessage);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                var errorMessage = $"error at ListAsync of {nameof(WaitingListEntryController)} ";
                HealthLogger.LogError(ex, errorMessage);
                return StatusCode(500);
            }
        }

    }
}
