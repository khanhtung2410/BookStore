using Abp.Application.Services;
using AutoMapper.Internal.Mappers;
using Bookstore.Books.Dto;
using Bookstore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Books
{
    public interface IBookAppService : IApplicationService
    {
        Task<int> CreateBook(CreateBookDto input);
        Task UpdateBook(UpdateBookDto input);
        Task DeleteBook(DeleteBookDto input);
        Task<List<ListBookDto>> GetAllBooks();
        Task<BookDto> GetBook(int id);
    }
    public interface IBookImportAppService : IApplicationService
    {
        Task ImportBooksFromExcel(byte[] fileBytes);
    }

}
