using Event.Booking.System.BusinessService;
using Event.Booking.System.BusinessService.Interfaces;
using Event.Booking.System.BusinessService.Interfaces.Utilities;
using Event.Booking.System.Core.Dtos.Auth;
using Event.Booking.System.Core.Enums;
using Event.Booking.System.Core.Exceptions;
using Event.Booking.System.Core.Models;
using Event.Booking.System.Repository.Interfaces;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event.Booking.Xunit.Test
{
    public class EventServiceTest
    {
        private readonly Mock<IEventBusinessService> _eventBusinessServiceMock;
        private readonly Mock<IEventRepository> _eventRepositoryMock;
        private readonly Mock<IGlobalService> _globalServiceMock;
        private readonly Mock<IServiceScopeFactory> _serviceScopeMock;
        
        private readonly Mock<ILogger<System.Core.Models.Event>> _loggerMock;
        private readonly Mock<EventBusinessService> _eventBusinessService;
        private readonly Mock<IGlobalDateTimeSettings> _globalDateTimeSettingsMock;

        private readonly EventBusinessService _service;

        public EventServiceTest()
        {
            _eventRepositoryMock = new Mock<IEventRepository>();
            _eventBusinessServiceMock = new Mock<IEventBusinessService>();
            _globalDateTimeSettingsMock = new Mock<IGlobalDateTimeSettings>();
            _loggerMock = new Mock<ILogger<System.Core.Models.Event>>();
            _globalServiceMock = new Mock<IGlobalService>();

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

            _eventBusinessService = new Mock<EventBusinessService>(_eventRepositoryMock.Object,
                 _globalDateTimeSettingsMock.Object, _loggerMock.Object,
                 _globalServiceMock.Object, _serviceScopeMock.Object)
            {
                CallBase = true
            };

            _service = new EventBusinessService(_eventRepositoryMock.Object,
                 _globalDateTimeSettingsMock.Object, _loggerMock.Object,
                 _globalServiceMock.Object, _serviceScopeMock.Object);

        }

        [Fact]
        public async Task AddAsync_Should_Add_Event_When_Admin_And_Valid_Dates()
        {
            // Arrange
            var newEvent = new System.Core.Models.Event
            {
                Id = Guid.Empty,
                Name = "Music Festival",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(2)
            };

            _globalServiceMock.Setup(g => g.Roles).Returns("Admin");

            var expectedId = Guid.NewGuid();
            _eventRepositoryMock.Setup(r => r.AddAsync(newEvent)).ReturnsAsync(expectedId);

            // Act
            var result = await _service.AddAsync(newEvent);

            // Assert
            Assert.Equal(expectedId, result);
            Assert.Equal(EventStatusEnum.NotStarted, newEvent.Status);
            _eventRepositoryMock.Verify(r => r.AddAsync(newEvent), Times.Once);
        }

        [Fact]
        public async Task AddAsync_Should_Throw_When_EndDate_Is_Before_StartDate()
        {
            // Arrange
            var newEvent = new System.Core.Models.Event
            {
                Id = Guid.Empty,
                Name = "Tech Summit",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(-1)
            };

            _globalServiceMock.Setup(g => g.Roles).Returns("Admin");

            // Act & Assert
            var ex = await Assert.ThrowsAsync<EventException>(() => _service.AddAsync(newEvent));
            Assert.Contains("Invalid date setup", ex.Message);
            _eventRepositoryMock.Verify(r => r.AddAsync(It.IsAny<System.Core.Models.Event>()), Times.Never);
        }

        [Fact]
        public async Task AddAsync_Should_Throw_When_User_Is_Not_Admin()
        {
            // Arrange
            var newEvent = new System.Core.Models.Event
            {
                Id = Guid.Empty,
                Name = "Community Fair",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(1)
            };

            _globalServiceMock.Setup(g => g.Roles).Returns("User");

            // Act & Assert
            var ex = await Assert.ThrowsAsync<EventException>(() => _service.AddAsync(newEvent));
            Assert.Contains("create permission", ex.Message);
            _eventRepositoryMock.Verify(r => r.AddAsync(It.IsAny<System.Core.Models.Event>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_Should_Call_Repository_When_Admin_And_Valid_Dates()
        {
            // Arrange
            var existingEvent = new System.Core.Models.Event
            {
                Id = Guid.NewGuid(),
                Name = "Tech Conference",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(1)
            };

            _globalServiceMock.Setup(g => g.Roles).Returns("Admin");

            // Act
            await _service.UpdateAsync(existingEvent);

            // Assert
            _eventRepositoryMock.Verify(r => r.UpdateAsync(existingEvent), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_Should_Throw_When_EndDate_Before_StartDate()
        {
            // Arrange
            var invalidEvent = new System.Core.Models.Event
            {
                Id = Guid.NewGuid(),
                Name = "Music Festival",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(-1)
            };

            _globalServiceMock.Setup(g => g.Roles).Returns("Admin");

            // Act & Assert
            var ex = await Assert.ThrowsAsync<EventException>(() => _service.UpdateAsync(invalidEvent));
            Assert.Contains("Invalid date setup", ex.Message);
            _eventRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<System.Core.Models.Event>()), Times.Never);
        }
    }
}
