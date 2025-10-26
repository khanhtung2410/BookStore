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
    public class UpdateBookEditionDto : IValidatableObject
    {
        public int Id { get; set; }
        [Required]
        public int BookId { get; set; }
        [Required]
        public BookConsts.Format Format { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "PublisherMaxLengthExceeded")]
        public string Publisher { get; set; }
        [Required(ErrorMessage = "PublishedDateIsRequired")]
        public DateTime? PublishedDate { get; set; }

        [Required(ErrorMessage = "ISBNIsRequired")]
        [StringLength(13, ErrorMessage = "ISBNMaxLengthExceeded")]
        public string ISBN { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PublishedDate > DateTime.Now.AddYears(1))
            {
                yield return new ValidationResult("InvalidPublishedDateFuture", new[] { nameof(PublishedDate) });
            }
            if (ISBN.Length != 13 && ISBN.Length != 10)
            {
                yield return new ValidationResult("ISBNMaxLengthExceeded", new[] { nameof(ISBN) });
            }
            if (!ISBN.All(char.IsDigit))
            {
                yield return new ValidationResult(
                    "InvalidISBNFormat",
                    new[] { nameof(ISBN) }
                );
            }
        }
    }

    [AutoMapTo(typeof(Book))]
    public class UpdateBookDto : IValidatableObject
    {
        [Required]
        public int Id { get; set; }

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
        public List<UpdateBookEditionDto> Editions { get; set; } = new List<UpdateBookEditionDto>();

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
        }
    }
}
