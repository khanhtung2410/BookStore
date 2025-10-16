using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.UI;
using Bookstore.Books.Dto;
using Bookstore.Entities.Books;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bookstore.Books
{
    /// <summary>
    /// Application service for managing books, editions, and inventories.
    /// </summary>
    public class BookAppService : ApplicationService, IBookAppService
    {
        private readonly IRepository<Book, int> _bookRepository;
        private readonly IRepository<BookInventory, int> _bookInventoryRepository;
        private readonly IRepository<BookEdition, int> _bookEditionRepository;

        /// <summary>
        /// Constructor for <see cref="BookAppService"/>.
        /// </summary>
        public BookAppService(
            IRepository<Book, int> bookRepository,
            IRepository<BookInventory, int> bookInventoryRepository,
            IRepository<BookEdition, int> bookEditionRepository)
        {
            _bookRepository = bookRepository;
            _bookInventoryRepository = bookInventoryRepository;
            _bookEditionRepository = bookEditionRepository;
        }

        /// <summary>
        /// Creates a new book with editions and inventories.
        /// </summary>
        /// <param name="input">The book data.</param>
        /// <returns>The created book ID.</returns>
        /// <exception cref="UserFriendlyException">Thrown when no editions are provided.</exception>
        [Abp.Authorization.AbpAuthorize("Pages.Books.Create")]
        public async Task<int> CreateBook(CreateBookDto input)
        {
            if (input.Editions == null || input.Editions.Count==0)
                throw new UserFriendlyException("A book must have at least one edition.");

            var book = new Book(
                input.Title ?? throw new ArgumentNullException(nameof(input.Title)),
                input.Author ?? throw new ArgumentNullException(nameof(input.Author)),
                input.Genre,
                input.Description ?? throw new ArgumentNullException(nameof(input.Description))
            );

            var createdBookId = await _bookRepository.InsertAndGetIdAsync(book);

            foreach (var editionDto in input.Editions)
            {
                var edition = new BookEdition(
                    createdBookId,
                    editionDto.Format,
                    editionDto.Publisher ?? throw new ArgumentNullException(nameof(editionDto.Publisher)),
                    editionDto.PublishedDate ?? DateTime.Now,
                    editionDto.ISBN ?? throw new ArgumentNullException(nameof(editionDto.ISBN))
                );

                var editionId = await _bookEditionRepository.InsertAndGetIdAsync(edition);

                if (editionDto.Inventory != null)
                {
                    var inventory = new BookInventory(
                        editionId,
                        editionDto.Inventory.BuyPrice,
                        editionDto.Inventory.SellPrice,
                        editionDto.Inventory.StockQuantity
                    );
                    await _bookInventoryRepository.InsertAsync(inventory);
                }
            }

            return createdBookId;
        }

        /// <summary>
        /// Deletes a book by its ID.
        /// </summary>
        /// <param name="input">The delete DTO containing the book ID.</param>
        [Abp.Authorization.AbpAuthorize("Pages.Books.Delete")]
        public async Task DeleteBook(DeleteBookDto input)
        {
            await _bookRepository.DeleteAsync(input.Id);
        }

        /// <summary>
        /// Gets all books.
        /// </summary>
        /// <returns>List of books.</returns>
        public async Task<List<ListBookDto>> GetAllBooks()
        {
            var books = await _bookRepository.GetAllListAsync();
            return ObjectMapper.Map<List<ListBookDto>>(books);
        }

        /// <summary>
        /// Gets a single book including editions and inventories.
        /// </summary>
        /// <param name="id">The book ID.</param>
        /// <returns>Book DTO.</returns>
        /// <exception cref="UserFriendlyException">Thrown when book is not found.</exception>
        public async Task<BookDto> GetBook(int id)
        {
            var book = await _bookRepository.GetAllIncluding(b => b.Editions)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
                throw new UserFriendlyException("Book not found.");

            var bookDto = new BookDto
            {
                Id = book.Id,
                Title = book.Title ?? string.Empty,
                Author = book.Author ?? string.Empty,
                Description = book.Description ?? string.Empty,
                Genre = book.Genre,
                Editions = new List<BookEditionDto>()
            };

            foreach (var edition in book.Editions ?? Enumerable.Empty<BookEdition>())
            {
                var inventory = await _bookInventoryRepository.FirstOrDefaultAsync(bi => bi.BookEditionId == edition.Id);

                bookDto.Editions.Add(new BookEditionDto
                {
                    Id = edition.Id,
                    BookId = edition.BookId,
                    Format = edition.Format,
                    Publisher = edition.Publisher ?? string.Empty,
                    PublishedDate = edition.PublishedDate,
                    ISBN = edition.ISBN ?? string.Empty,
                    Inventory = inventory != null ? new BookInventoryDto
                    {
                        Id = inventory.Id,
                        BookEditionId = inventory.BookEditionId,
                        BuyPrice = inventory.BuyPrice,
                        SellPrice = inventory.SellPrice,
                        StockQuantity = inventory.StockQuantity
                    } : null,
                    Discount = null
                });
            }

            return bookDto;
        }

        /// <summary>
        /// Updates a book and its editions/inventories.
        /// </summary>
        /// <param name="input">The book update DTO.</param>
        /// <exception cref="UserFriendlyException">Thrown when book is not found.</exception>
        [Abp.Authorization.AbpAuthorize("Pages.Books.Update")]
        public async Task UpdateBook(UpdateBookDto input)
        {
            var book = await _bookRepository.GetAllIncluding(b => b.Editions)
                .FirstOrDefaultAsync(b => b.Id == input.Id);

            if (book == null)
                throw new UserFriendlyException("Book not found.");

            book.Title = input.Title ?? string.Empty;
            book.Author = input.Author ?? string.Empty;
            book.Description = input.Description ?? string.Empty;
            book.Genre = input.Genre;

            var incomingEditionIds = input.Editions?.Where(e => e.Id > 0).Select(e => e.Id).ToList() ?? new List<int>();

            // Remove deleted editions
            var deletedEditions = book.Editions?.Where(e => !incomingEditionIds.Contains(e.Id)).ToList() ?? new List<BookEdition>();
            foreach (var del in deletedEditions)
            {
                await _bookEditionRepository.DeleteAsync(del);
                book.Editions?.Remove(del);
            }

            // Add/update editions
            foreach (var editionInput in input.Editions)
            {
                BookEdition edition;

                if (editionInput.Id > 0)
                {
                    // Update existing
                    edition = await _bookEditionRepository.GetAllIncluding(e => e.Inventory)
                        .FirstOrDefaultAsync(e => e.Id == editionInput.Id);

                    if (edition == null) continue;

                    edition.Format = editionInput.Format;
                    edition.Publisher = editionInput.Publisher ?? string.Empty;
                    edition.PublishedDate = editionInput.PublishedDate ?? DateTime.Now;
                    edition.ISBN = editionInput.ISBN ?? string.Empty;

                    if (editionInput.Inventory != null)
                    {
                        if (edition.Inventory != null)
                        {
                            edition.Inventory.StockQuantity = editionInput.Inventory.StockQuantity;
                            edition.Inventory.BuyPrice = editionInput.Inventory.BuyPrice;
                            edition.Inventory.SellPrice = editionInput.Inventory.SellPrice;
                            await _bookInventoryRepository.UpdateAsync(edition.Inventory);
                        }
                        else
                        {
                            var newInventory = new BookInventory(
                                edition.Id,
                                editionInput.Inventory.BuyPrice,
                                editionInput.Inventory.SellPrice,
                                editionInput.Inventory.StockQuantity
                            );
                            await _bookInventoryRepository.InsertAsync(newInventory);
                        }
                    }

                    await _bookEditionRepository.UpdateAsync(edition);
                }
                else
                {
                    // New edition
                    edition = new BookEdition(
                        book.Id,
                        editionInput.Format,
                        editionInput.Publisher ?? string.Empty,
                        editionInput.PublishedDate ?? DateTime.Now,
                        editionInput.ISBN ?? string.Empty
                    );

                    await _bookEditionRepository.InsertAsync(edition);
                    await CurrentUnitOfWork.SaveChangesAsync();

                    if (editionInput.Inventory != null)
                    {
                        var newInventory = new BookInventory(
                            edition.Id,
                            editionInput.Inventory.BuyPrice,
                            editionInput.Inventory.SellPrice,
                            editionInput.Inventory.StockQuantity
                        );
                        await _bookInventoryRepository.InsertAsync(newInventory);
                    }

                    book.Editions?.Add(edition);
                }
            }

            await _bookRepository.UpdateAsync(book);
            await CurrentUnitOfWork.SaveChangesAsync();
        }
    }
}
