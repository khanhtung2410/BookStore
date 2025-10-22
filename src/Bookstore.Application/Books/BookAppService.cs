using Abp.Application.Services;
using Abp.Application.Services.Dto;
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
    public class BookAppService : ApplicationService, IBookAppService
    {
        private readonly IRepository<Book, int> _bookRepository;
        private readonly IRepository<BookInventory, int> _bookInventoryRepository;
        private readonly IRepository<BookEdition, int> _bookEditionRepository;

        public BookAppService(IRepository<Book, int> bookRepository, IRepository<BookInventory, int> bookInventoryRepository, IRepository<BookEdition, int> bookEditionRepository)
        {
            _bookRepository = bookRepository;
            _bookInventoryRepository = bookInventoryRepository;
            _bookEditionRepository = bookEditionRepository;
        }
        [Abp.Authorization.AbpAuthorize("Pages.Books.Create")]
        public async Task<int> CreateBook(CreateBookDto input)
        {
            if (input.Editions == null || !input.Editions.Any())
                throw new UserFriendlyException("A book must have at least one edition.");
            var book = new Book(
                input.Title,
                input.Author,
                input.Genre,
                input.Description
            );
            var createdBookId = await _bookRepository.InsertAndGetIdAsync(book);
            foreach (var editionDto in input.Editions)
            {
                var edition = new BookEdition(
                    createdBookId,
                    editionDto.Format,
                    editionDto.Publisher,
                    editionDto.PublishedDate ?? DateTime.Now,
                    editionDto.ISBN
                );
                var editionId = await _bookEditionRepository.InsertAndGetIdAsync(edition);

                // 3️⃣ For each Edition, create its Inventory
                var inventory = new BookInventory(
                    editionId,
                    editionDto.Inventory.BuyPrice,
                    editionDto.Inventory.SellPrice,
                    editionDto.Inventory.StockQuantity
                );
                await _bookInventoryRepository.InsertAsync(inventory);
            }

            return createdBookId;
        }
        [Abp.Authorization.AbpAuthorize("Pages.Books.Delete")]
        public async Task DeleteBook(DeleteBookDto input)
        {
            await _bookRepository.DeleteAsync(input.Id);
        }

        public async Task<PagedResultDto<ListBookDto>> GetAllBooks(GetAllBooksInput input = null)
        {
            input ??= new GetAllBooksInput();

            var query = _bookRepository.GetAll();

            if (!string.IsNullOrWhiteSpace(input.Keyword))
            {
                query = query.Where(b => b.Title.Contains(input.Keyword) || b.Author.Contains(input.Keyword));
            }

            if (input.Genre.HasValue)
            {
                query = query.Where(b => b.Genre == input.Genre.Value);
            }


            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(b => b.Title)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            var mapped = ObjectMapper.Map<List<ListBookDto>>(items);

            return new PagedResultDto<ListBookDto>(totalCount, mapped);
        }
        public async Task<BookDto> GetBook(int id)
        {
            var book = await _bookRepository.GetAllIncluding(b => b.Editions).FirstOrDefaultAsync(b => b.Id == id);

            if (book == null || book.IsDeleted)
                return null;
            var bookDto = new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                Description = book.Description,
                Genre = book.Genre,
                Editions = new List<BookEditionDto>()
            };
            foreach (var edition in book.Editions)
            {
                var inventory = await _bookInventoryRepository.FirstOrDefaultAsync(bi => bi.BookEditionId == edition.Id);
                bookDto.Editions.Add(new BookEditionDto
                {
                    Id = edition.Id,
                    BookId = edition.BookId,
                    Format = edition.Format,
                    Publisher = edition.Publisher,
                    PublishedDate = edition.PublishedDate,
                    ISBN = edition.ISBN,
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
        public async Task<BookDto> GetBookEditionByIdAsync(int bookId, int bookEditionId)
        {
            var book = await _bookRepository
                .GetAllIncluding(b => b.Editions).Include(b => b.Editions)
                .ThenInclude(e => e.Inventory)
                .FirstOrDefaultAsync(b => b.Id == bookId);

            if (book == null)
                throw new UserFriendlyException("Book not found.");

            // Find the specific edition
            var edition = book.Editions.FirstOrDefault(e => e.Id == bookEditionId);
            if (edition == null)
                throw new UserFriendlyException("Book edition not found.");

            var bookDto = new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                Description = book.Description,
                Genre = book.Genre,
                Editions = new List<BookEditionDto>
        {
            new BookEditionDto
            {
                Id = edition.Id,
                Format = edition.Format,
                ISBN = edition.ISBN,
                PublishedDate = edition.PublishedDate,
                Publisher = edition.Publisher,
                Inventory = edition.Inventory != null
                    ? new BookInventoryDto
                    {
                        Id = edition.Inventory.Id,
                        StockQuantity = edition.Inventory.StockQuantity,
                        BuyPrice = edition.Inventory.BuyPrice,
                        SellPrice = edition.Inventory.SellPrice
                    }
                    : null
            }
        }
            };

            return bookDto;
        }

        [Abp.Authorization.AbpAuthorize("Pages.Books.Update")]
        public async Task UpdateBook(UpdateBookDto input)
        {
            var book = await _bookRepository
                .GetAllIncluding(b => b.Editions)
                .FirstOrDefaultAsync(b => b.Id == input.Id);

            if (book == null)
                throw new UserFriendlyException("Book not found");

            // Update main book fields
            book.Title = input.Title;
            book.Author = input.Author;
            book.Description = input.Description;
            book.Genre = input.Genre;

            var incomingEditionIds = input.Editions?.Where(e => e.Id > 0).Select(e => e.Id).ToList() ?? new List<int>();

            // Delete removed editions
            var deletedEditions = book.Editions
                .Where(e => !incomingEditionIds.Contains(e.Id))
                .ToList();

            foreach (var del in deletedEditions)
            {
                await _bookEditionRepository.DeleteAsync(del);
                book.Editions.Remove(del);
            }

            // Add or update editions (without inventory)
            foreach (var editionInput in input.Editions)
            {
                BookEdition edition;

                if (editionInput.Id > 0)
                {
                    // Existing edition
                    edition = await _bookEditionRepository
                        .FirstOrDefaultAsync(e => e.Id == editionInput.Id);

                    if (edition == null)
                        continue;

                    var duplicate = await _bookEditionRepository
                        .FirstOrDefaultAsync(e => e.ISBN == editionInput.ISBN && e.Id != editionInput.Id);

                    if (duplicate != null)
                    {
                        throw new UserFriendlyException($"Another edition with ISBN {editionInput.ISBN} already exists.");
                    }

                    edition.Format = editionInput.Format;
                    edition.Publisher = editionInput.Publisher;
                    edition.PublishedDate = editionInput.PublishedDate ?? DateTime.Now;
                    edition.ISBN = editionInput.ISBN;

                    await _bookEditionRepository.UpdateAsync(edition);
                }
                else
                {
                    // New edition
                    var existingEdition = await _bookEditionRepository
                        .FirstOrDefaultAsync(e => e.ISBN == editionInput.ISBN);

                    if (existingEdition != null)
                    {
                        throw new UserFriendlyException($"An edition with ISBN {editionInput.ISBN} already exists.");
                    }

                    edition = new BookEdition(
                        book.Id,
                        editionInput.Format,
                        editionInput.Publisher,
                        editionInput.PublishedDate ?? DateTime.Now,
                        editionInput.ISBN
                    );

                    await _bookEditionRepository.InsertAsync(edition);
                    book.Editions.Add(edition);
                }
            }

            await _bookRepository.UpdateAsync(book);
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        public async Task<List<SelectListItemDto>> GetBookGenreAsync()
        {
            return Enum.GetValues(typeof(BookConsts.Genre))
           .Cast<BookConsts.Genre>()
           .Select(g => new SelectListItemDto
           {
               Value = ((int)g).ToString(),
               Text = g.ToString()
           }).ToList();
        }
    }
}
