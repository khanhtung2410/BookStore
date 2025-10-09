using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Entities
{
    public static class BookConsts
    {
        public const int MaxTitleLength = 200;
        public const int MaxAuthorLength = 50;
        public const int MaxDescriptionLength = 1000;
        public enum Genre
        {
            Fiction,
            NonFiction,
            Mystery,
            Thriller,
            Romance,
            Fantasy,
            ScienceFiction,
            Biography,
            History,
            Adventure,
            Children,
            SelfHelp,
            Classic,
            Travel,
            Cooking,
            Horror,
            GraphicNovel
        }
    }

    [Table("Books")]
    public class Book : FullAuditedEntity<int>, IValidatableObject
    {
        [Required]
        [StringLength(BookConsts.MaxTitleLength)]
        public string Title { get; set; }

        [Required]
        [StringLength(BookConsts.MaxAuthorLength)]
        public string Author { get; set; }

        [Required]
        public BookConsts.Genre Genre { get; set; }

        [StringLength(BookConsts.MaxDescriptionLength)]
        public string Description { get; set; }

        // Relationships
        public virtual ICollection<BookInventory> Inventories { get; set; } = new List<BookInventory>();
        public virtual ICollection<BookImage> Images { get; set; } = new List<BookImage>();
        public virtual ICollection<BookEdition> Editions { get; set; } = new List<BookEdition>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                yield return new ValidationResult("Title is required.", new[] { nameof(Title) });
            }
            if (string.IsNullOrWhiteSpace(Author))
            {
                yield return new ValidationResult("Author is required.", new[] { nameof(Author) });
            }
            if (!Enum.IsDefined(typeof(BookConsts.Genre), Genre))
            {
                yield return new ValidationResult("Genre is required and must be a valid value.", new[] { nameof(Genre) });
            }
        }
    }

    [Table("BookEditions")]
    public class BookEdition : FullAuditedEntity<int>, IValidatableObject
    {
        [ForeignKey(nameof(Book))]
        public int BookId { get; set; }
        public virtual Book Book { get; set; }

        [Required]
        [StringLength(100)]
        public string EditionName { get; set; }

        [Required]
        public DateTime? PublishedDate { get; set; }

        [Required]
        [StringLength(50)]
        public string ISBN { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(EditionName))
            {
                yield return new ValidationResult("Edition name is required.", new[] { nameof(EditionName) });
            }
            if (string.IsNullOrWhiteSpace(ISBN))
            {
                yield return new ValidationResult("ISBN is required.", new[] { nameof(ISBN) });
            }
            if (PublishedDate.HasValue && PublishedDate.Value > DateTime.Now.AddYears(1))
            {
                yield return new ValidationResult("Published date cannot be more than 1 year in the future.", new[] { nameof(PublishedDate) });
            }
        }
    }

    [Table("BookInventories")]
    public class BookInventory : FullAuditedEntity<int>
    {
        [ForeignKey(nameof(Book))]
        public int BookId { get; set; } 
        public virtual Book Book { get; set; }

        [Required]
        public long Amount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BuyPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SellPrice { get; set; }
    }

    [Table("BookImages")]
    public class BookImage : FullAuditedEntity<int>
    {
        [ForeignKey(nameof(Book))]
        public int BookId { get; set; }
        public virtual Book Book { get; set; }

        [Required]
        [StringLength(500)]
        public string ImagePath { get; set; }

        [StringLength(200)]
        public string Caption { get; set; }

        public int DisplayOrder { get; set; } = 0;
        public bool IsCover { get; set; } = false;
    }
}