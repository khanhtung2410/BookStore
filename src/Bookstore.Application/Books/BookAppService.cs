using Abp.Application.Editions;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.UI;
using Bookstore.Books.Dto;
using Bookstore.Entities.Books;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

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

        public async Task<List<ListBookDto>> GetAllBooks()
        {
            var books = await _bookRepository.GetAllListAsync();
            return ObjectMapper.Map<List<ListBookDto>>(books);
        }

        public async Task<BookDto> GetBook(int id)
        {
            var book = await _bookRepository.GetAllIncluding(b => b.Editions).FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
                throw new UserFriendlyException("Book not found.");
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
        [Abp.Authorization.AbpAuthorize("Pages.Books.Update")]
        public async Task UpdateBook(UpdateBookDto input)
        {
            var book = await _bookRepository
                .GetAllIncluding(b => b.Editions)
                .FirstOrDefaultAsync(b => b.Id == input.Id);

            if (book == null)
                throw new UserFriendlyException("Book not found");

            // Update main fields
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

            // Add/update editions and inventories
            foreach (var editionInput in input.Editions)
            {
                BookEdition edition;

                if (editionInput.Id > 0)
                {
                    // Existing edition
                    edition = await _bookEditionRepository
                        .GetAllIncluding(e => e.Inventory)
                        .FirstOrDefaultAsync(e => e.Id == editionInput.Id);

                    if (edition == null)
                        continue;

                    edition.Format = editionInput.Format;
                    edition.Publisher = editionInput.Publisher;
                    edition.PublishedDate = editionInput.PublishedDate ?? DateTime.Now;
                    edition.ISBN = editionInput.ISBN;

                    // Inventory update
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
                        editionInput.Publisher,
                        editionInput.PublishedDate ?? DateTime.Now,
                        editionInput.ISBN
                    );

                    await _bookEditionRepository.InsertAsync(edition);
                    await CurrentUnitOfWork.SaveChangesAsync(); // Ensure edition.Id is generated

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

                    book.Editions.Add(edition);
                }
            }

            await _bookRepository.UpdateAsync(book);
            await CurrentUnitOfWork.SaveChangesAsync();
        }


    }
}
