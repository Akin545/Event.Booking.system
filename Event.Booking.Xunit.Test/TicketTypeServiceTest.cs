using Event.Booking.System.BusinessService;
using Event.Booking.System.BusinessService.Interfaces;
using Event.Booking.System.BusinessService.Interfaces.Utilities;
using Event.Booking.System.Core.Exceptions;
using Event.Booking.System.Core.Models;
using Event.Booking.System.Repository.Interfaces;

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
    public class TicketTypeServiceTest
    {
        private readonly Mock<ITicketTypeBusinessService> _ticketTypeBusinessServiceMock;
        private readonly Mock<ITicketTypeRepository> _ticketTypeRepositoryMock;
        private readonly Mock<IGlobalService> _globalServiceMock;
        private readonly Mock<IServiceScopeFactory> _serviceScopeMock;

        private readonly Mock<ILogger<TicketType>> _loggerMock;
        private readonly Mock<TicketTypeBusinessService> _ticketTypeBusinessService;
        private readonly Mock<IGlobalDateTimeSettings> _globalDateTimeSettingsMock;

        private readonly TicketTypeBusinessService _service;


        public TicketTypeServiceTest()
        {
            _ticketTypeRepositoryMock = new Mock<ITicketTypeRepository>();
            _ticketTypeBusinessServiceMock = new Mock<ITicketTypeBusinessService>();
            _globalDateTimeSettingsMock = new Mock<IGlobalDateTimeSettings>();
            _loggerMock = new Mock<ILogger<TicketType>>();
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

            _ticketTypeBusinessService = new Mock<TicketTypeBusinessService>(_ticketTypeRepositoryMock.Object,
                 _globalDateTimeSettingsMock.Object, _loggerMock.Object,
                 _globalServiceMock.Object, _serviceScopeMock.Object)
            {
                CallBase = true
            };

            _service = new TicketTypeBusinessService(_ticketTypeRepositoryMock.Object,
                 _globalDateTimeSettingsMock.Object, _loggerMock.Object,
                 _globalServiceMock.Object, _serviceScopeMock.Object);

        }

        [Fact]
        public async Task AddAsync_Should_Call_Repository_When_User_Is_Admin()
        {
            // Arrange
            var ticketType = new TicketType
            {
                //Id = Guid.NewGuid(),
                Name = "VIP Ticket"
            };

            _globalServiceMock.Setup(g => g.Roles).Returns("Admin");
            _ticketTypeRepositoryMock.Setup(r => r.AddAsync(ticketType))
                           .ReturnsAsync(ticketType.Id);

            // Act
            var result = await _service.AddAsync(ticketType);

            // Assert
            Assert.Equal(ticketType.Id, result);
            _ticketTypeRepositoryMock.Verify(r => r.AddAsync(ticketType), Times.Once);
        }

        [Fact]
        public async Task AddAsync_Should_Throw_When_User_Is_Not_Admin()
        {
            // Arrange
            var ticketType = new TicketType
            {
                //Id = Guid.NewGuid(),
                Name = "Regular Ticket"
            };

            _globalServiceMock.Setup(g => g.Roles).Returns("User");

            // Act & Assert
            var ex = await Assert.ThrowsAsync<TicketTypeException>(() => _service.AddAsync(ticketType));
            Assert.Contains("create permission", ex.Message);
            _ticketTypeRepositoryMock.Verify(r => r.AddAsync(It.IsAny<TicketType>()), Times.Never);
        }

        [Fact]
        public async Task AddAsync_Should_Throw_When_Item_Is_Null()
        {
            // Arrange
            _globalServiceMock.Setup(g => g.Roles).Returns("Admin");

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => _service.AddAsync(null));
            _ticketTypeRepositoryMock.Verify(r => r.AddAsync(It.IsAny<TicketType>()), Times.Never);
        }
    }
}
