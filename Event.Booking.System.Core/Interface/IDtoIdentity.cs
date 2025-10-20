using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event.Booking.System.Core.Interface
{
    public interface IDtoIdentity : IDtoNoID
    {
        
        Guid Id { get; set; }
    }
}
