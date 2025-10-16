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
        [Required]
        public int Id { get; set; }
        [Required]
        [StringLength(BookConsts.MaxTitleLength, ErrorMessage = "Title can't be too long")]
        public string Title { get; set; }
        [Required]
        [StringLength(BookConsts.MaxAuthorLength)]
        public string Author { get; set; }
        [EnumDataType(typeof(BookConsts.Genre),ErrorMessage ="Please select valid genre")]
        public BookConsts.Genre Genre { get; set; }
        [StringLength(BookConsts.MaxDescriptionLength, ErrorMessage = "Description can't be too long")]
        public string Description { get; set; }
        public List<BookEditionDto> Editions { get; set; } = new();
    }
    public class BookEditionDto {
        [Required]
        public int Id { get; set; }
        [Required]
        public int BookId { get; set; }
        [EnumDataType(typeof(BookConsts.Format), ErrorMessage = "Please select valid format")]
        public BookConsts.Format Format { get; set; }
        [Required]
        public string Publisher { get; set; }
        [Required]
        public DateTime PublishedDate { get; set; }
        [Required]
        [StringLength(13, ErrorMessage = "ISBN can't be longer than 13 characters")]
        public string ISBN { get; set; }
        public BookInventoryDto Inventory { get; set; }
        public DiscountDto? Discount { get; set; }
    }
 
}
