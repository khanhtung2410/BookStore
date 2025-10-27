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
    public class CreateBookEditionDto
    {
        public BookConsts.Format Format { get; set; }
        public string Publisher { get; set; }
        public DateTime? PublishedDate { get; set; }
        public string ISBN { get; set; }
        public CreateBookInventoryDto? Inventory { get; set; } // Added inventory to edition
    }

    public class CreateBookInventoryDto
    {
        public decimal BuyPrice { get; set; }
        public decimal SellPrice { get; set; }
        public long StockQuantity { get; set; }
    }

    [AutoMapTo(typeof(Book))]
    public class CreateBookDto
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public BookConsts.Genre Genre { get; set; }
        public string Description { get; set; }
        public List<CreateBookEditionDto> Editions { get; set; } = new List<CreateBookEditionDto>();
        public List<BookImageDto>? Images { get; set; }
    }
}
