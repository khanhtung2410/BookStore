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
            CreateMap<BookEdition, BookEditionDto>()
                .ForMember(dest => dest.Inventory, opt => opt.MapFrom(src => src.Inventory));
            CreateMap<Book, BookDto>()
                .ForMember(dest => dest.Editions, opt => opt.MapFrom(src => src.Editions));
            CreateMap<BookInventory, BookInventoryDto>();
        }
    }
}
