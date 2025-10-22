using Bookstore.Books;
using Bookstore.Books.Dto;
using Bookstore.Controllers;
using Bookstore.Entities.Books;
using Bookstore.Web.Models.Books;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bookstore.Web.Controllers
{
    public class BooksController : BookstoreControllerBase
    {
        private readonly IBookAppService _bookAppService;

        public BooksController(IBookAppService bookAppService)
        {
            _bookAppService = bookAppService;
        }

        public async Task<ActionResult> Index(int page = 1)
        {
            // Call the service
            var booksResult = await _bookAppService.GetAllBooks();

            // Extract the list of items
            var books = booksResult?.Items ?? new List<ListBookDto>();

            var booksPerPage = 10;
            var pagedBooks = books
                .Skip((page - 1) * booksPerPage)
                .Take(booksPerPage)
                .ToList();

            var model = new Models.Books.BookListViewModel
            {
                Books = pagedBooks,
                CurrentPage = page,
                TotalPages = (books != null)
                    ? (int)Math.Ceiling(books.Count / (double)booksPerPage)
                    : 0
            };

            return View(model);
        }


        public async Task<ActionResult> Detail(int id)
        {
            var book = await _bookAppService.GetBook(id);
            if (book == null )
            {
                TempData["ErrorMessage"] = "This book is no longer available.";
                return RedirectToAction("Index"); // redirect to list page
            }
            var model = new BookDetailViewModel
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                Description = book.Description,
                Genre = book.Genre,
                Editions = book.Editions != null
                    ? book.Editions.Select(e => new BookEditionViewModel
                    {
                        Id = e.Id,
                        BookId = e.BookId,
                        Format = e.Format,
                        Publisher = e.Publisher,
                        PublishedDate = e.PublishedDate,
                        ISBN = e.ISBN,
                        Inventory = e.Inventory != null ? new BookInventoryViewModel
                        {
                            StockQuantity = e.Inventory.StockQuantity,
                            BuyPrice = e.Inventory.BuyPrice,
                            SellPrice = e.Inventory.SellPrice
                        } : null,
                        Discount = e.Discount != null ? new DiscountViewModel
                        {
                            // Map Discount properties here if needed
                        } : null
                    }).ToList()
                    : new List<BookEditionViewModel>(),
                GenreList = Enum.GetValues(typeof(BookConsts.Genre))
                    .Cast<BookConsts.Genre>()
                    .Select(g => new SelectListItem
                    {
                        Value = g.ToString(),
                        Text = g.ToString(),
                        Selected = g.ToString() == book.Genre.ToString()
                    }).ToList()
            };
            return View(model);
        }

        public ActionResult Create()
        {
            var model = new Models.Books.BookCreateViewModel();

            model.GenreList = Enum.GetValues(typeof(BookConsts.Genre))
            .Cast<BookConsts.Genre>()
            .Select(g => new SelectListItem
            {
                Value = g.ToString(),
                Text = g.ToString()
            }).ToList();
           
            model.Editions = new List<BookEditionCreateViewModel>
            {
                new BookEditionCreateViewModel
                {
                    FormatList = Enum.GetValues(typeof(BookConsts.Format)).Cast<BookConsts.Format>()
                    .Select(f => new SelectListItem
                    {
                        Value = f.ToString(),
                        Text = f.ToString()
                    }).ToList()
                }
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookCreateViewModel model)
        {
            // Validate presence of nested objects
            if (model == null || model.Editions == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid book data. Please add an Edition.");
            }

            if (!ModelState.IsValid)
            {
                model ??= new BookCreateViewModel();

                // Repopulate GenreList for the select element
                model.GenreList = Enum.GetValues(typeof(BookConsts.Genre))
                    .Cast<BookConsts.Genre>()
                    .Select(g => new SelectListItem
                    {
                        Value = g.ToString(),
                        Text = g.ToString()
                    }).ToList();

                return View(model);
            }

            var dto = new CreateBookDto
            {
                Title = model.Title,
                Author = model.Author,
                Genre = model.Genre,
                Description = model.Description,
                Editions = model.Editions.Select(e => new CreateBookEditionDto
                {
                    Format = e.Format,
                    PublishedDate = e.PublishedDate,
                    Publisher = e.Publisher,
                    ISBN = e.ISBN,
                    Inventory = new CreateBookInventoryDto
                    {
                        StockQuantity = e.Inventory.StockQuantity,
                        BuyPrice = e.Inventory.BuyPrice,
                        SellPrice = e.Inventory.SellPrice
                    }
                }).ToList()
            };

            await _bookAppService.CreateBook(dto);

            // On success, redirect to the list page
            return RedirectToAction(nameof(Index));
        }
    }
}
