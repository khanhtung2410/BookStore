using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Entities
{
    [Table("BookEditions")]
    public class BookEdition : FullAuditedEntity<int>, IValidatableObject
    {
        [ForeignKey(nameof(Book))]
        public int BookId { get; set; }
        public virtual Book Book { get; set; }

        [Required]
        public BookConsts.Format Format { get; set; }

        [Required]
        [StringLength(100)]
        public string Publisher { get; set; }

        [Required]
        public DateTime PublishedDate { get; set; }

        [Required]
        [StringLength(13)]
        public string ISBN { get; set; }

        public virtual BookInventory Inventory { get; set; }

        public BookEdition() { }

        public BookEdition(int bookId, BookConsts.Format format, string publisher, DateTime publishedDate, string isbn)
        {
            BookId = bookId;
            Format = format;
            Publisher = publisher;
            PublishedDate = publishedDate;
            ISBN = isbn;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(ISBN))
                yield return new ValidationResult("ISBN is required.", new[] { nameof(ISBN) });

            if (PublishedDate > DateTime.Now.AddYears(1))
                yield return new ValidationResult("Published date cannot be more than 1 year in the future.", new[] { nameof(PublishedDate) });

            if (!Enum.IsDefined(typeof(BookConsts.Format), Format))
                yield return new ValidationResult("Invalid format.", new[] { nameof(Format) });
        }
    }
}
