using Event.Booking.System.Core.Interface;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event.Booking.System.Core.Dtos
{
    public class DtoIdentity : IDtoIdentity
    {
        public Guid Id { get; set; }
    }
}