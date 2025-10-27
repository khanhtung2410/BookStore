//using Abp.Application.Services.Dto;
//using Abp.Domain.Repositories;
//using Abp.UI;
//using Bookstore.Books;
//using Bookstore.Books.Dto;
//using Bookstore.Entities.Books;
//using Shouldly;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Xunit;

//namespace Bookstore.Tests.Books
//{
//    public class BookAppService_Tests : BookstoreTestBase
//    {
//        private readonly IBookAppService _bookAppService;

//        public BookAppService_Tests()
//        {
//            _bookAppService = Resolve<IBookAppService>();
//        }

//        [Fact]
//        public async Task Should_Create_Book_With_Valid_Data()
//        {
//            // Arrange
//            var input = new CreateBookDto
//            {
//                Title = "The Hobbit",
//                Author = "J.R.R. Tolkien",
//                Genre = BookConsts.Genre.Fantasy,
//                Description = "A classic fantasy adventure.",
//                Editions = new[]
//                {
//                    new CreateBookEditionDto
//                    {
//                        Format = BookConsts.Format.Hardcover,
//                        Publisher = "Allen & Unwin",
//                        PublishedDate = DateTime.Now.AddMonths(-3),
//                        ISBN = "1234567890"
//                    }
//                }.ToList()
//            };

//            // Act
//            var bookId = await _bookAppService.CreateBook(input);
//            var book = await _bookAppService.GetBook(bookId);

//            // Assert
//            book.ShouldNotBeNull();
//            book.Title.ShouldBe("The Hobbit");
//            book.Author.ShouldBe("J.R.R. Tolkien");
//            book.Description.ShouldBe("A classic fantasy adventure.");
//            book.Editions.Count.ShouldBe(1);
//            book.Editions.First().ISBN.ShouldBe("1234567890");
//        }

//        [Fact]
//        public async Task Should_Throw_If_ISBN_Already_Exists()
//        {
//            // Arrange
//            var input1 = new CreateBookDto
//            {
//                Title = "Book A",
//                Author = "Author A",
//                Description = "First book with ISBN.",
//                Genre = BookConsts.Genre.Mystery,
//                Editions = new[]
//                {
//                    new CreateBookEditionDto
//                    {
//                        Format = BookConsts.Format.Paperback,
//                        Publisher = "Pub1",
//                        ISBN = "1111111111",
//                        PublishedDate = DateTime.Now
//                    }
//                }.ToList()
//            };

//            var input2 = new CreateBookDto
//            {
//                Title = "Book B",
//                Author = "Author B",
//                Description = "Second book with same ISBN.",
//                Genre = BookConsts.Genre.Mystery,
//                Editions = new[]
//                {
//                    new CreateBookEditionDto
//                    {
//                        Format = BookConsts.Format.Hardcover,
//                        Publisher = "Pub2",
//                        ISBN = "1111111111", // duplicate ISBN
//                        PublishedDate = DateTime.Now
//                    }
//                }.ToList()
//            };

//            await _bookAppService.CreateBook(input1);

//            // Act + Assert
//            await Should.ThrowAsync<UserFriendlyException>(async () =>
//            {
//                await _bookAppService.CreateBook(input2);
//            });
//        }

//        [Fact]
//        public async Task Should_Merge_Books_With_Same_Title_And_Author()
//        {
//            // Arrange
//            var input1 = new CreateBookDto
//            {
//                Title = "1984",
//                Author = "George Orwell",
//                Description = "Dystopian masterpiece.",
//                Genre = BookConsts.Genre.Adventure,
//                Editions = new[]
//                {
//                    new CreateBookEditionDto
//                    {
//                        Format = BookConsts.Format.Paperback,
//                        Publisher = "Secker & Warburg",
//                        ISBN = "5555555555",
//                        PublishedDate = DateTime.Now.AddMonths(-5)
//                    }
//                }.ToList()
//            };

//            var input2 = new CreateBookDto
//            {
//                Title = "1984",
//                Author = "George Orwell", // same author
//                Description = "Duplicate entry to merge.",
//                Genre = BookConsts.Genre.Adventure,
//                Editions = new[]
//                {
//                    new CreateBookEditionDto
//                    {
//                        Format = BookConsts.Format.Hardcover,
//                        Publisher = "Penguin",
//                        ISBN = "6666666666",
//                        PublishedDate = DateTime.Now.AddMonths(-1)
//                    }
//                }.ToList()
//            };

//            // Act
//            var id1 = await _bookAppService.CreateBook(input1);
//            var id2 = await _bookAppService.CreateBook(input2);

//            // Assert
//            id2.ShouldBe(id1); // same book, merged editions
//            var book = await _bookAppService.GetBook(id1);
//            book.Editions.Count.ShouldBe(2);
//        }

