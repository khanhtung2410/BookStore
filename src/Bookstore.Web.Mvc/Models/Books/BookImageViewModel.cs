using Bookstore.Books.Dto;
using System.Collections.Generic;

namespace Bookstore.Web.Models.Books
{
    public class BookImageViewModel
    {
        public int BookId { get; set; }
        public string Title { get; set; }
        public List<BookImageDto> Images { get; set; } = new List<BookImageDto>();
    }
}
