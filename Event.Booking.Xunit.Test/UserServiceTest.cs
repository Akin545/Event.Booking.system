using Event.Booking.System.BusinessService;
using Event.Booking.System.BusinessService.Interfaces;
using Event.Booking.System.BusinessService.Interfaces.Utilities;
using Event.Booking.System.BusinessService.Utilities;
using Event.Booking.System.Core.Dtos.Auth;
using Event.Booking.System.Core.Dtos.User;
using Event.Booking.System.Core.Exceptions;
using Event.Booking.System.Core.Models;
using Event.Booking.System.Repository.Interfaces;

using FluentAssertions;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;

namespace Event.Booking.Xunit.Test
{
    public class UserServiceTest 
    {

        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IUserBusinessService> _userBusinessServiceMock;
        private readonly Mock<IGlobalService> _globalServiceMock;
        private readonly Mock<IServiceScopeFactory> _serviceScopeMock;
        private readonly Mock<JwtSettings> _jwtSettingsMock;
       
        private readonly Mock<ILogger<User>> _loggerMock;
        private readonly Mock<UserBusinessService> _userBusinessService;
        private readonly Mock<IGlobalDateTimeSettings> _globalDateTimeSettingsMock;

        private readonly UserBusinessService _service;
        public UserServiceTest()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _userBusinessServiceMock = new Mock<IUserBusinessService>();
            _globalDateTimeSettingsMock= new Mock<IGlobalDateTimeSettings>();
            _loggerMock= new Mock<ILogger<User>>();
            _globalServiceMock= new Mock<IGlobalService>();
           
            //_serviceScopeMock = new Mock<IServiceScopeFactory>();
            _jwtSettingsMock= new Mock<JwtSettings>();

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IGlobalService)))
                .Returns(_globalServiceMock.Object);

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock
                .Setup(s => s.ServiceProvider)
                .Returns(serviceProviderMock.Object);

            _serviceScopeMock = new Mock<IServiceScopeFactory>();
            _serviceScopeMock
                .Setup(sf => sf.CreateScope())
                .Returns(serviceScopeMock.Object);

            _userBusinessService = new Mock<UserBusinessService>(_userRepoMock.Object,
                 _globalDateTimeSettingsMock.Object, _loggerMock.Object,
                 _globalServiceMock.Object, _serviceScopeMock.Object
                 , _jwtSettingsMock.Object)
            {
                CallBase = true
            };

            _service = new UserBusinessService(_userRepoMock.Object,
                 _globalDateTimeSettingsMock.Object, _loggerMock.Object,
                 _globalServiceMock.Object, _serviceScopeMock.Object
                 , _jwtSettingsMock.Object);
           
        }


        [Fact]
        public async Task Login_Should_Return_Token_When_Credentials_Are_Valid()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), Email = "user@example.com", PasswordHash = "12345" };
            var model = new User { Email = "user@example.com", PasswordHash = "12345" };
            var expectedToken = "real-jwt-token";
            _userBusinessService.Setup(s => s.GetByEmailAsync(user.Email))
                            .ReturnsAsync(user);

            _userBusinessService.Setup(s => s.GenerateToken(model))
               .Returns(expectedToken);

            // Act
            var result = await _userBusinessService.Object.LoginAsync(user);

            // Assert
            Assert.Equal(expectedToken, result);
        }

        [Fact]
        public async Task LoginAsync_Should_Throw_UserException_When_User_Not_Found()
        {
            // Arrange
            var user = new User { Email = "notfound@email.com", PasswordHash = "123" };

            _userBusinessService.Setup(s => s.GetByEmailAsync(user.Email))
                            .ReturnsAsync((User)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<UserException>(() =>
                _userBusinessService.Object.LoginAsync(user));

            Assert.Contains("InInvalid credentials", ex.Message);
        }

        [Fact]
        public async Task LoginAsync_Should_Throw_UserException_When_Password_Invalid()
        {
            // Arrange
            var user = new User { Email = "test@email.com", PasswordHash = "wrong" };
            var dbUser = new User { Email = "test@email.com", PasswordHash = "correct" };

            _userBusinessService.Setup(s => s.GetByEmailAsync(user.Email))
                            .ReturnsAsync(dbUser);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<UserException>(() =>
                _userBusinessService.Object.LoginAsync(user));

            Assert.Contains("InInvalid credentials", ex.Message);
        }

        [Fact]
        public async Task AddAsync_Should_Return_Id_When_User_Is_Valid()
        {
            // Arrange
            var user = new User { Id = Guid.Empty, Email = "test@email.com" };
            var expectedId = Guid.NewGuid();

            _userRepoMock
                .Setup(r => r.AddAsync(user))
                .ReturnsAsync(expectedId);

            // Act
            var result = await _service.AddAsync(user);

            // Assert
            Assert.Equal(expectedId, result);
            _userRepoMock.Verify(r => r.AddAsync(user), Times.Once);
        }

        [Fact]
        public async Task AddAsync_Should_Throw_Exception_When_User_Is_Null()
        {
            // Arrange
            User nullUser = null;

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => _service.AddAsync(nullUser));

            _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task AddAsync_Should_Throw_Exception_When_User_Has_Id()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), Email = "duplicate@email.com" };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.AddAsync(user));

            _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
        }
    }
}
