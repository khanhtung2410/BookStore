using Abp.AutoMapper;
using Bookstore.Entities.Books;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Books.Dto
{
    public class UpdateBookEditionDto 
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public BookConsts.Format Format { get; set; }
        public string Publisher { get; set; }
        public DateTime? PublishedDate { get; set; }
        public string ISBN { get; set; }
    }

    [AutoMapTo(typeof(Book))]
    public class UpdateBookDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public BookConsts.Genre Genre { get; set; }
        public string Description { get; set; }
        public List<UpdateBookEditionDto> Editions { get; set; } = new List<UpdateBookEditionDto>();
    }
}
