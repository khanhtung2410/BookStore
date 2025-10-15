using AutoMapper;
using Bookstore.Entities.Books;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Books.Dto
{
    public class BookMapProfile : Profile
    {
        public BookMapProfile()
        {
            CreateMap<Book, ListBookDto>();
            CreateMap<BookEdition, BookEditionDto>();
            CreateMap<BookInventory, BookInventoryDto>();
        }
    }
}
