using Bookstore.Books;
using Bookstore.Books.Dto;
using Bookstore.Controllers;
using Bookstore.Entities;
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
            var books = await _bookAppService.GetAllBooks() ?? new List<ListBookDto>();

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
            ? (int)Math.Ceiling(books.Count / 10.0)
            : 0
            };

            return View(model);
        }

        public async Task<ActionResult> Detail(int id)
        {
            var book = await _bookAppService.GetBook(id);
            if (book == null)
            {
                return NotFound();
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

        //public ActionResult Create()
        //{
        //    var model = new Models.Books.BookCreatePageViewModel();
        //    model.Book = new Models.Books.BookCreateViewModel();

        //    model.Book.GenreList = Enum.GetValues(typeof(BookConsts.Genre))
        //    .Cast<BookConsts.Genre>()
        //    .Select(g => new SelectListItem
        //    {
        //        Value = g.ToString(),
        //        Text = g.ToString()
        //    }).ToList();

        //    return View(model);
        //}

        //        [HttpPost]
        //        [ValidateAntiForgeryToken]
        //        public async Task<IActionResult> Create(BookCreatePageViewModel model)
        //        {
        //            // Validate presence of nested objects
        //            if (model?.Book == null || model.Book.Inventory == null)
        //            {
        //                ModelState.AddModelError(string.Empty, "Invalid book data. Please fill in all fields.");
        //            }

        //            if (!ModelState.IsValid)
        //            {
        //                // Ensure model and nested Book are not null so the view can render safely
        //                model ??= new BookCreatePageViewModel();
        //                model.Book ??= new BookCreateViewModel();

        //                // Repopulate GenreList for the select element
        //                model.Book.GenreList = Enum.GetValues(typeof(BookConsts.Genre))
        //                    .Cast<BookConsts.Genre>()
        //                    .Select(g => new SelectListItem
        //                    {
        //                        Value = g.ToString(),
        //                        Text = g.ToString()
        //                    }).ToList();

        //                return View(model);
        //            }

        //            var dto = new CreateBookDto
        //            {
        //                Title = model.Book.Title,
        //                Author = model.Book.Author,
        //                Genre = model.Book.Genre,
        //                Description = model.Book.Description,
        //                PublishedDate = model.Book.PublishedDate,
        //                Inventory = new CreateBookInventoryDto
        //                {
        //                    Amount = model.Book.Inventory.Amount,
        //                    BuyPrice = model.Book.Inventory.BuyPrice,
        //                    SellPrice = model.Book.Inventory.SellPrice
        //                }
        //            };

        //            await _bookAppService.CreateBook(dto);

        //            // On success, redirect to the list page
        //            return RedirectToAction(nameof(Index));
        //        }

        //        [HttpPost]
        //        [ValidateAntiForgeryToken]
        //        public async Task<IActionResult> Delete(int id)
        //        {
        //            var book = await _bookAppService.GetBook(id);
        //            if (book == null)
        //            {
        //                return NotFound();
        //            }
        //            await _bookAppService.DeleteBook(new DeleteBookDto { Id = id });
        //            return Ok();
        //        }

        //        public async Task<ActionResult> Update(int id)
        //        {
        //            var book = await _bookAppService.GetBook(id);
        //            if (book == null)
        //            {
        //                return NotFound();
        //            }


        //            var model = new BookUpdateViewModel
        //            {
        //                Id = book.Id,
        //                Title = book.Title,
        //                Author = book.Author,
        //                Description = book.Description,
        //                Genre = book.Genre,
        //                Inventory = book.Inventory != null ?
        //                new BookInventoryViewModel
        //                {
        //                    Amount = book.Inventory.Amount,
        //                    BuyPrice = book.Inventory.BuyPrice,
        //                    SellPrice = book.Inventory.SellPrice
        //                } : new BookInventoryViewModel()
        //            };

        //            model.GenreList = Enum.GetValues(typeof(BookConsts.Genre))
        //                .Cast<BookConsts.Genre>()
        //                .Select(g => new SelectListItem
        //                {
        //                    Value = g.ToString(),
        //                    Text = g.ToString(),
        //                    Selected = g.ToString() == model.Genre.ToString()
        //                }).ToList();

        //            return View(model);
        //        }

        //        [HttpPost]
        //        [ValidateAntiForgeryToken]
        //        public async Task<IActionResult> Update(int id, BookUpdateViewModel model)
        //        {
        //            if (id != model.Id)
        //            {
        //                return NotFound();
        //            }

        //            if (!ModelState.IsValid)
        //            {
        //                model.GenreList = Enum.GetValues(typeof(BookConsts.Genre))
        //               .Cast<BookConsts.Genre>()
        //               .Select(g => new SelectListItem
        //               {
        //                   Value = g.ToString(),
        //                   Text = g.ToString(),
        //                   Selected = g.ToString() == model.Genre.ToString()
        //               }).ToList();
        //                return View(model);
        //            }

        //            var dto = new UpdateBookDto
        //            {
        //                Id = model.Id,
        //                Title = model.Title,
        //                Author = model.Author,
        //                Description = model.Description,
        //                PublishedDate = model.PublishedDate,
        //                Genre = model.Genre,
        //                Inventory = new UpdateBookInventoryDto
        //                {
        //                    Amount = model.Inventory.Amount,
        //                    BuyPrice = model.Inventory.BuyPrice,
        //                    SellPrice = model.Inventory.SellPrice
        //                }
        //            };

        //            await _bookAppService.UpdateBook(dto);
        //            return RedirectToAction(nameof(Index));

        //        }
    }
}
