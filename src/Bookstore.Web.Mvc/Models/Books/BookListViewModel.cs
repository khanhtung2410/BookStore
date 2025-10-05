using Bookstore.Books.Dto;
using System.Collections.Generic;

namespace Bookstore.Web.Models.Books
{
    public class BookListViewModel
    {
        public IReadOnlyList<ListBookDto> Books { get; set; }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
