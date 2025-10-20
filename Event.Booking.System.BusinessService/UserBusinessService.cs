using Event.Booking.System.BusinessService.Interfaces;
using Event.Booking.System.BusinessService.Interfaces.Utilities;
using Event.Booking.System.Core.Dtos.Auth;
using Event.Booking.System.Core.Exceptions;
using Event.Booking.System.Core.Models;
using Event.Booking.System.Repository.Interfaces;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Event.Booking.System.BusinessService
{
    public class UserBusinessService : BusinessServiceBase<User, IUserRepository>
       , IUserBusinessService
    {
        private readonly JwtSettings _jwtSettings;
       
        public UserBusinessService(IUserRepository repository
            , IGlobalDateTimeSettings globalDateTimeBusinessServices
            , ILogger<User> logger
            , IGlobalService globalService
            , IServiceScopeFactory scopeFactory
            , JwtSettings jwtSettings)
            : base(repository,
                globalDateTimeBusinessServices,
                logger,
                globalService,
                scopeFactory)
        {
            _jwtSettings= jwtSettings;
            
        }

        public async Task<string> LoginAsync(User item)
        {
            var user = await GetByEmailAsync(item.Email);
            if (user == null)
            {
                var errorMessage = $"InInvalid credentials.";
                HealthLogger.LogError($"{errorMessage}; ");
                throw new UserException(errorMessage);
            }
             
            if (user.PasswordHash != item.PasswordHash)
            {
                 var errorMessage = $"InInvalid credentials.";
                 HealthLogger.LogError($"{errorMessage}; ");
                 throw new UserException(errorMessage);
            }

            return GenerateToken(user);
        }

        public override async Task<Guid> AddAsync(User item)
        {
            CheckIfNull(item);
            CheckIfAddedEntityHasId(item.Id);

            var user = await GetByEmailAsync(item.Email);
            if (user != null)
            {
                var errorMessage = $"Email already registered";
                HealthLogger.LogError($"{errorMessage}; ");
                throw new UserException(errorMessage);
            }

            var id = await RepositoryManager.AddAsync(item);
            return id;
        }

        public virtual async Task<User> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                var errorMessage = $"Invalid email";
                HealthLogger.LogError($"{errorMessage}; ");
                throw new UserException(errorMessage);
            }

            var result = await RepositoryManager.GetByEmailAsync(email);

            if (result == null)
            {
                return null;
            }

            return result;
        }

        public virtual string GenerateToken(User user)
        {
            try
            {
                var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);
                var tokenHandler = new JwtSecurityTokenHandler();

                var claims = new[]
                {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Roles.ToString()),
                new Claim(ClaimTypes.Name, user.FullName)
            };

                var descriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresMinutes),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                    Issuer = _jwtSettings.Issuer,
                    Audience = _jwtSettings.Audience
                };

                var token = tokenHandler.CreateToken(descriptor);
                var result = tokenHandler.WriteToken(token);

                return result;
            }
            catch(Exception ex) 
            {
                var errorMessage = $"ILogin failed. Please try again";
                HealthLogger.LogError($"{errorMessage}; {ex}");
                throw new UserException(errorMessage);
            }         
        }

    }

}