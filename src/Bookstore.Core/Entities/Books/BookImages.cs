using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Entities.Books
{
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

        public BookImage() { }

        public BookImage(int bookId, string imagePath, string caption, int displayOrder, bool isCover)
        {
            BookId = bookId;
            ImagePath = imagePath;
            Caption = caption;
            DisplayOrder = displayOrder;
            IsCover = isCover;
        }
    }
}
