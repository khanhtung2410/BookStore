using Abp.Runtime.Validation;
using Bookstore.Books;
using Bookstore.Books.Dto;
using Bookstore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Bookstore.Tests.Books
{
    public class BookAppService_Test : BookstoreTestBase
    {
        private readonly IBookAppService _bookAppService;
        public BookAppService_Test()
        {
            _bookAppService = Resolve<IBookAppService>();
        }

        #region Get Tests

        [Fact]
        public async Task GetAllBooks_Test()
        {
            var books = await _bookAppService.GetAllBooks();
            Assert.NotNull(books);
        }

        [Fact]
        public async Task CreateAndGetBook_Test()
        {
            var input = new CreateBookDto
            {
                Title = "Test Book",
                Author = "Test Author",
                Genre = BookConsts.Genre.Fiction,
                Description = "Test Description",
                PublishedDate = DateTime.Now,
                Inventory = new CreateBookInventoryDto
                {
                    Amount = 10,
                    BuyPrice = 5.99m,
                    SellPrice = 9.99m
                }
            };
            var bookId = await _bookAppService.CreateBook(input);

            var createdBook = await _bookAppService.GetBook(bookId);

            Assert.NotNull(createdBook);
            Assert.Equal("Test Book", createdBook.Title);
            Assert.Equal("Test Author", createdBook.Author);
            Assert.Equal(BookConsts.Genre.Fiction.ToString(), createdBook.Genre);
            Assert.Equal("Test Description", createdBook.Description);
            Assert.Equal(input.PublishedDate, createdBook.PublishedDate);

            // Check inventory
            UsingDbContext(context =>
            {
                var inventory = context.BookInventories.FirstOrDefault(x => x.BookId == bookId);
                Assert.NotNull(inventory);
                Assert.Equal(10, inventory.Amount);
                Assert.Equal(5.99m, inventory.BuyPrice);
                Assert.Equal(9.99m, inventory.SellPrice);
            });
        }

        #endregion

        #region Validation Tests

        [Theory]
        [InlineData(null, "Author", BookConsts.Genre.Fiction, "Description")] // Title null
        [InlineData("Title", null, BookConsts.Genre.Fiction, "Description")] // Author null
        [InlineData("Title", "Author", BookConsts.Genre.Fiction, null)] // Description null
        [InlineData("", "Author", BookConsts.Genre.Fiction, "Description")] // Title empty
        [InlineData("Title", "", BookConsts.Genre.Fiction, "Description")] // Author empty
        [InlineData("Title", "Author", BookConsts.Genre.Fiction, "")] // Description empty
        public async Task CreateBook_Should_Throw_When_RequiredFieldsMissing(string title, string author, BookConsts.Genre genre, string description)
        {
            var input = new CreateBookDto
            {
                Title = title,
                Author = author,
                Genre = genre,
                Description = description,
                PublishedDate = DateTime.Now,
                Inventory = new CreateBookInventoryDto
                {
                    Amount = 10,
                    BuyPrice = 5.99m,
                    SellPrice = 9.99m
                }
            };

            await Assert.ThrowsAsync<AbpValidationException>(() => _bookAppService.CreateBook(input));
        }

        [Fact]
        public async Task CreateBook_Should_Throw_When_Inventory_Is_Null()
        {
            var input = new CreateBookDto
            {
                Title = "Valid Title",
                Author = "Valid Author",
                Genre = BookConsts.Genre.Fiction,
                Description = "Valid Description",
                PublishedDate = DateTime.Now,
                Inventory = null
            };

            await Assert.ThrowsAsync<AbpValidationException>(() => _bookAppService.CreateBook(input));
        }

        [Fact]
        public async Task CreateBook_Should_Throw_When_Title_Too_Long()
        {
            var input = new CreateBookDto
            {
                Title = new string('A', BookConsts.MaxTitleLength + 1),
                Author = "Author",
                Genre = BookConsts.Genre.Fiction,
                Description = "Description",
                PublishedDate = DateTime.Now,
                Inventory = new CreateBookInventoryDto
                {
                    Amount = 10,
                    BuyPrice = 5.99m,
                    SellPrice = 9.99m
                }
            };

            await Assert.ThrowsAsync<AbpValidationException>(() => _bookAppService.CreateBook(input));
        }

        [Fact]
        public async Task CreateBook_Should_Throw_When_Genre_Is_Invalid()
        {
            // Try to cast an invalid integer to the Genre enum
            var invalidGenre = (BookConsts.Genre)9999;
            var input = new CreateBookDto
            {
                Title = "Title",
                Author = "Author",
                Genre = invalidGenre,
                Description = "Description",
                PublishedDate = DateTime.Now,
                Inventory = new CreateBookInventoryDto
                {
                    Amount = 10,
                    BuyPrice = 5.99m,
                    SellPrice = 9.99m
                }
            };

            await Assert.ThrowsAsync<AbpValidationException>(() => _bookAppService.CreateBook(input));
        }

        #endregion
    }
}
