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
    [Table("BookInventories")]
    public class BookInventory : FullAuditedEntity<int>, IValidatableObject
    {
        [ForeignKey(nameof(BookEdition))]
        public int BookEditionId { get; set; }
        public virtual BookEdition BookEdition { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BuyPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SellPrice { get; set; }

        [Required]
        public long StockQuantity { get; set; }

        public BookInventory() { }

        public BookInventory(int bookEditionId, decimal buyPrice, decimal sellPrice, long stockQuantity)
        {
            BookEditionId = bookEditionId;
            BuyPrice = buyPrice;
            SellPrice = sellPrice;
            StockQuantity = stockQuantity;
        }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (BuyPrice < 0)
                yield return new ValidationResult("Buy price cannot be negative.", new[] { nameof(BuyPrice) });
            if (SellPrice < 0)
                yield return new ValidationResult("Sell price cannot be negative.", new[] { nameof(SellPrice) });
            if (StockQuantity < 0)
                yield return new ValidationResult("Stock quantity cannot be negative.", new[] { nameof(StockQuantity) });
        }
    }

}
