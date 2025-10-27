using System;
using System.ComponentModel.DataAnnotations;

namespace Bookstore.Books.Dto
{
    public class BookInventoryDto
    {
        public int Id { get; set; }
        public int BookEditionId { get; set; }
        public long StockQuantity { get; set; }
        public decimal BuyPrice { get; set; }
        public decimal SellPrice { get; set; }
    }

    public class DiscountDto
    {
        public int Id { get; set; }
        [Required]
        public int BookEditionId { get; set; }
        public decimal DiscountValue { get; set; }

        public bool IsPercentage { get; set; } = true;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }
    }
}
