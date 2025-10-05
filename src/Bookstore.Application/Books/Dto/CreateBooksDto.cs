using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Bookstore.Books.Dto
{
    public class CreateBooksDto : IValidatableObject
    {
        [Required]
        public List<CreateBookDto> Books { get; set; } = new List<CreateBookDto>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Books == null || Books.Count == 0)
            {
                yield return new ValidationResult("At least one book must be provided.", new[] { nameof(Books) });
            }

            // Validate each book individually
            foreach (var book in Books)
            {
                var results = new List<ValidationResult>();
                var context = new ValidationContext(book, null, null);
                if (!Validator.TryValidateObject(book, context, results, true))
                {
                    foreach (var validationResult in results)
                    {
                        // Prefix the error with the book index for clarity
                        yield return new ValidationResult(
                            $"Book [{Books.IndexOf(book)}]: {validationResult.ErrorMessage}",
                            validationResult.MemberNames
                        );
                    }
                }
            }
        }
    }
}
