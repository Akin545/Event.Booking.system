using Event.Booking.System.BusinessService.Interfaces;
using Event.Booking.System.BusinessService.Interfaces.Utilities;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Event.Booking.System.BusinessService.Utilities
{
    
    public class GlobalService: IGlobalService
    {
        private readonly ILogger<GlobalService> _logger;
        protected readonly IHttpContextAccessor? _httpContextAccessor;
        public GlobalService(ILogger<GlobalService> logger
            , IConfiguration configuration
            , IGlobalDateTimeSettings globalDateTimeService
            , IServiceScopeFactory ScopeFactory
            , IHttpContextAccessor? httpContextAccessor)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }
        public bool IsValidEmail(string email)
        {
            return new EmailAddressAttribute().IsValid(email);
        }

        public string? Email
        {
            get
            {
                var result = _httpContextAccessor.HttpContext?
                    .User
                    .Claims.Where(r => r.Type == ClaimTypes.Email)
                    .FirstOrDefault();

                return result == null ? null : result.Value;
            }
        }

        public string? Roles
        {
            get
            {
                var result = _httpContextAccessor.HttpContext?
                    .User
                    .Claims.Where(r => r.Type == ClaimTypes.Role)
                    .FirstOrDefault();

                return result == null ? null : result.Value;
            }
        }

        public string? Name
        {
            get
            {
                var result = _httpContextAccessor.HttpContext?
                    .User?
                    .Identity?.Name;

                return result;
            }
        }

       
        public string? Id
        {
            get
            {
                var result = _httpContextAccessor.HttpContext?
                    .User?
                    .Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

                return result;
            }
        }

        public bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            // Remove spaces and common formatting characters
            var cleanNumber = phoneNumber.Trim().Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

            // Check if it matches international format
            var regex = new Regex(@"^\+?[1-9]\d{1,14}$");
            return regex.IsMatch(cleanNumber);
        }
    }

    
 }
