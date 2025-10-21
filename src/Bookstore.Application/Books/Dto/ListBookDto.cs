using Abp.Application.Services.Dto;
using Bookstore.Entities.Books;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Books.Dto
{
    public class ListBookDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Genre { get; set; }
    }
    public class GetAllBooksInput : PagedAndSortedResultRequestDto
    {
        public BookConsts.Genre? Genre { get; set; }
        public string Keyword { get; set; }
    }
}
