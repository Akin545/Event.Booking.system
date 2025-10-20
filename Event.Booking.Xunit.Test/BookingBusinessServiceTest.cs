using Event.Booking.System.BusinessService;
using Event.Booking.System.BusinessService.Interfaces;
using Event.Booking.System.BusinessService.Interfaces.Utilities;
using Event.Booking.System.Core.Enums;
using Event.Booking.System.Core.Exceptions;
using Event.Booking.System.Core.Models;
using Event.Booking.System.Repository.Interfaces;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;
using Moq.Protected;

namespace Event.Booking.Xunit.Test
{
    public class BookingBusinessServiceTest
    {
        private readonly Mock<IBookingBusinessService> _bookingBusinessServiceMock;
        private readonly Mock<IBookingRepository> _bookingRepositoryMock;
        private readonly Mock<IGlobalService> _globalServiceMock;
        private readonly Mock<IServiceScopeFactory> _serviceScopeMock;
        private readonly Mock<ITicketTypeBusinessService> _ticketTypeBusinessServiceMock;

        private readonly Mock<ILogger<System.Core.Models.Booking>> _loggerMock;
        private readonly Mock<BookingBusinessService> _bookingBusinessService;
        private readonly Mock<IGlobalDateTimeSettings> _globalDateTimeSettingsMock;

        private readonly BookingBusinessService _service;

        public BookingBusinessServiceTest()
        {
            _ticketTypeBusinessServiceMock = new Mock<ITicketTypeBusinessService>();
            _bookingRepositoryMock = new Mock<IBookingRepository>();
            _bookingBusinessServiceMock = new Mock<IBookingBusinessService>();
            _globalDateTimeSettingsMock = new Mock<IGlobalDateTimeSettings>();
            _loggerMock = new Mock<ILogger<System.Core.Models.Booking>>();
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

            _bookingBusinessService = new Mock<BookingBusinessService>(_bookingRepositoryMock.Object,
                 _globalDateTimeSettingsMock.Object, _loggerMock.Object,
                 _globalServiceMock.Object, _serviceScopeMock.Object, _ticketTypeBusinessServiceMock.Object)
            {
                CallBase = true
            };

            _service = new BookingBusinessService(_bookingRepositoryMock.Object,
                 _globalDateTimeSettingsMock.Object, _loggerMock.Object,
                 _globalServiceMock.Object, _serviceScopeMock.Object, _ticketTypeBusinessServiceMock.Object);

        }

        [Fact]
        public async Task AddAsync_Should_Add_Booking_When_Valid()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var ticketTypeId = Guid.NewGuid();

            var booking = new System.Core.Models.Booking
            {
                Id = Guid.Empty,
                EventId = eventId,
                TicketTypeId = ticketTypeId,
                Quantity = 2
            };


            _bookingBusinessService
            .Setup(s => s.GetBookingUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync((System.Core.Models.Booking)null);

            _ticketTypeBusinessServiceMock.Setup(s => s.GetEventByIdAsync(eventId))
                .ReturnsAsync(new TicketType
                {
                    Id = ticketTypeId,
                    EventId = eventId,
                    QuantityAvailable = 10,
                    Event = new System.Core.Models.Event
                    {
                        EndDate = DateTime.UtcNow.AddDays(2)
                    }
                });

            _bookingRepositoryMock.Setup(r => r.AddAsync(It.IsAny<System.Core.Models.Booking>(), It.IsAny<TicketType>(), It.IsAny<WaitingListEntry>()))
                           .ReturnsAsync(Guid.NewGuid());

            _globalServiceMock.Setup(g => g.Roles).Returns("Admin");
            _globalServiceMock.Setup(g => g.Id).Returns(Guid.NewGuid().ToString());
            _globalDateTimeSettingsMock.Setup(g => g.CurrentDateTime).Returns(DateTime.UtcNow);

            // Act
            var result = await _bookingBusinessService.Object.AddAsync(booking);

            // Assert
            Assert.NotEqual(Guid.Empty, result);
            _bookingRepositoryMock.Verify(r => r.AddAsync(It.IsAny<System.Core.Models.Booking>(), It.IsAny<TicketType>(), It.IsAny<WaitingListEntry>()), Times.Once);
        }

        [Fact]
        public async Task AddAsync_Should_Throw_When_User_Already_Booked()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var existingBooking = new System.Core.Models.Booking
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                UserId = userId,
                Status = BookingStatus.Confirmed
            };

            var newBooking = new System.Core.Models.Booking
            {
                Id = Guid.Empty,
                EventId = eventId,
                UserId = userId,
                TicketTypeId = Guid.NewGuid()
            };

           
            _bookingBusinessService
           .Setup(s => s.GetBookingUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
           .ReturnsAsync(existingBooking);

            _globalServiceMock.Setup(g => g.Roles).Returns("Admin");
            _globalServiceMock.Setup(g => g.Id).Returns(Guid.NewGuid().ToString());
            _globalDateTimeSettingsMock.Setup(g => g.CurrentDateTime).Returns(DateTime.UtcNow);

            // Act + Assert
            await Assert.ThrowsAsync<BookingException>(() => _bookingBusinessService.Object.AddAsync(newBooking));

            _bookingRepositoryMock.Verify(r => r.AddAsync(It.IsAny<System.Core.Models.Booking>(), It.IsAny<TicketType>(), It.IsAny<WaitingListEntry>()), Times.Never);
        }

    }
}
