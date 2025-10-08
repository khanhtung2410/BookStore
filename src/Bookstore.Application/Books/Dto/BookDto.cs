using Bookstore.Entities;
using System;
using System.Collections.Generic;
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
        public DateTime? PublishedDate { get; set; }

        public BookInventoryDto Inventory { get; set; }
    }
    public class BookInventoryDto {
        public int Id { get; set; }           
        public int BookId { get; set; }        
        public int Amount { get; set; }        
        public decimal BuyPrice { get; set; }   
        public decimal SellPrice { get; set; }
    }

}
