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
        [Required(ErrorMessage = "FormatIsRequired")]
        public BookConsts.Format Format { get; set; }

        [Required(ErrorMessage = "PublisherIsRequired")]
        [StringLength(100, ErrorMessage = "PublisherMaxLengthExceeded")]
        public string Publisher { get; set; }

        [Required(ErrorMessage = "PublishedDateIsRequired")]
        public DateTime? PublishedDate { get; set; }

        [Required(ErrorMessage = "ISBNIsRequired")]
        [StringLength(13, ErrorMessage = "ISBNMaxLengthExceeded")]
        public string ISBN { get; set; }
        public CreateBookInventoryDto? Inventory { get; set; } // Added inventory to edition
    }

    public class CreateBookInventoryDto
    {
        [Required(ErrorMessage = "BuyPriceRequired")]
        [Range(0.0, double.MaxValue, ErrorMessage = "BuyPriceNonNegative")]
        public decimal BuyPrice { get; set; }

        [Required(ErrorMessage = "SellPriceRequired")]
        [Range(0.0, double.MaxValue, ErrorMessage = "SellPriceNonNegative")]
        public decimal SellPrice { get; set; }

        [Required(ErrorMessage = "StockQuantityRequired")]
        [Range(0, long.MaxValue, ErrorMessage = "StockQuantityNonNegative")]
        public long StockQuantity { get; set; }
    }

    [AutoMapTo(typeof(Book))]
    public class CreateBookDto : IValidatableObject
    {
        [Required(ErrorMessage = "TitleIsRequired")]
        [StringLength(BookConsts.MaxTitleLength, ErrorMessage = "TitleMaxLengthExceeded")]
        public string Title { get; set; }

        [Required(ErrorMessage = "AuthorIsRequired")]
        [StringLength(BookConsts.MaxAuthorLength, ErrorMessage = "AuthorMaxLengthExceeded")]
        public string Author { get; set; }

        [Required(ErrorMessage = "GenreIsRequired")]
        public BookConsts.Genre Genre { get; set; }

        [Required(ErrorMessage = "DescriptionIsRequired")]
        [StringLength(BookConsts.MaxDescriptionLength, ErrorMessage = "DescriptionMaxLengthExceeded")]
        public string Description { get; set; }

        [Required]
        public List<CreateBookEditionDto> Editions { get; set; } = new List<CreateBookEditionDto>();
        public List<BookImageDto>? Images { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!Enum.IsDefined(typeof(BookConsts.Genre), Genre))
            {
                yield return new ValidationResult("InvalidGenre", new[] { nameof(Genre) });
            }
            if (Editions == null || Editions.Count == 0)
            {
                yield return new ValidationResult("AtLeastOneEditionRequired", new[] { nameof(Editions) });
            }
            foreach (var edition in Editions)
            {
                if (edition.Inventory != null)
                {
                    var context = new ValidationContext(edition.Inventory);
                    var results = new List<ValidationResult>();
                    Validator.TryValidateObject(edition.Inventory, context, results, true);
                    foreach (var result in results)
                    {
                        yield return result;
                    }
                }
            }
        }
    }
}
