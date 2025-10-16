using Bookstore.Entities.Books;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace Bookstore.Web.Models.Books
{
    public class BookUpdateViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public BookConsts.Genre Genre { get; set; }
        public string Description { get; set; }
        [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever]
        public IEnumerable<SelectListItem> GenreList { get; set; }
        public List<BookEditionViewModel> Editions { get; set; } = new();
    }
}
