using AutoMapper;
using Bookstore.Entities;
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
            CreateMap<Book, BookDto>()
                .ForMember(dest =>dest.Genre,opt =>opt.MapFrom(src =>src.Genre.ToString()));
            CreateMap<CreateBookDto, Book>()
                .ForMember(dest => dest.Genre, opt => opt.MapFrom(src => src.Genre.ToString()));
            CreateMap<Book, ListBookDto>()
                .ForMember(des => des.Genre, opt => opt.MapFrom(src => src.Genre.ToString()));
        }
    }
}
