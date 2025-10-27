using Bookstore.Entities.Books;
using Microsoft.AspNetCore.Antiforgery;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Books.Dto
{
    public class BookDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public BookConsts.Genre Genre { get; set; }
        public string Description { get; set; }
        public List<BookEditionDto> Editions { get; set; } = new();
        public List<BookImageDto>? Images { get; set; } = new();

    }
    public class BookEditionDto {
        [Required]
        public int Id { get; set; }
        [Required]
        public int BookId { get; set; }
        public BookConsts.Format Format { get; set; }
        public string Publisher { get; set; }
        public DateTime PublishedDate { get; set; }
        public string ISBN { get; set; }
        public BookInventoryDto Inventory { get; set; }
        public DiscountDto? Discount { get; set; }
    }
 
}
