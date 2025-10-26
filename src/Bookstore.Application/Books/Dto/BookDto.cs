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
        [Required(ErrorMessage = "TitleIsRequired")]
        [StringLength(BookConsts.MaxTitleLength, ErrorMessage = "TitleMaxLengthExceeded")]
        public string Title { get; set; }

        [Required(ErrorMessage = "AuthorIsRequired")]
        [StringLength(BookConsts.MaxAuthorLength, ErrorMessage = "AuthorMaxLengthExceeded")]
        public string Author { get; set; }

        [EnumDataType(typeof(BookConsts.Genre),ErrorMessage = "InvalidGenre")]
        public BookConsts.Genre Genre { get; set; }

        [Required(ErrorMessage = "DescriptionIsRequired")]
        [StringLength(BookConsts.MaxDescriptionLength, ErrorMessage = "DescriptionMaxLengthExceeded")]
        public string Description { get; set; }
        public List<BookEditionDto> Editions { get; set; } = new();
        public List<BookImageDto>? Images { get; set; } = new();

    }
    public class BookEditionDto {
        [Required]
        public int Id { get; set; }
        [Required]
        public int BookId { get; set; }
        [EnumDataType(typeof(BookConsts.Format), ErrorMessage = "InvalidFormat")]
        public BookConsts.Format Format { get; set; }
        [Required(ErrorMessage = "PublisherIsRequired")]
        [StringLength(100, ErrorMessage = "PublisherMaxLengthExceeded")]
        public string Publisher { get; set; }
        [Required(ErrorMessage = "PublishedDateIsRequired")]
        public DateTime PublishedDate { get; set; }

        [Required(ErrorMessage = "ISBNIsRequired")]
        [StringLength(13, ErrorMessage = "ISBNMaxLengthExceeded")]
        public string ISBN { get; set; }
        public BookInventoryDto Inventory { get; set; }
        public DiscountDto? Discount { get; set; }
    }
 
}
