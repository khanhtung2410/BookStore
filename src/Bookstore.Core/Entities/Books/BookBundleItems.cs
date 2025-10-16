using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Entities.Books
{
    [Table("BookBundleItems")]
    public class BookBundleItem : FullAuditedEntity<int>
    {
        [ForeignKey(nameof(BookBundle))]
        public int BundleId { get; set; }
        public virtual BookBundle Bundle { get; set; }

        [ForeignKey(nameof(Book))]
        public int BookId { get; set; }
        public virtual Book Book { get; set; }

        public int Quantity { get; set; } = 1;

        public BookBundleItem() { }

        public BookBundleItem(int bundleId, int bookId, int quantity)
        {
            BundleId = bundleId;
            BookId = bookId;
            Quantity = quantity;
        }
    }
}
