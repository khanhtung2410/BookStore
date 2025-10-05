using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Bookstore.Books.Dto
{
    public class UpdateBooksDto : IValidatableObject
    {
        [Required]
        public List<UpdateBookDto> Books { get; set; } = new List<UpdateBookDto>();

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
                        // Include the index in the error message for clarity
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
