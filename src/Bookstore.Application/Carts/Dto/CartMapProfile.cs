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
            CreateMap<Cart, CartDto>();
            CreateMap<CartItem, CartItemDto>()
                .ForMember(dest => dest.BookEdition, opt => opt.MapFrom(src => src.BookEdition))
                .ForMember(dest => dest.Book, opt => opt.MapFrom(src => src.BookEdition.Book));
        }
    }
}
