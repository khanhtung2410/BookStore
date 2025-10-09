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
    [Table("BookBundleImages")]
    public class BookBundleImage : FullAuditedEntity<int>
    {
        [ForeignKey(nameof(BookBundle))]
        public int BundleId { get; set; }
        public virtual BookBundle Bundle { get; set; }

        [Required]
        [StringLength(500)]
        public string ImagePath { get; set; }

        [StringLength(200)]
        public string Caption { get; set; }

        public int DisplayOrder { get; set; } = 0;
        public bool IsCover { get; set; } = false;

        public BookBundleImage() { }

        public BookBundleImage(int bundleId, string imagePath, string caption, int displayOrder, bool isCover)
        {
            BundleId = bundleId;
            ImagePath = imagePath;
            Caption = caption;
            DisplayOrder = displayOrder;
            IsCover = isCover;
        }
    }
}
