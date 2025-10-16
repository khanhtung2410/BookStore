using Abp.Domain.Entities.Auditing;
using Bookstore.Entities.Books;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Entities.Carts
{
    [Table("CartItems")]
    public class CartItem : AuditedEntity, IValidatableObject
    {
        [ForeignKey(nameof(Cart))]
        public Guid CartId { get; set; }
        [ForeignKey(nameof(Books.BookEdition))]
        public int? EditionId { get; set; }
        [Required]
        public int Quantity { get; set; }
        public virtual BookEdition? BookEdition { get; set; }

        public CartItem() { }
        public CartItem(Guid cartId, int editionId, int quantity)
        {
            CartId = cartId;
            EditionId = editionId;
            Quantity = quantity;
        }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Quantity <= 0)
                yield return new ValidationResult("Quantity must be greater than zero.", new[] { nameof(Quantity) });
        }
    }
}
