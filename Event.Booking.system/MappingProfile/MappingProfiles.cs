using AutoMapper;

using Event.Booking.System.Core.Dtos.Auth;
using Event.Booking.System.Core.Dtos.Bookings;
using Event.Booking.System.Core.Dtos.Event;
using Event.Booking.System.Core.Dtos.TicketTypes;
using Event.Booking.System.Core.Dtos.User;
using Event.Booking.System.Core.Dtos.WaitingListEntry;
using Event.Booking.System.Core.Models;

namespace Event.Booking.system.MappingProfile
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            #region User
            CreateMap<UserDto, User>(MemberList.None).ReverseMap();
            CreateMap<CreateUserDto, User>(MemberList.None)
                .ForMember(r => r.PasswordHash, o => o.MapFrom(s => s.Password)).ReverseMap();
           
            CreateMap<LoginDto, User>(MemberList.None)
                .ForMember(r => r.PasswordHash, o => o.MapFrom(s => s.Password)).ReverseMap();

            #endregion

            #region TicketType
            CreateMap<TicketTypeDto, TicketType>(MemberList.None).ReverseMap();
            CreateMap<CreateTicketTypeDto, TicketType>(MemberList.None).ReverseMap();

            CreateMap<UpdateTicketTypeDto, TicketTypeDto>(MemberList.None).ReverseMap();

            #endregion

            #region Event
            CreateMap<EventDto, System.Core.Models.Event>(MemberList.None)
                 .ForMember(r => r.TicketTypes, o => o.MapFrom(s => s.TicketTypes)).ReverseMap();
            CreateMap<CreateEventDto, System.Core.Models.Event>(MemberList.None).ReverseMap();

            CreateMap<UpdateEventDto, EventDto>(MemberList.None).ReverseMap();

            #endregion

            #region WaitingList 

            CreateMap<WaitingListEntry, WaitingListDto >(MemberList.None)
                .ForMember(r => r.UserFullName, o => o.MapFrom(s => s.User.FullName))
                .ForMember(r => r.EventName, o => o.MapFrom(s => s.Event.Name)).ReverseMap();

            #endregion

            #region Booking
            CreateMap<System.Core.Models.Booking, BookingDto>(MemberList.None)
                .ForMember(r => r.EventName, o => o.MapFrom(s => s.Event.Name))
                .ForMember(r => r.TicketType, o => o.MapFrom(s => s.TicketType.Name))
                .ForMember(r => r.UserFullName, o => o.MapFrom(s => s.User.FullName)).ReverseMap();
          
            CreateMap<CreateBookingDto, System.Core.Models.Booking>(MemberList.None).ReverseMap();
            CreateMap<BookingCancelTicketDto, BookingDto>(MemberList.None).ReverseMap();

            #endregion
        }
    }
}