//        [Fact]
//        public async Task Should_Throw_If_PublishedDate_Exceeds_One_Year_Ahead()
//        {
//            // Arrange
//            var input = new CreateBookDto
//            {
//                Title = "Future Book",
//                Author = "Futurist",
//                Description = "Set too far in the future.",
//                Genre = BookConsts.Genre.Children,
//                Editions = new[]
//                {
//                    new CreateBookEditionDto
//                    {
//                        Format = BookConsts.Format.Paperback,
//                        Publisher = "Tomorrow Press",
//                        ISBN = "2222222222",
//                        PublishedDate = DateTime.Now.AddYears(2) // invalid
//                    }
//                }.ToList()
//            };

//            // Act & Assert
//            await Should.ThrowAsync<UserFriendlyException>(async () =>
//            {
//                await _bookAppService.CreateBook(input);
//            });
//        }

//        [Fact]
//        public async Task Should_Update_Book_And_Add_New_Edition()
//        {
//            // Arrange
//            var createInput = new CreateBookDto
//            {
//                Title = "Test Update Book",
//                Author = "Updater",
//                Description = "Original book before update.",
//                Genre = BookConsts.Genre.Fantasy,
//                Editions = new[]
//                {
//                    new CreateBookEditionDto
//                    {
//                        Format = BookConsts.Format.Paperback,
//                        Publisher = "InitPub",
//                        ISBN = "9999999999",
//                        PublishedDate = DateTime.Now.AddMonths(-2)
//                    }
//                }.ToList()
//            };

//            var bookId = await _bookAppService.CreateBook(createInput);
//            var existingBook = await _bookAppService.GetBook(bookId);
//            var editionId = existingBook.Editions.First().Id;

//            var updateInput = new UpdateBookDto
//            {
//                Id = bookId,
//                Title = "Test Update Book (Updated)",
//                Author = "Updater",
//                Description = "Book updated with new edition.",
//                Genre = BookConsts.Genre.Fantasy,
//                Editions = new[]
//                {
//                    new UpdateBookEditionDto
//                    {
//                        Id = editionId,
//                        Format = BookConsts.Format.Paperback,
//                        Publisher = "InitPub",
//                        ISBN = "9999999999",
//                        PublishedDate = DateTime.Now.AddMonths(-2)
//                    },
//                    new UpdateBookEditionDto
//                    {
//                        Id = 0, // new edition
//                        Format = BookConsts.Format.Hardcover,
//                        Publisher = "NewPublisher",
//                        ISBN = "7777777777",
//                        PublishedDate = DateTime.Now.AddMonths(-1)
//                    }
//                }.ToList()
//            };

//            // Act
//            await _bookAppService.UpdateBook(updateInput);
//            var updated = await _bookAppService.GetBook(bookId);

//            // Assert
//            updated.Title.ShouldBe("Test Update Book (Updated)");
//            updated.Editions.Count.ShouldBe(2);
//        }

//        [Fact]
//        public async Task GetBookImagesAsync_ShouldReturnImages_WhenBookExists()
//        {
//            // Arrange: create a book with images
//            var bookId = await _bookAppService.CreateBook(new CreateBookDto
//            {
//                Title = "Integration Test Book",
//                Author = "Tester",
//                Genre = BookConsts.Genre.Fantasy,
//                Description = "Testing images",
//                Editions = new[]
//                {
//                    new CreateBookEditionDto
//                    {
//                        Format = BookConsts.Format.Hardcover,
//                        Publisher = "Test Publisher",
//                        ISBN = "1234567890",
//                        PublishedDate = System.DateTime.Now
//                    }
//                }.ToList()
//            });

//            // Add images directly via repository
//            await UsingDbContextAsync(async context =>
//            {
//                context.BookImages.Add(new Entities.Books.BookImage
//                {
//                    BookId = bookId,
//                    ImagePath = "/uploads/books/test1.jpg",
//                    Caption = "Image 1",
//                    DisplayOrder = 0,
//                    IsCover = true
//                });

//                context.BookImages.Add(new Entities.Books.BookImage
//                {
//                    BookId = bookId,
//                    ImagePath = "/uploads/books/test2.jpg",
//                    Caption = "Image 2",
//                    DisplayOrder = 1,
//                    IsCover = false
//                });

//                await context.SaveChangesAsync();
//            });

//            // Act
//            var result = await _bookAppService.GetBookImagesAsync(bookId);

//            // Assert
//            result.ShouldNotBeNull();
//            result.Items.Count.ShouldBe(2);
//            result.Items.First().ImagePath.ShouldBe("/uploads/books/test1.jpg");
//        }
//    }
//}

    
