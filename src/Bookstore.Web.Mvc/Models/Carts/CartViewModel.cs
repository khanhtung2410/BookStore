using Bookstore.Books.Dto;
using Bookstore.Entities.Books;
using System.Collections.Generic;

namespace Bookstore.Web.Models.Carts
{
    public class CartViewModel
    {
        public long UserId { get; set; }
        public IReadOnlyList<CartItemViewModel> Items { get; set; }
    }

    public class CartItemViewModel
    {
        public int CartItemId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public BookConsts.Format Format { get; set; }
        public int Quantity { get; set; }
        public decimal SellPrice { get; set; }
        public int StockQuantity { get; set; } 
        public int BookEditionId { get; set; }
    }

}
