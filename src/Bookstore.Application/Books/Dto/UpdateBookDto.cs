using Bookstore.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Books.Dto
{
    public class UpdateBookInventoryDto
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
    public class UpdateBookDto : IValidatableObject
    {
        public int Id { get; set; }

        [Required]
        [StringLength(BookConsts.MaxTitleLength)]
        public string Title { get; set; }

        [Required]
        [StringLength(BookConsts.MaxAuthorLength)]
        public string Author { get; set; }

        [Required]
        [StringLength(BookConsts.MaxDescriptionLength)]
        public string Description { get; set; }

        public DateTime? PublishedDate { get; set; }

        [Required]
        public BookConsts.Genre Genre { get; set; }

        [Required(ErrorMessage = "Iventory required.")]
        public UpdateBookInventoryDto Inventory { get; set; }

        // Validate
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PublishedDate.HasValue && PublishedDate.Value > DateTime.Now.AddYears(1))
            {
                yield return new ValidationResult(
                    "Published date cannot be more than 1 years in the future.",
                    new[] { nameof(PublishedDate) }
                );
            }
            if (!Enum.IsDefined(typeof(BookConsts.Genre), Genre))
            {
                yield return new ValidationResult(
                    "Please select a valid genre.",
                    new[] { nameof(Genre) }
                );
            }
            if (Inventory == null)
            {
                yield return new ValidationResult("Inventory is required.", new[] { nameof(Inventory) });
            }
        }
    }
}
