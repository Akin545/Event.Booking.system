using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event.Booking.System.Core.Interface
{
    public interface IEntityIdentity
    {
        [Key]
        Guid Id { get; set; }
    }
}