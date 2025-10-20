using Event.Booking.System.Core.Interface;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event.Booking.System.Core.Model
{
    public class EntityIdentity : IEntityIdentity
    {
        public Guid Id { get; set; }

        public override bool Equals(object obj)
        {
            var item = obj as IEntityIdentity;

            if (item == null)
                return false;

            return Id.Equals(item.Id);
        }
    }
}
