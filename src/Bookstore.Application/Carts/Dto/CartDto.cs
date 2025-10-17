using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Carts.Dto
{
    public class CartDto
    {
        public Guid Id { get; set; }
        public long UserId { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
    }
    public class CartItemDto
    {
        public int Id { get; set; }
        public Guid CartId { get; set; }
        public int? BookEditionId { get; set; }
        public int? BookId { get; set; }

        public int Quantity { get; set; }
        public string? BookTitle { get; set; }
        public string? BookAuthor { get; set; }
        public decimal SellPrice { get; set; }
        public decimal TotalPrice => SellPrice * Quantity;

        public Bookstore.Books.Dto.BookDto? Book { get; set; }
        public Bookstore.Books.Dto.BookEditionDto? BookEdition { get; set; }
    }
}
