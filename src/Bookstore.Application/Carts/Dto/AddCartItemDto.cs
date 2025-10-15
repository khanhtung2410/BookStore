using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Carts.Dto
{
    public class AddCartItemDto
    {
        public Guid? BookId { get; set; }
        public int? BookEditionId { get; set; }
        public long Quantity { get; set; }
    }
}
