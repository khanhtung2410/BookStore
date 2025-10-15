using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Carts.Dto
{
    public class UpdateCartItem
    {
        public int CartItemId { get; set; }
        public int NewQuantity { get; set; }
    }
}
