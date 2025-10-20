using AutoMapper;

using Event.Booking.System.BusinessService.Interfaces;
using Event.Booking.System.BusinessService.Interfaces.Utilities;
using Event.Booking.System.Core.Dtos.Event;
using Event.Booking.System.Core.Dtos.TicketTypes;
using Event.Booking.System.Core.Dtos.User;
using Event.Booking.System.Core.Exceptions;
using Event.Booking.System.Core.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using Swashbuckle.AspNetCore.Annotations;

using static Event.Booking.system.Controllers.BookingSystemControllerBase;

namespace Event.Booking.system.Controllers
{
    public class TicketTypeController : BookingControllerBase<TicketTypeDto, System.Core.Models.TicketType, ITicketTypeBusinessService>
    {

        public TicketTypeController(ILogger<System.Core.Models.TicketType> logger
           , ITicketTypeBusinessService businessService
           , IGlobalDateTimeSettings globalDateTimeBusinessServices
           , IMapper mapper
           , IConfiguration configuration
           , IGlobalService globalService
           , IServiceScopeFactory scopeFactory)
           : base(logger, businessService, globalDateTimeBusinessServices, mapper, configuration, globalService, scopeFactory)
        {

        }

        [HttpPost("Add-Ticket-Types")]
        [Authorize]
        [SwaggerOperation(
         Summary = "Only admin can add ticket types",
         Description = "Ticket types can come in different forms. from vips, regulars and the likes",
            
         Tags = new[] { "TicketType" }
         )]
        public async Task<IActionResult> AddAsync([FromBody] CreateTicketTypeDto dto)
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

                    var result = MapDTOToEntityWithNoID<CreateTicketTypeDto, TicketType>(dto);
                    SetAuditInformation(result);

                    var response = await BusinessServiceManager.AddAsync(result);
                    return Ok(response);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (TicketTypeException ex)
            {
                var errorMessage = $"error at AddAsync of {nameof(TicketTypeController)}  ";
                HealthLogger.LogError(ex, errorMessage);
                return BadRequest(ex.Message);
            }
            catch (PermissionBaseException ex)
            {
                var errorMessage = $"error at AddAsync of {nameof(TicketTypeController)}";
                HealthLogger.LogError(ex, errorMessage);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                var errorMessage = $"error at AddAsync of {nameof(TicketTypeController)}";
                HealthLogger.LogError(ex, errorMessage);
                return StatusCode(500);
            }
        }

        [HttpGet()]
        [Route("{id}")]
        [Authorize]
        [SwaggerOperation(
         Summary = "Users can view available ticket types",
         Description = "Ticket types can come in different forms. from vips, regulars and the likes",

         Tags = new[] { "TicketType" }
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
            catch (TicketTypeException ex)
            {
                var errorMessage = $"error at GetAsync of {nameof(TicketTypeController)}  ";
                HealthLogger.LogError(ex, errorMessage);
                return BadRequest(ex.Message);
            }
            catch (PermissionBaseException ex)
            {
                var errorMessage = $"error at GetAsync of {nameof(TicketTypeController)}";
                HealthLogger.LogError(ex, errorMessage);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                var errorMessage = $"error at GetAsync of {nameof(TicketTypeController)}";
                HealthLogger.LogError(ex, errorMessage);
                return StatusCode(500);
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        [SwaggerOperation(
         Summary = "Only admin can update ticket types",
         Description = "Ticket types can come in different forms. from vips, regulars and the likes",

         Tags = new[] { "TicketType" }
         )]
        public async Task<IActionResult> UpdateAsync([FromBody] UpdateTicketTypeDto item, Guid id)
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    if (item != null)
                    {
                        var requestBody = $"update UpdateTicketType Request Body:  {JsonConvert.SerializeObject(item)}";
                        HealthLogger.LogInformation(requestBody);
                    }

                    if (item == null || !ModelState.IsValid)
                    {
                        return BadRequest($"Invalid update {nameof(TicketType)} State");
                    }

                    var ticketTypeDTO = MapperManager.Map<UpdateTicketTypeDto, TicketTypeDto>(item);
                               
                    return await base.UpdateAsync(ticketTypeDTO, id);
                }
                else
                {
                    return Unauthorized();
                }

            }
            catch (TicketTypeException ex)
            {
                var errorMessage = $"error at UpdateAsync of {nameof(TicketTypeController)}  ";
                HealthLogger.LogError(ex, errorMessage);
                return BadRequest(ex.Message);
            }
            catch (PermissionBaseException ex)
            {
                var errorMessage = $"error at UpdateAsync of {nameof(TicketTypeController)}";
                HealthLogger.LogError(ex, errorMessage);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                var errorMessage = $"error at UpdateAsync of {nameof(TicketTypeController)}";
                HealthLogger.LogError(ex, errorMessage);
                return StatusCode(500);
            }
        }



        protected override TicketType SetUpdateAuditInformation(TicketTypeDto TDTO, TicketType entity)
        {
            
            var createdBy = entity.CreatedBy;
            var createdOn = entity.CreatedDate;

            TicketType copy = SetAllDbValuesIntoACopy(entity);

            MapperManager.Map(TDTO, entity);

            SetNonUpdatableDbEntityPropertiesFromCopy(entity, copy);

            ReplaceNullOnDtoWithDbValues(entity, copy);

            entity.CreatedBy = createdBy;
            entity.CreatedDate = createdOn;

            return entity;
        }


        private void ReplaceNullOnDtoWithDbValues(TicketType entity, TicketType dbCopy)
        {
            entity.Name = entity.Name ?? dbCopy.Name;

            entity.Price = entity.Price < 1 ? dbCopy.Price : entity.Price;
            entity.QuantityAvailable = entity.QuantityAvailable < 1 ? dbCopy.QuantityAvailable : entity.QuantityAvailable;
        }

        private void SetNonUpdatableDbEntityPropertiesFromCopy(TicketType entity, TicketType dbCopy)
        {
            entity.Id = dbCopy.Id;
            entity.EventId = dbCopy.EventId;
        }

        private TicketType SetAllDbValuesIntoACopy(TicketType entity)
        {
            var copy = new TicketType();

            copy.Id = entity.Id;
            copy.QuantityAvailable = entity.QuantityAvailable;
            copy.Name = entity.Name;
            copy.EventId = entity.EventId;

            copy.Price = entity.Price;
            
            return copy;
        }

    }
}
