using Bookstore.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace Bookstore.Web.Models.Books
{
    public class BookDetailViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public BookConsts.Genre Genre { get; set; }
        public string Description { get; set; }
        public IEnumerable<SelectListItem> GenreList { get; set; }
        public List<BookEditionViewModel> Editions { get; set; } = new List<BookEditionViewModel>();
    }

    public class BookEditionViewModel
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public BookConsts.Format Format { get; set; }
        public string Publisher { get; set; }
        public DateTime PublishedDate { get; set; }
        public string ISBN { get; set; }
        public BookInventoryViewModel Inventory { get; set; } = new BookInventoryViewModel();
        public DiscountViewModel? Discount { get; set; }
    }
    public class BookInventoryViewModel
    {
        public long StockQuantity { get; set; }
        public decimal BuyPrice { get; set; }
        public decimal SellPrice { get; set; }
    }
    public class DiscountViewModel
    {
        public decimal DiscountValue { get; set; }
        public bool IsPercentage { get; set; } = true;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
