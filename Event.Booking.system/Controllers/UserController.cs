using AutoMapper;

using Event.Booking.System.BusinessService.Interfaces;
using Event.Booking.System.BusinessService.Interfaces.Utilities;
using Event.Booking.System.Core.Dtos.User;
using Event.Booking.System.Core.Exceptions;
using Event.Booking.System.Core.Models;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using Swashbuckle.AspNetCore.Annotations;

using static Event.Booking.system.Controllers.BookingSystemControllerBase;

namespace Event.Booking.system.Controllers
{
    public class UsersController : BookingControllerBase<UserDto, User, IUserBusinessService>
    {

        public UsersController(ILogger<User> logger
           , IUserBusinessService businessService
           , IGlobalDateTimeSettings globalDateTimeBusinessServices
           , IMapper mapper
           , IConfiguration configuration
           , IGlobalService globalService
           , IServiceScopeFactory scopeFactory)
           : base(logger, businessService, globalDateTimeBusinessServices, mapper, configuration, globalService, scopeFactory)
        {

        }

        [HttpPost("Register")]
        
        [SwaggerOperation(
         Summary = "Registration section for both Users only.",
         Description = "Though this is just a basic authentication. Admin are given secured privilege" +
            "which can be found in the readme file",
         Tags = new[] { "Login" }
         )]
        public async Task<IActionResult> AddAsync([FromBody] CreateUserDto dto)
        {
            try
            {
                if (!ModelState.IsValid || dto == null)
                {
                    return BadRequest("Invalid User State");
                }

                // i want to see what was passed incase of an error in my logger file
                if (dto != null)
                {
                    var requestBody = $"create  Request Body:  {JsonConvert.SerializeObject(dto)}";
                    HealthLogger.LogInformation(requestBody);
                }

                var result = MapDTOToEntityWithNoID<CreateUserDto, User>(dto);
                SetAuditInformation(result);

                var response = await BusinessServiceManager.AddAsync(result);
                return Ok(response);
            }
            catch (UserException ex)
            {
                var errorMessage = $"error at AddAsync of {nameof(UsersController)}  ";
                HealthLogger.LogError(ex, errorMessage);
                return BadRequest(ex.Message);
            }
            catch (PermissionBaseException ex)
            {
                var errorMessage = $"error at AddAsync of {nameof(UsersController)}";
                HealthLogger.LogError(ex, errorMessage);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                var errorMessage = $"error at AddAsync of {nameof(UsersController)}";
                HealthLogger.LogError(ex, errorMessage);
                return StatusCode(500);
            }
        }

        [HttpPost("Login")]
        [SwaggerOperation(
         Summary = "Login section for both Users and Admin",
         Description = "Allows an admin to create create an event and perform other sensitive operations" +
            "while users login to book and see their current status after booking",
         Tags = new[] { "Login" }
         )]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {

                if (loginDto != null)
                {
                    var requestBody = $"create  Request Body:  {JsonConvert.SerializeObject(loginDto)}";
                    HealthLogger.LogInformation(requestBody);
                }

                if (loginDto == null || !ModelState.IsValid)
                {
                    return BadRequest("Invalid Login State");
                }

                var result = MapDTOToEntityWithNoID<LoginDto, User>(loginDto);
                SetAuditInformation(result);
                var token = await BusinessServiceManager.LoginAsync(result);

                return Ok(token);

            }
            catch (UserException ex)
            {
                var errorMessage = $"error at Login of {nameof(UsersController)}  ";
                HealthLogger.LogError(ex, errorMessage);
                return BadRequest(ex.Message);
            }
            catch (PermissionBaseException ex)
            {
                var errorMessage = $"error at Login of {nameof(UsersController)} ";
                HealthLogger.LogError(ex, errorMessage);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                var errorMessage = $"error at Login of {nameof(UsersController)} ";
                HealthLogger.LogError(ex, errorMessage);
                return StatusCode(500);
            }
        }

    }
}
