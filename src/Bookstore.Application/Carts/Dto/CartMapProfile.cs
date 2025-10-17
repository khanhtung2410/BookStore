using Bookstore.Entities.Carts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Carts.Dto
{
    public class CartMapProfile : AutoMapper.Profile
    {
        public CartMapProfile()
        {
            CreateMap<CartItem, CartItemDto>()
                .ForMember(dest => dest.BookTitle, opt => opt.MapFrom(src => src.BookEdition.Book.Title))
                .ForMember(dest => dest.BookAuthor, opt => opt.MapFrom(src => src.BookEdition.Book.Author))
                .ForMember(dest => dest.BookEdition, opt => opt.MapFrom(src => src.BookEdition));

            CreateMap<Cart, CartDto>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));
        }
    }
}
