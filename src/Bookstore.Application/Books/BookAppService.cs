using Abp.Application.Services;
using Abp.Domain.Repositories;
using Bookstore.Books.Dto;
using Bookstore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

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

        public Task DeleteBook(DeleteBookDto input)
        {
            _bookRepository.Delete(input.Id);
            return Task.CompletedTask;
        }

        public async Task<List<ListBookDto>> GetAllBooks()
        {
            var books = await _bookRepository.GetAllListAsync();
            return ObjectMapper.Map<List<ListBookDto>>(books);
        }

        public async Task<BookDto> GetBook(int id)
        {
            var book = await _bookRepository.GetAsync(id);
            return ObjectMapper.Map<BookDto>(book);
        }


        public Task UpdateBook(UpdateBookDto input)
        {
            throw new NotImplementedException();
        }

        public Task UpdateBooks(UpdateBooksDto input)
        {
            throw new NotImplementedException();
        }
    }
}
