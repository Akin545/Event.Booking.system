using AutoMapper;

using Event.Booking.System.BusinessService.Interfaces;
using Event.Booking.System.BusinessService.Interfaces.Utilities;
using Event.Booking.System.Core.Dtos.Event;
using Event.Booking.System.Core.Dtos.TicketTypes;
using Event.Booking.System.Core.Exceptions;
using Event.Booking.System.Core.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using Swashbuckle.AspNetCore.Annotations;

using static Event.Booking.system.Controllers.BookingSystemControllerBase;

namespace Event.Booking.system.Controllers
{
   public class EventController : BookingControllerBase<EventDto, System.Core.Models.Event, IEventBusinessService>
    {

        public EventController(ILogger<System.Core.Models.Event> logger
           , IEventBusinessService businessService
           , IGlobalDateTimeSettings globalDateTimeBusinessServices
           , IMapper mapper
           , IConfiguration configuration
           , IGlobalService globalService
           , IServiceScopeFactory scopeFactory)
           : base(logger, businessService, globalDateTimeBusinessServices, mapper, configuration, globalService, scopeFactory)
        {

        }

        [HttpPost("Add-Event")]
        [Authorize]
        [SwaggerOperation(
        Summary = "Only admin can add event",
        Description = "Different types of event can be added by the admin which users can book",

        Tags = new[] { "Event" }
        )]
        public async Task<IActionResult> AddAsync([FromBody] CreateEventDto dto)
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

                    var result = MapDTOToEntityWithNoID<CreateEventDto, System.Core.Models.Event>(dto);
                    SetAuditInformation(result);

                    var response = await BusinessServiceManager.AddAsync(result);
                    return Ok(response);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (EventException ex)
            {
                var errorMessage = $"error at AddAsync of {nameof(EventController)}  ";
                HealthLogger.LogError(ex, errorMessage);
                return BadRequest(ex.Message);
            }
            catch (PermissionBaseException ex)
            {
                var errorMessage = $"error at AddAsync of {nameof(EventController)}";
                HealthLogger.LogError(ex, errorMessage);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                var errorMessage = $"error at AddAsync of {nameof(EventController)}";
                HealthLogger.LogError(ex, errorMessage);
                return StatusCode(500);
            }
        }

        [HttpGet()]
        [Route("{id}")]
        [Authorize]
        [SwaggerOperation(
        Summary = "Users can retrieve current event status",
        Description = "Users can retrieve current event status and other details related to an event ",

        Tags = new[] { "Event" }
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
            catch (EventException ex)
            {
                var errorMessage = $"error at GetAsync of {nameof(EventController)}  ";
                HealthLogger.LogError(ex, errorMessage);
                return BadRequest(ex.Message);
            }
            catch (PermissionBaseException ex)
            {
                var errorMessage = $"error at GetAsync of {nameof(EventController)}";
                HealthLogger.LogError(ex, errorMessage);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                var errorMessage = $"error at GetAsync of {nameof(EventController)}";
                HealthLogger.LogError(ex, errorMessage);
                return StatusCode(500);
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        [SwaggerOperation(
         Summary = "Only admin can update Event",
         Description = "Different types of event can be updated by the admin which users can book",
         Tags = new[] { "Event" }
         )]
        public async Task<IActionResult> UpdateAsync([FromBody] UpdateEventDto item, Guid id)
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    if (item != null)
                    {
                        var requestBody = $"update Event Request Body:  {JsonConvert.SerializeObject(item)}";
                        HealthLogger.LogInformation(requestBody);
                    }

                    if (item == null || !ModelState.IsValid)
                    {
                        return BadRequest($"Invalid update {nameof(TicketType)} State");
                    }

                    var eventDTO = MapperManager.Map<UpdateEventDto, EventDto>(item);

                    return await base.UpdateAsync(eventDTO, id);
                }
                else
                {
                    return Unauthorized();
                }

            }
            catch (EventException ex)
            {
                var errorMessage = $"error at UpdateAsync of {nameof(EventController)}  ";
                HealthLogger.LogError(ex, errorMessage);
                return BadRequest(ex.Message);
            }
            catch (PermissionBaseException ex)
            {
                var errorMessage = $"error at UpdateAsync of {nameof(EventController)}";
                HealthLogger.LogError(ex, errorMessage);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                var errorMessage = $"error at UpdateAsync of {nameof(EventController)}";
                HealthLogger.LogError(ex, errorMessage);
                return StatusCode(500);
            }
        }



        protected override System.Core.Models.Event SetUpdateAuditInformation(EventDto TDTO, System.Core.Models.Event entity)
        {

            var createdBy = entity.CreatedBy;
            var createdOn = entity.CreatedDate;

            System.Core.Models.Event copy = SetAllDbValuesIntoACopy(entity);

            MapperManager.Map(TDTO, entity);

            SetNonUpdatableDbEntityPropertiesFromCopy(entity, copy);

            ReplaceNullOnDtoWithDbValues(entity, copy);

            entity.CreatedBy = createdBy;
            entity.CreatedDate = createdOn;

            return entity;
        }


        private void ReplaceNullOnDtoWithDbValues(System.Core.Models.Event entity, System.Core.Models.Event dbCopy)
        {
            entity.Name = entity.Name ?? dbCopy.Name;
            entity.Status = entity.Status != 0 ? entity.Status : dbCopy.Status;
            entity.Description = entity.Description ?? dbCopy.Description;
            entity.Venue = entity.Venue ?? dbCopy.Venue;

            entity.Capacity = entity.Capacity < 1 ? dbCopy.Capacity : entity.Capacity;
            entity.StartDate = entity.StartDate == default(DateTime) || entity.StartDate == null
                ? dbCopy.StartDate : entity.StartDate;

            entity.EndDate = entity.EndDate == default(DateTime) || entity.EndDate == null
                ? dbCopy.EndDate : entity.EndDate;
        }

        private void SetNonUpdatableDbEntityPropertiesFromCopy(System.Core.Models.Event entity, System.Core.Models.Event dbCopy)
        {
            entity.Id = dbCopy.Id;
        }

        private System.Core.Models.Event SetAllDbValuesIntoACopy(System.Core.Models.Event entity)
        {
            var copy = new System.Core.Models.Event();

            copy.Id = entity.Id;
            copy.Description = entity.Description;
            copy.Name = entity.Name;
            copy.StartDate = entity.StartDate;
            copy.Venue = entity.Venue;
            copy.EndDate = entity.EndDate;
            copy.Status = entity.Status;
            copy.Capacity = entity.Capacity;

            return copy;
        }

    }
}
