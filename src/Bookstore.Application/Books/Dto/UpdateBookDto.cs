using Bookstore.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Books.Dto
{
    public class UpdateBookDto : IValidatableObject
    {
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
        }
    }
}
