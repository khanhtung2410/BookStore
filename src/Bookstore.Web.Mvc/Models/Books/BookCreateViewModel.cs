using Bookstore.Books.Dto;
using Bookstore.Entities.Books;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Bookstore.Web.Models.Books
{
    public class BookCreateViewModel
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public BookConsts.Genre Genre { get; set; }
        public string Description { get; set; }
        public DateTime? PublishedDate { get; set; }
        public IEnumerable<SelectListItem> GenreList { get; set; }
        public List<BookEditionCreateViewModel> Editions { get; set; } = new ();
    }
    public class BookEditionCreateViewModel
    {
        public BookConsts.Format Format { get; set; }
        public string Publisher { get; set; }
        public DateTime? PublishedDate { get; set; }
        public string ISBN { get; set; }
        public IEnumerable<SelectListItem> FormatList { get; set; }
        public BookInventoryViewModel Inventory { get; set; } = new BookInventoryViewModel();

    }
}
