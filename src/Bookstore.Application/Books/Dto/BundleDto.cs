using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Books.Dto
{
    public class BundleDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string BundleName { get; set; }
        [Required]
        public string Description { get; set; }
        public decimal SellPrice { get; set; }
        public DiscountDto? Discount { get; set; }
        public List<BundleItemDto> Items { get; set; } = new List<BundleItemDto>();
        public List<string> ImageUrls { get; set; } = new List<string>();
    }
    public class BundleItemDto
    {
        [Required]
        public int BookId { get; set; }
        [Required]
        public string Title { get; set; }
        [Required, Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
