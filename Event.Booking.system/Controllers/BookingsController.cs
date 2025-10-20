using AutoMapper;

using Event.Booking.System.BusinessService.Interfaces;
using Event.Booking.System.BusinessService.Interfaces.Utilities;
using Event.Booking.System.Core.Dtos.Bookings;
using Event.Booking.System.Core.Dtos.Event;
using Event.Booking.System.Core.Enums;
using Event.Booking.System.Core.Exceptions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using Swashbuckle.AspNetCore.Annotations;

using static Event.Booking.system.Controllers.BookingSystemControllerBase;

namespace Event.Booking.system.Controllers
{
    public class BookingsController : BookingControllerBase<BookingDto, System.Core.Models.Booking, IBookingBusinessService>
    {

        public BookingsController(ILogger<System.Core.Models.Booking> logger
           , IBookingBusinessService businessService
           , IGlobalDateTimeSettings globalDateTimeBusinessServices
           , IMapper mapper
           , IConfiguration configuration
           , IGlobalService globalService
           , IServiceScopeFactory scopeFactory)
           : base(logger, businessService, globalDateTimeBusinessServices, mapper, configuration, globalService, scopeFactory)
        {

        }

        [HttpPost()]
        [Authorize]
        [SwaggerOperation(
       Summary = "users can book for an event",
       Description = "if there are no available tickets. They are automatically placed on a waiting list",

       Tags = new[] { "Booking" }
       )]
        public async Task<IActionResult> AddAsync([FromBody] CreateBookingDto dto)
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    if (!ModelState.IsValid || dto == null)
                    {
                        return BadRequest("Invalid CreateTicketType State");
                    }

                    // i want to see what was passed incase of an error in my logger file
                    if (dto != null)
                    {
                        var requestBody = $"create  Request Body:  {JsonConvert.SerializeObject(dto)}";
                        HealthLogger.LogInformation(requestBody);
                    }

                    var result = MapDTOToEntityWithNoID<CreateBookingDto, System.Core.Models.Booking>(dto);
                    SetAuditInformation(result);

                    var response = await BusinessServiceManager.AddAsync(result);
                    return Ok(response);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (BookingException ex)
            {
                var errorMessage = $"error at AddAsync of {nameof(BookingsController)}  ";
                HealthLogger.LogError(ex, errorMessage);
                return BadRequest(ex.Message);
            }
            catch (PermissionBaseException ex)
            {
                var errorMessage = $"error at AddAsync of {nameof(BookingsController)}";
                HealthLogger.LogError(ex, errorMessage);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                var errorMessage = $"error at AddAsync of {nameof(BookingsController)}";
                HealthLogger.LogError(ex, errorMessage);
                return StatusCode(500);
            }
        }

        [HttpPut("Cancel/{bookingId}")]
        [Authorize]
        [SwaggerOperation(
       Summary = "users can cancel ticket booked for an event by simply entering booking Id",
       Description = "This is done so because a user migth have multiple tickets booked and may one to cancel one",

       Tags = new[] { "Booking" }
       )]
        public async Task<IActionResult> CancelTicketAsync(Guid  bookingId)
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    var booking=await BusinessServiceManager.GetAsync(bookingId);

                    var result=await BusinessServiceManager.UpdateAsync(booking);

                    return Ok(result);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (BookingException ex)
            {
                var errorMessage = $"error at CancelTicketAsync of {nameof(BookingsController)}  ";
                HealthLogger.LogError(ex, errorMessage);
                return BadRequest(ex.Message);
            }
            catch (PermissionBaseException ex)
            {
                var errorMessage = $"error at CancelTicketAsync of {nameof(BookingsController)}";
                HealthLogger.LogError(ex, errorMessage);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                var errorMessage = $"error at CancelTicketAsync of {nameof(BookingsController)}";
                HealthLogger.LogError(ex, errorMessage);
                return StatusCode(500);
            }
        }

        [HttpGet()]
        [Route("{id}")]
        [Authorize]
        [SwaggerOperation(
        Summary = "users can get to view their current bookings status",
      
        Tags = new[] { "Booking" }
        )]
        public new async Task<IActionResult> GetAsync(Guid id)
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    return await base.GetAsync(id);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (BookingException ex)
            {
                var errorMessage = $"error at GetAsync of {nameof(BookingsController)}  ";
                HealthLogger.LogError(ex, errorMessage);
                return BadRequest(ex.Message);
            }
            catch (PermissionBaseException ex)
            {
                var errorMessage = $"error at GetAsync of {nameof(BookingsController)}";
                HealthLogger.LogError(ex, errorMessage);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                var errorMessage = $"error at GetAsync of {nameof(BookingsController)}";
                HealthLogger.LogError(ex, errorMessage);
                return StatusCode(500);
            }
        }


        protected override System.Core.Models.Booking SetUpdateAuditInformation(BookingDto TDTO, System.Core.Models.Booking entity)
        {

            var createdBy = entity.CreatedBy;
            var createdOn = entity.CreatedDate;

            System.Core.Models.Booking copy = SetAllDbValuesIntoACopy(entity);

            MapperManager.Map(TDTO, entity);

            SetNonUpdatableDbEntityPropertiesFromCopy(entity, copy);

            ReplaceNullOnDtoWithDbValues(entity, copy);

            entity.CreatedBy = createdBy;
            entity.CreatedDate = createdOn;

            return entity;
        }


        private void ReplaceNullOnDtoWithDbValues(System.Core.Models.Booking entity, System.Core.Models.Booking dbCopy)
        {
            entity.Status = BookingStatus.Cancelled;
        }

        private void SetNonUpdatableDbEntityPropertiesFromCopy(System.Core.Models.Booking entity, System.Core.Models.Booking dbCopy)
        {
            entity.Id = dbCopy.Id;
            entity.Quantity = dbCopy.Quantity;
            entity.EventId = dbCopy.EventId;
            entity.BookingDate = dbCopy.BookingDate;
            entity.UserId = dbCopy.UserId;
        }

        private System.Core.Models.Booking SetAllDbValuesIntoACopy(System.Core.Models.Booking entity)
        {
            var copy = new System.Core.Models.Booking();

            copy.Id = entity.Id;
            copy.BookingDate = entity.BookingDate;
            copy.Quantity = entity.Quantity;
            copy.Status = entity.Status;
            copy.EventId = entity.EventId;
           
            copy.UserId = entity.UserId;

            return copy;
        }


    }
}
