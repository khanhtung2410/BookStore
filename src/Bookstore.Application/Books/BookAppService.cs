using Abp.Application.Editions;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.UI;
using Bookstore.Books.Dto;
using Bookstore.Entities;
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
        public async Task<int> CreateBook(CreateBookDto input)
        {
            var book = new Book(
                input.Title,
                input.Author,
                input.Genre,
                input.Description
            );

            book.Editions = input.Editions?.Select(e => new BookEdition(
                0,
                e.Format,
                e.Publisher,
                e.PublishedDate ?? DateTime.Now,
                e.ISBN
            )).ToList() ?? new List<BookEdition>();

            var createdBookId = await _bookRepository.InsertAndGetIdAsync(book);
            var createdBook = await _bookRepository.GetAsync(createdBookId);

            var createdEditions = createdBook.Editions.ToList(); 

            for (int i = 0; i < input.Editions.Count; i++)
            {
                var editionDto = input.Editions[i];
                var editionEntity = createdEditions[i];

                var inventory = new BookInventory(
                    editionEntity.Id,
                    editionDto.Inventory.BuyPrice,
                    editionDto.Inventory.SellPrice,
                    editionDto.Inventory.StockQuantity
                );

                await _bookInventoryRepository.InsertAsync(inventory);
            }

            return createdBookId;
        }



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
            var book = await _bookRepository.GetAsync(id);

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
                    Pricing = inventory != null ? new BookInventoryDto
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

        public async Task UpdateBook(UpdateBookDto input)
        {
            var book = await _bookRepository.GetAsync(input.Id);
            if (book == null)
            {
                throw new UserFriendlyException("Book not found");
            }
            book.Title = input.Title;
            book.Author = input.Author;
            book.Description = input.Description;
            book.Genre = input.Genre;

            var existingEditionIds = book.Editions.Select(e => e.Id).ToList();
            var incomingEditionIds = input.Editions?.Select(e => e.Id).ToList() ?? new List<int>();

            var deletedEditions = book.Editions
        .Where(e => !incomingEditionIds.Contains(e.Id))
        .ToList();
            foreach (var del in deletedEditions)
            {
                await _bookEditionRepository.DeleteAsync(del);
            }

            //Update or add editions
            foreach (var editionInput in input.Editions)
            {
                var existingEdition = await _bookEditionRepository.FirstOrDefaultAsync(be => be.Id == editionInput.Id);

                if (existingEdition != null)
                {
                    existingEdition.Format = editionInput.Format;
                    existingEdition.Publisher = editionInput.Publisher;
                    existingEdition.PublishedDate = editionInput.PublishedDate ?? DateTime.Now;
                    existingEdition.ISBN = editionInput.ISBN;

                    await _bookEditionRepository.UpdateAsync(existingEdition);
                }
                else
                {
                    var newEdition = new BookEdition(
                        book.Id,
                        editionInput.Format,
                        editionInput.Publisher,
                        editionInput.PublishedDate ?? DateTime.Now,
                        editionInput.ISBN
                    );

                    await _bookEditionRepository.InsertAsync(newEdition);
                }
            }
            //Update or add inventories
            foreach (var editionInput in input.Editions)
            {
                if (editionInput.Inventory == null) continue;

                var inventory = await _bookInventoryRepository
                    .FirstOrDefaultAsync(bi => bi.BookEditionId == editionInput.Id);

                if (inventory != null)
                {
                    inventory.StockQuantity = editionInput.Inventory.StockQuantity;
                    inventory.BuyPrice = editionInput.Inventory.BuyPrice;
                    inventory.SellPrice = editionInput.Inventory.SellPrice;
                    await _bookInventoryRepository.UpdateAsync(inventory);
                }
                else
                {
                    var newInventory = new BookInventory(
                        editionInput.Id,
                        editionInput.Inventory.BuyPrice,
                        editionInput.Inventory.SellPrice,
                                                editionInput.Inventory.StockQuantity

                    );
                    await _bookInventoryRepository.InsertAsync(newInventory);
                }
            }

            await _bookRepository.UpdateAsync(book);
        }

    }
}
