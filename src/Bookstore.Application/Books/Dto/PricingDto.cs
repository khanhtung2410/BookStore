using System;
using System.ComponentModel.DataAnnotations;

namespace Bookstore.Books.Dto
{
    public class BookInventoryDto
    {
        public int Id { get; set; }
        [Required]
        public int BookEditionId { get; set; }

        [Range(0, long.MaxValue, ErrorMessage = "Stock quantity cannot be negative.")]
        public long StockQuantity { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Buy price must be non-negative.")]
        public decimal BuyPrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Sell price must be non-negative.")]
        public decimal SellPrice { get; set; }
    }

    public class DiscountDto
    {
        public int Id { get; set; }
        [Required]
        public int BookEditionId { get; set; }

        [Range(0, 100, ErrorMessage = "Discount must be between 0 and 100.")]
        public decimal DiscountValue { get; set; }

        public bool IsPercentage { get; set; } = true;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }
    }
}
