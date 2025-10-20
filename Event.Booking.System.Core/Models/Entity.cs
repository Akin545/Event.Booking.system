using Event.Booking.System.Core.Interface;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Event.Booking.System.Core.Model
{
    public class Entity : EntityIdentity, IEntity
    {

        [Required]
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public Guid? UpdatedBy { get; set; }

        [Required]
        public Guid CreatedBy { get; set; }




        public override bool Equals(object obj)
        {
            var item = obj as IEntityIdentity;

            if (item == null)
                return false;

            return Id.Equals(item.Id);
        }
    }
}
