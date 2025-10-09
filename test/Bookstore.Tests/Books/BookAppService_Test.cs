using Abp.Runtime.Validation;
using Bookstore.Books;
using Bookstore.Books.Dto;
using Bookstore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
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
        [Fact]
        public async Task GetAllBooks_Test()
        {
            var result = await _bookAppService.GetAllBooks();
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CreateAndGetBook_Test()
        {
            int bookId = 0;
            await UsingDbContextAsync(async context =>
            {
                // Ensure a clean state for the test
                context.Books.RemoveRange(context.Books);
                context.BookEditions.RemoveRange(context.BookEditions);
                context.BookInventories.RemoveRange(context.BookInventories);
                await context.SaveChangesAsync();
            });

            var input = new CreateBookDto
            {
                Title = "Test Book",
                Author = "Test Author",
                Genre = BookConsts.Genre.Fiction,
                Description = "Test Description",
                Editions = new List<CreateBookEditionDto>
                {
                    new CreateBookEditionDto
                    {
                        Format = BookConsts.Format.Hardcover,
                        Publisher = "Test Publisher",
                        PublishedDate = DateTime.Now,
                        ISBN = "1234567890123",
                        Inventory = new CreateBookInventoryDto
                        {
                            BuyPrice = 5.99m,
                            SellPrice = 9.99m,
                            StockQuantity = 10
                        }
                    }
                }
            };
            bookId = await _bookAppService.CreateBook(input);

            var createdBook = await _bookAppService.GetBook(bookId);

            Assert.NotNull(createdBook);
            Assert.Equal("Test Book", createdBook.Title);
            Assert.Equal("Test Author", createdBook.Author);
            Assert.Equal(BookConsts.Genre.Fiction, createdBook.Genre);
            Assert.Equal("Test Description", createdBook.Description);

            Assert.NotNull(createdBook.Editions);
            Assert.Single(createdBook.Editions);
            var edition = createdBook.Editions.First();
            Assert.NotNull(edition.Pricing);
            Assert.Equal(10, edition.Pricing.StockQuantity);
            Assert.Equal(5.99m, edition.Pricing.BuyPrice);
            Assert.Equal(9.99m, edition.Pricing.SellPrice);
        }

        [Fact]
        public async Task UpdateBook_Test()
        {
            int bookId = 0;
            await UsingDbContextAsync(async context =>
            {
                // Ensure a clean state for the test
                context.Books.RemoveRange(context.Books);
                context.BookEditions.RemoveRange(context.BookEditions);
                context.BookInventories.RemoveRange(context.BookInventories);
                await context.SaveChangesAsync();
            });

            var createInput = new CreateBookDto
            {
                Title = "Original Title",
                Author = "Original Author",
                Genre = BookConsts.Genre.Fiction,
                Description = "Original Description",
                Editions = new List<CreateBookEditionDto>
                {
                    new CreateBookEditionDto
                    {
                        Format = BookConsts.Format.Hardcover,
                        Publisher = "Original Publisher",
                        PublishedDate = DateTime.Now,
                        ISBN = "1234567890123",
                        Inventory = new CreateBookInventoryDto
                        {
                            BuyPrice = 10.0m,
                            SellPrice = 15.0m,
                            StockQuantity = 5
                        }
                    }
                }
            };
            bookId = await _bookAppService.CreateBook(createInput);

            var createdBook = await _bookAppService.GetBook(bookId);
            var editionId = createdBook.Editions.First().Id;

            var updatedEdition = new UpdateBookEditionDto
            {
                Id = editionId,
                BookId = bookId,
                Format = BookConsts.Format.Hardcover,
                Publisher = "Updated Publisher",
                PublishedDate = DateTime.Now.AddDays(1),
                ISBN = "1234567890123",
                Inventory = new CreateBookInventoryDto
                {
                    BuyPrice = 12.0m,
                    SellPrice = 18.0m,
                    StockQuantity = 7
                }
            };

            var updateInput = new UpdateBookDto
            {
                Id = bookId,
                Title = "Updated Title",
                Author = "Updated Author",
                Genre = BookConsts.Genre.NonFiction,
                Description = "Updated Description",
                Editions = new List<UpdateBookEditionDto> { updatedEdition }
            };

            await _bookAppService.UpdateBook(updateInput);

            var updatedBook = await _bookAppService.GetBook(bookId);
            Assert.NotNull(updatedBook);
            Assert.Equal("Updated Title", updatedBook.Title);
            Assert.Equal("Updated Author", updatedBook.Author);
            Assert.Equal(BookConsts.Genre.NonFiction, updatedBook.Genre);
            Assert.Equal("Updated Description", updatedBook.Description);

            Assert.NotNull(updatedBook.Editions);
            Assert.Single(updatedBook.Editions);
            var edition = updatedBook.Editions.First();
            Assert.NotNull(edition.Pricing);
            Assert.Equal(7, edition.Pricing.StockQuantity);
            Assert.Equal(12.0m, edition.Pricing.BuyPrice);
            Assert.Equal(18.0m, edition.Pricing.SellPrice);
        }
    }
}
