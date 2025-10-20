using Event.Booking.System.Core.Interface;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event.Booking.System.Core.Dtos.WaitingListEntry
{
    public class WaitingListResponseDTO<T> where T : IDto
    {
        public List<T> Results { get; set; } = null!;
        public int? MaxLength { get; set; }
    }
}
