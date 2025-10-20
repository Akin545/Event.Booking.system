using Event.Booking.System.BusinessService;
using Event.Booking.System.BusinessService.Interfaces;
using Event.Booking.System.BusinessService.Interfaces.Utilities;
using Event.Booking.System.Core.Exceptions;
using Event.Booking.System.Core.Models;
using Event.Booking.System.Repository.Interfaces;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;

namespace Event.Booking.Xunit.Test
{
    public class WaitingListEntryBusinessServiceTest
    {
        private readonly Mock<IWaitingListEntryBusinessService> _waitingListEntryBusinessServiceMock;
        private readonly Mock<IWaitingListEntryRepository> _waitingListEntryRepositoryMock;
        private readonly Mock<IGlobalService> _globalServiceMock;
        private readonly Mock<IServiceScopeFactory> _serviceScopeMock;

        private readonly Mock<ILogger<WaitingListEntry>> _loggerMock;
        private readonly Mock<WaitingListEntryBusinessService> _waitingListEntryBusinessService;
        private readonly Mock<IGlobalDateTimeSettings> _globalDateTimeSettingsMock;

        private readonly WaitingListEntryBusinessService _service;


        public WaitingListEntryBusinessServiceTest()
        {
            _waitingListEntryRepositoryMock = new Mock<IWaitingListEntryRepository>();
            _waitingListEntryBusinessServiceMock = new Mock<IWaitingListEntryBusinessService>();
            _globalDateTimeSettingsMock = new Mock<IGlobalDateTimeSettings>();
            _loggerMock = new Mock<ILogger<WaitingListEntry>>();
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

            _waitingListEntryBusinessService = new Mock<WaitingListEntryBusinessService>(_waitingListEntryRepositoryMock.Object,
                 _globalDateTimeSettingsMock.Object, _loggerMock.Object,
                 _globalServiceMock.Object, _serviceScopeMock.Object)
            {
                CallBase = true
            };

            _service = new WaitingListEntryBusinessService(_waitingListEntryRepositoryMock.Object,
                 _globalDateTimeSettingsMock.Object, _loggerMock.Object,
                 _globalServiceMock.Object, _serviceScopeMock.Object);

        }

        [Fact]
        public async Task ListAsync_Should_Return_WaitingListEntries()
        {
            // Arrange
            int pageNumber = 1;
            Guid eventId = Guid.NewGuid();

            var expectedEntries = new List<WaitingListEntry>
        {
            new WaitingListEntry { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), EventId = eventId },
            new WaitingListEntry { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), EventId = eventId }
        };

            _waitingListEntryRepositoryMock
                .Setup(r => r.ListAsync(pageNumber, eventId))
                .ReturnsAsync(expectedEntries);

            // Act
            var result = await _service.ListAsync(pageNumber, eventId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(expectedEntries, result);
            _waitingListEntryRepositoryMock.Verify(r => r.ListAsync(pageNumber, eventId), Times.Once);
        }

        [Fact]
        public async Task ListAsync_Should_Return_EmptyList_When_Repository_Returns_Empty()
        {
            // Arrange
            int pageNumber = 1;
            Guid eventId = Guid.NewGuid();

            _waitingListEntryRepositoryMock
                .Setup(r => r.ListAsync(pageNumber, eventId))
                .ReturnsAsync(new List<WaitingListEntry>());

            // Act
            var result = await _service.ListAsync(pageNumber, eventId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _waitingListEntryRepositoryMock.Verify(r => r.ListAsync(pageNumber, eventId), Times.Once);
        }

        [Fact]
        public async Task CountWaitingListAsync_Should_Return_Count_When_EventId_Is_Valid()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var expectedCount = 5;

            _waitingListEntryRepositoryMock.Setup(r => r.CountWaitingListAsync(eventId))
                           .ReturnsAsync(expectedCount);

            // Act
            var result = await _service.CountWaitingListAsync(eventId);

            // Assert
            Assert.Equal(expectedCount, result);
            _waitingListEntryRepositoryMock.Verify(r => r.CountWaitingListAsync(eventId), Times.Once);
        }

        [Fact]
        public async Task CountWaitingListAsync_Should_Throw_When_EventId_Is_Empty()
        {
            // Arrange
            var invalidEventId = Guid.Empty;

            // Act & Assert
            var ex = await Assert.ThrowsAsync<WaitingListEntryException>(
                () => _service.CountWaitingListAsync(invalidEventId)
            );

            Assert.Contains("Event Id not valid", ex.Message);
            _waitingListEntryRepositoryMock.Verify(r => r.CountWaitingListAsync(It.IsAny<Guid>()), Times.Never);
        }

    }
}
