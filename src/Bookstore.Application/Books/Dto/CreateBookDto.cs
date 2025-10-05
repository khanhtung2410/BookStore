using Abp.AutoMapper;
using Bookstore.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Books.Dto
{
    public class CreateBookInventoryDto 
    { 
        [Required(ErrorMessage = "Inventory amount is required.")]
        [Range(0, long.MaxValue, ErrorMessage = "Amount must be a non-negative number.")]
        public long Amount { get; set; }

        [Required(ErrorMessage = "Inventory buy price is required.")]
        [Range(0.0, double.MaxValue, ErrorMessage = "Buy price must be a non-negative number.")]
        public decimal BuyPrice { get; set; }

        [Required(ErrorMessage = "Inventory sell price is required.")]
        [Range(0.0, double.MaxValue, ErrorMessage = "Sell price must be a non-negative number.")]
        public decimal SellPrice { get; set; }
    }

    [AutoMapTo(typeof(Book))]
    public class CreateBookDto : IValidatableObject
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(BookConsts.MaxTitleLength)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Author is required.")]
        [StringLength(BookConsts.MaxAuthorLength)]
        public string Author { get; set; }

        [Required(ErrorMessage = "Genre is required.")]
        public BookConsts.Genre Genre { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(BookConsts.MaxDescriptionLength)]
        public string Description { get; set; }

        public DateTime? PublishedDate { get; set; }

        [Required(ErrorMessage = "Inventory is required.")]
        public CreateBookInventoryDto Inventory { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!Enum.IsDefined(typeof(BookConsts.Genre), Genre))
            {
                yield return new ValidationResult("Genre is required and must be a valid value.", new[] { nameof(Genre) });
            }
            if (PublishedDate.HasValue && PublishedDate.Value > DateTime.Now.AddYears(1))
            {
                yield return new ValidationResult("Published date cannot be more than 1 years in the future.", new[] { nameof(PublishedDate) });
            }
            if (Inventory == null)
            {
                yield return new ValidationResult("Inventory is required.", new[] { nameof(Inventory) });
            }
        }
    }
}
