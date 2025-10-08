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

        public BookAppService(IRepository<Book, int> bookRepository, IRepository<BookInventory, int> bookInventoryRepository)
        {
            _bookRepository = bookRepository;
            _bookInventoryRepository = bookInventoryRepository;
        }
        public async Task<int> CreateBook(CreateBookDto input)
        {
            Book book = new Book(
                input.Title,
                input.Author,
                input.Genre,
                input.Description,
                input.PublishedDate
            );

            var bookId = await _bookRepository.InsertAndGetIdAsync(book);

            // Create inventory
            var inventory = new BookInventory(
                bookId,
                input.Inventory.Amount,
                input.Inventory.BuyPrice,
                input.Inventory.SellPrice
            );
            await _bookInventoryRepository.InsertAsync(inventory);

            return bookId;
        }

        public Task CreateBooks(CreateBooksDto input)
        {
            throw new NotImplementedException();
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
            var inventory = await _bookInventoryRepository.FirstOrDefaultAsync(bi => bi.BookId == id);

            var dto = ObjectMapper.Map<BookDto>(book);

            if (inventory != null)
            {
                dto.Inventory = new BookInventoryDto
                {
                    Id = inventory.Id,
                    BookId = inventory.BookId,
                    Amount = (int)inventory.Amount,
                    BuyPrice = inventory.BuyPrice,
                    SellPrice = inventory.SellPrice
                };
            }

            return dto;
        }

        public async Task UpdateBook(UpdateBookDto input)
        {
            var book = await _bookRepository.GetAsync(input.Id);
            if(book == null)
            {
                throw new UserFriendlyException("Book not found");
            }
            book.Title = input.Title;
            book.Author = input.Author;
            book.Description = input.Description;
            book.PublishedDate = input.PublishedDate;
            book.Genre = input.Genre;
            if(input.Inventory != null)
            {
                var inventory = await _bookInventoryRepository.FirstOrDefaultAsync(bi => bi.BookId == book.Id);
                if(inventory != null)
                {
                    inventory.Amount = input.Inventory.Amount;
                    inventory.BuyPrice = input.Inventory.BuyPrice;
                    inventory.SellPrice = input.Inventory.SellPrice;
                    await _bookInventoryRepository.UpdateAsync(inventory);
                }
                else
                {
                    // Create new inventory if not exists
                    var newInventory = new BookInventory(
                        book.Id,
                        input.Inventory.Amount,
                        input.Inventory.BuyPrice,
                        input.Inventory.SellPrice
                    );
                    await _bookInventoryRepository.InsertAsync(newInventory);
                }
            }
        }

        public Task UpdateBooks(UpdateBooksDto input)
        {
            throw new NotImplementedException();
        }
    }
}
