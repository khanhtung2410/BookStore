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
    public class UpdateBookEditionDto
    {
        public int Id { get; set; }
        [Required]
        public int BookId { get; set; }
        [Required]
        public BookConsts.Format Format { get; set; }
        [Required]
        [StringLength(100)]
        public string Publisher { get; set; }
        [Required]
        public DateTime? PublishedDate { get; set; }
        [Required]
        [StringLength(50)]
        public string ISBN { get; set; }

        [Required]
        public CreateBookInventoryDto Inventory { get; set; } 
    }

    [AutoMapTo(typeof(Book))]
    public class UpdateBookDto : IValidatableObject
    {
        [Required]
        public int Id { get; set; }

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

        [Required]
        public List<UpdateBookEditionDto> Editions { get; set; } = new List<UpdateBookEditionDto>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!Enum.IsDefined(typeof(BookConsts.Genre), Genre))
            {
                yield return new ValidationResult("Genre is required and must be a valid value.", new[] { nameof(Genre) });
            }
            if (Editions == null || Editions.Count == 0)
            {
                yield return new ValidationResult("At least one edition is required.", new[] { nameof(Editions) });
            }
            foreach (var edition in Editions)
            {
                if (edition.Inventory == null)
                {
                    yield return new ValidationResult("Inventory is required for each edition.", new[] { nameof(Editions) });
                }
            }
        }
    }
}
