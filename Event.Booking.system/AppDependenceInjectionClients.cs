using Event.Booking.System.BusinessService;
using Event.Booking.System.BusinessService.Interfaces;
using Event.Booking.System.BusinessService.Interfaces.Utilities;
using Event.Booking.System.BusinessService.Utilities;
using Event.Booking.System.Repository;
using Event.Booking.System.Repository.Interfaces;

namespace Event.Booking.system
{
    public class AppDependenceInjectionClients
    {
        public static void Resgister(WebApplicationBuilder builder)
        {
            //add all dependency services here

            builder.Services.AddScoped<IUserBusinessService, UserBusinessService>();
            builder.Services.AddScoped<IEventBusinessService, EventBusinessService>();
            builder.Services.AddScoped<IBookingBusinessService, BookingBusinessService>();
            builder.Services.AddScoped<ITicketTypeBusinessService, TicketTypeBusinessService>();
            builder.Services.AddScoped<IWaitingListEntryBusinessService, WaitingListEntryBusinessService>();

            builder.Services.AddScoped<IGlobalService, GlobalService>();
            builder.Services.AddScoped<IGlobalDateTimeSettings, GlobalDateTimeSettings>();
           

            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IEventRepository, EventRepository>();
            builder.Services.AddScoped<IBookingRepository, BookingRepository>();
            builder.Services.AddScoped<ITicketTypeRepository, TicketTypeRepository>();
            builder.Services.AddScoped<IWaitingListEntryRepository, WaitingListEntryRepository>();

        }
    }
}
