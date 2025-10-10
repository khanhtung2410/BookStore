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
            await UsingDbContextAsync(async context =>
            {
                context.Books.RemoveRange(context.Books);
                context.BookEditions.RemoveRange(context.BookEditions);
                context.BookInventories.RemoveRange(context.BookInventories);
                await context.SaveChangesAsync();
            });

            var input1 = new CreateBookDto
            {
                Title = "Book One",
                Author = "Author One",
                Genre = BookConsts.Genre.Fiction,
                Description = "Description One",
                Editions = new List<CreateBookEditionDto>
                {
                    new CreateBookEditionDto
                    {
                        Format = BookConsts.Format.Hardcover,
                        Publisher = "Publisher One",
                        PublishedDate = DateTime.Now,
                        ISBN = "1111111111111",
                        Inventory = new CreateBookInventoryDto
                        {
                            BuyPrice = 10.0m,
                            SellPrice = 15.0m,
                            StockQuantity = 5
                        }
                    }
                }
            };
            var input2 = new CreateBookDto
            {
                Title = "Book Two",
                Author = "Author Two",
                Genre = BookConsts.Genre.NonFiction,
                Description = "Description Two",
                Editions = new List<CreateBookEditionDto>
                {
                    new CreateBookEditionDto
                    {
                        Format = BookConsts.Format.Paperback,
                        Publisher = "Publisher Two",
                        PublishedDate = DateTime.Now,
                        ISBN = "2222222222222",
                        Inventory = new CreateBookInventoryDto
                        {
                            BuyPrice = 12.0m,
                            SellPrice = 18.0m,
                            StockQuantity = 10
                        }
                    }
                }
            };

            await _bookAppService.CreateBook(input1);
            await _bookAppService.CreateBook(input2);

            var result = await _bookAppService.GetAllBooks();

            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count);

            var bookOne = result.FirstOrDefault(b => b.Title == "Book One");
            var bookTwo = result.FirstOrDefault(b => b.Title == "Book Two");

            Assert.NotNull(bookOne);
            Assert.NotNull(bookTwo);
            Assert.Equal("Author One", bookOne.Author);
            Assert.Equal("Author Two", bookTwo.Author);
        }

        [Fact]
        public async Task CreateAndGetBook_Test()
        {
            await UsingDbContextAsync(async context =>
            {
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
            var bookId = await _bookAppService.CreateBook(input);

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
            await UsingDbContextAsync(async context =>
            {
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
            var bookId = await _bookAppService.CreateBook(createInput);

            var createdBook = await _bookAppService.GetBook(bookId);
            var editionId = createdBook.Editions.First().Id;

            var updatedEdition = new UpdateBookEditionDto
            {
                Id = editionId,
                BookId = bookId,
                Format = BookConsts.Format.Paperback, // Different format
                Publisher = "Updated Publisher",
                PublishedDate = DateTime.Now.AddDays(1),
                ISBN = "9876543210987", // Different ISBN
                Inventory = new CreateBookInventoryDto
                {
                    BuyPrice = 20.0m, // Different price
                    SellPrice = 25.0m,
                    StockQuantity = 3 // Different quantity
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
            Assert.Equal(BookConsts.Format.Paperback, edition.Format);
            Assert.Equal("Updated Publisher", edition.Publisher);
            Assert.Equal("9876543210987", edition.ISBN);
            Assert.NotNull(edition.Pricing);
            Assert.Equal(3, edition.Pricing.StockQuantity);
            Assert.Equal(20.0m, edition.Pricing.BuyPrice);
            Assert.Equal(25.0m, edition.Pricing.SellPrice);
        }

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

            await Assert.ThrowsAsync<AbpValidationException>(() => _bookAppService.CreateBook(input));
        }

        [Fact]
        public async Task CreateBook_Should_Throw_When_Edition_Inventory_Is_Null()
        {
            var input = new CreateBookDto
            {
                Title = "Valid Title",
                Author = "Valid Author",
                Genre = BookConsts.Genre.Fiction,
                Description = "Valid Description",
                Editions = new List<CreateBookEditionDto>
                {
                    new CreateBookEditionDto
                    {
                        Format = BookConsts.Format.Hardcover,
                        Publisher = "Test Publisher",
                        PublishedDate = DateTime.Now,
                        ISBN = "1234567890123",
                        Inventory = null
                    }
                }
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

            await Assert.ThrowsAsync<AbpValidationException>(() => _bookAppService.CreateBook(input));
        }

        [Fact]
        public async Task CreateBook_Should_Throw_When_Genre_Is_Invalid()
        {
            var invalidGenre = (BookConsts.Genre)9999;
            var input = new CreateBookDto
            {
                Title = "Title",
                Author = "Author",
                Genre = invalidGenre,
                Description = "Description",
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

            await Assert.ThrowsAsync<AbpValidationException>(() => _bookAppService.CreateBook(input));
        }
    }
}
