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
    [Table("BookDiscounts")]
    public class BookDiscount : FullAuditedEntity<int>, IValidatableObject
    {
        [ForeignKey(nameof(BookEdition))]
        public int BookEditionId { get; set; }
        public virtual BookEdition BookEdition { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal DiscountValue { get; set; }

        public bool IsPercentage { get; set; } = true;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public BookDiscount() { }
        public BookDiscount(int bookEditionId, decimal discountValue, bool isPercentage, DateTime startDate, DateTime endDate)
        {
            BookEditionId = bookEditionId;
            DiscountValue = discountValue;
            IsPercentage = isPercentage;
            StartDate = startDate;
            EndDate = endDate;
        }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DiscountValue < 0)
                yield return new ValidationResult("Discount value cannot be negative.", new[] { nameof(DiscountValue) });
            if (IsPercentage && (DiscountValue < 0 || DiscountValue > 100))
                yield return new ValidationResult("Percentage discount must be between 0 and 100.", new[] { nameof(DiscountValue) });
            if (StartDate >= EndDate)
                yield return new ValidationResult("Start date must be before end date.", new[] { nameof(StartDate), nameof(EndDate) });
        }
    }
    }
