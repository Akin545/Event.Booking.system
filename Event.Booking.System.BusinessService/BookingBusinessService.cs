using Event.Booking.System.BusinessService.Interfaces;
using Event.Booking.System.BusinessService.Interfaces.Utilities;
using Event.Booking.System.Core.Enums;
using Event.Booking.System.Core.Exceptions;
using Event.Booking.System.Core.Models;
using Event.Booking.System.Repository.Interfaces;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Event.Booking.System.BusinessService
{
    public class BookingBusinessService : BusinessServiceBase<Core.Models.Booking, IBookingRepository>
       , IBookingBusinessService
    {
        private readonly ITicketTypeBusinessService _ticketTypeBusinessService;
        
        public BookingBusinessService(IBookingRepository repository
            , IGlobalDateTimeSettings globalDateTimeBusinessServices
            , ILogger<Core.Models.Booking> logger
            , IGlobalService globalService
            , IServiceScopeFactory scopeFactory
            , ITicketTypeBusinessService ticketTypeBusinessService)
            : base(repository,
                globalDateTimeBusinessServices,
                logger,
                globalService,
                scopeFactory)
        {
            _ticketTypeBusinessService = ticketTypeBusinessService;
           
        }

        public override async Task<Core.Models.Booking> GetAsync(Guid id)
        {
            ValidateId(id);
            var result = await RepositoryManager.GetAsync(id);

            if(GlobalService.Roles!="Admin" && id != result.Id)
            {
                var errorMessage = $"You don't have the right to view this bookings";
                HealthLogger.LogError($"{errorMessage}");
                throw new BookingException(errorMessage);
            }          

            return result;
        }

        public override async Task<Guid> AddAsync(Core.Models.Booking item)
        {
            CheckIfNull(item);
            CheckIfAddedEntityHasId(item.Id);

            TicketType updateTicketQty = new TicketType();
            WaitingListEntry waitingListEntry = new WaitingListEntry();
            item.UserId = Guid.Parse(GlobalService.Id);

            var confirmBooking=await GetBookingUserAsync(item.EventId, item.UserId);
            if (confirmBooking!=null)
            {
                var errorMessage = $"You have already booked for this event and your current status is {confirmBooking.Status}";
                HealthLogger.LogError($"{errorMessage}");
                throw new BookingException(errorMessage);
            }

            var getEvent=await _ticketTypeBusinessService.GetEventByIdAsync(item.EventId);
            if (getEvent == null)
            {
                var errorMessage = $"Event not found";
                HealthLogger.LogError($"{errorMessage}");
                throw new BookingException(errorMessage);
            }

            if (getEvent.Event.EndDate < GlobalDateTimeSettings.CurrentDateTime)
            {
                var errorMessage = $"Event has ended";
                HealthLogger.LogError($"{errorMessage}");
                throw new BookingException(errorMessage);
            }

            if (getEvent.Id != item.TicketTypeId)
            {
                var errorMessage = $"There is a mismach with event ticket id and this ticketId. {item.TicketTypeId}";
                HealthLogger.LogError($"{errorMessage}");
                throw new BookingException(errorMessage);
            }
                        
            if (item.Quantity > getEvent.QuantityAvailable)
            {
                //add to waiting list
                waitingListEntry = new WaitingListEntry
                {
                    UserId = item.UserId,
                    EventId = item.EventId,

                    Notified = false,
                    RequestedAt=GlobalDateTimeSettings.CurrentDateTime,
                };

                //set waiting list users as pending
                item.Status = BookingStatus.Pending;
                updateTicketQty = null;
            }
            else
            {
                waitingListEntry = null;
                item.Status = BookingStatus.Confirmed;
                var newQty = getEvent.QuantityAvailable - item.Quantity;
                updateTicketQty = new TicketType
                {
                    Id = getEvent.Id,
                    QuantityAvailable = newQty,
                    EventId = item.EventId,
                };
            }

            item.BookingDate = GlobalDateTimeSettings.CurrentDateTime;
                       
            return await RepositoryManager.AddAsync(item, updateTicketQty, waitingListEntry);
        }


        public async Task<Guid> UpdateAsync(Core.Models.Booking item)
        {
            
            CheckIfNull(item);
            //CheckIfAddedEntityHasId(item.Id);

            TicketType ticket = new TicketType();
            WaitingListEntry updateWaitingList = new WaitingListEntry();
            Core.Models.Booking updateBookingWaiter = new Core.Models.Booking();

            if(item.UserId!= Guid.Parse(GlobalService.Id))
            {
                var errorMessage = $"You don't have the right to cancel this bookings";
                HealthLogger.LogError($"{errorMessage}");
                throw new BookingException(errorMessage);
            }

            var getEvent = await _ticketTypeBusinessService.GetEventByIdAsync(item.EventId);
            if (getEvent == null)
            {
                var errorMessage = $"Event not found";
                HealthLogger.LogError($"{errorMessage}");
                throw new BookingException(errorMessage);
            }

            if (getEvent.Event.StartDate < GlobalDateTimeSettings.CurrentDateTime)
            {
                var errorMessage = $"Event already started for this id {getEvent.Event.Id}";
                HealthLogger.LogError($"{errorMessage}");
                throw new BookingException(errorMessage);
            }

            if (getEvent.Id != item.TicketTypeId)
            {
                var errorMessage = $"There is a mismach with event ticket id and this ticketId. {item.TicketTypeId}";
                HealthLogger.LogError($"{errorMessage}");
                throw new BookingException(errorMessage);
            }

            if (BookingStatus.Cancelled == item.Status)
            {
                var errorMessage = $"This booking is already marked cancelled. {item.Id}";
                HealthLogger.LogError($"{errorMessage}");
                throw new BookingException(errorMessage);
            }
            else if(BookingStatus.Pending == item.Status)
            {
                updateBookingWaiter = new Core.Models.Booking()
                {
                    Id = item.Id,
                    Status = BookingStatus.Cancelled,
                };

                updateWaitingList = new WaitingListEntry
                {
                    BookingId = item.Id,
                    Notified = false,
                };

                ticket = null;
            }
            else
            {
                var getPendingBooking = await GetLatestWaitingUserAsync(item.Quantity, item.EventId, getEvent.Id);
                if (getPendingBooking != null)
                {
                    updateBookingWaiter = new Core.Models.Booking()
                    {
                        Id = getPendingBooking.Id,
                        Status = BookingStatus.Confirmed,
                    };

                    updateWaitingList = new WaitingListEntry
                    {
                        BookingId = getPendingBooking.Id,
                        Notified = true,
                    };

                    var getCurrrentTicketQty = getEvent.QuantityAvailable;
                    var getQtyLeft = item.Quantity - getPendingBooking.Quantity;
                    var totalToRestore = getCurrrentTicketQty + getQtyLeft;
                    ticket = new TicketType
                    {
                        Id = item.TicketTypeId,
                        EventId = item.EventId,
                        QuantityAvailable = totalToRestore,
                    };

                    item.Status = BookingStatus.Cancelled;
                }
                else
                {
                    //return quantity back to ticket
                    var getCurrrentTicketQty = getEvent.QuantityAvailable;
                    var totalToRestor = getCurrrentTicketQty + item.Quantity;
                    ticket = new TicketType
                    {
                        Id = item.TicketTypeId,
                        EventId = item.EventId,
                        QuantityAvailable = totalToRestor,
                    };

                    updateWaitingList = null;
                    updateBookingWaiter = null;
                    item.Status = BookingStatus.Cancelled;
                }
            }          

            return await RepositoryManager.UpdateAllTablesRelatedAsync(item, ticket, updateWaitingList, updateBookingWaiter);
        }

        public async Task<Core.Models.Booking> GetLatestWaitingUserAsync(int qty,Guid eventId, Guid ticketId)
        {
            var result = await RepositoryManager.GetLatestWaitingUserAsync(qty, eventId,ticketId);

            return result;
        }

        public virtual async Task<Core.Models.Booking> GetBookingUserAsync(Guid eventId, Guid userId)
        {
            var result = await RepositoryManager.GetBookingUserAsync(eventId, userId);

            return result;
        }
    }
}
