using Bookstore.Carts.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Carts
{
   public interface ICartAppService
    {
        Task<Dto.CartDto> GetCartByUserIdAsync(long userId);
        Task<Dto.CartDto> AddItemToCartAsync(long userId, Dto.AddCartItemDto input);
        Task<Dto.CartDto> RemoveItemFromCartAsync(long userId, Dto.DeleteCartItemDto input);
        Task<Dto.CartDto> UpdateCartItemQuantityAsync(long userId, Dto.UpdateCartItemDto input);
        Task ClearCartAsync(long userId);
    }
}
