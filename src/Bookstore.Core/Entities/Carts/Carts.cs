using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Entities.Carts
{
    [Table("Carts")]
    public class Cart : AuditedEntity<Guid>
    {
        public long? UserId { get; set; }
        public virtual List<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
