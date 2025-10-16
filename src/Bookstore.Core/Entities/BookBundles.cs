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
    [Table("BookBundles")]
    public class BookBundle : FullAuditedEntity<int>, IValidatableObject
    {
        [Required]
        [StringLength(200)]
        public string BundleName { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SellPrice { get; set; }

        // Relationships
        public virtual ICollection<BookBundleItem> BundleItems { get; set; } = new List<BookBundleItem>();
        public virtual ICollection<BookBundleImage> Images { get; set; } = new List<BookBundleImage>();

        public BookBundle() { }

        public BookBundle(string bundleName, string description, decimal price)
        {
            BundleName = bundleName;
            Description = description;
            SellPrice = price;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(BundleName))
            {
                yield return new ValidationResult("Bundle name is required.", new[] { nameof(BundleName) });
            }

            if (SellPrice < 0)
            {
                yield return new ValidationResult("Price must be non-negative.", new[] { nameof(SellPrice) });
            }
        }
    }
}
