using Abp.Application.Services;
using Abp.Application.Services.Dto;
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
        Task<PagedResultDto<ListBookDto>> GetAllBooks(GetAllBooksInput input = null);
        Task<BookDto> GetBook(int id);
        Task<BookDto> GetBookEditionByIdAsync(int bookId, int bookEditionId);
    }
    public interface IBookImportAppService : IApplicationService
    {
        Task ImportBooksFromExcel(byte[] fileBytes);
        Task<byte[]> DownloadImportTemplateAsync();

    }

}
