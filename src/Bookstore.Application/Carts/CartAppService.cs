using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.UI;
using Bookstore.Carts.Dto;
using Bookstore.Entities.Books;
using Bookstore.Entities.Carts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Carts
{
    public class CartAppService : ApplicationService, ICartAppService
    {
        private readonly IRepository<Cart, Guid> _cartRepository;
        private readonly IRepository<CartItem, int> _cartItemRepository;
        private readonly IRepository<Book, int> _bookRepository;
        private readonly IRepository<BookEdition, int> _bookEditionRepository;
        public CartAppService(
            IRepository<Cart, Guid> cartRepository,
            IRepository<CartItem, int> cartItemRepository,
            IRepository<Book, int> bookRepository,
            IRepository<BookEdition, int> bookEditionRepository)
        {

            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _bookRepository = bookRepository;
            _bookEditionRepository = bookEditionRepository;
        }
        public async Task<Dto.CartDto> AddItemToCartAsync(long userId, Dto.AddCartItemDto input)
        {
            if (input.BookId == null && input.BookEditionId == null)
            {
                throw new UserFriendlyException("Either BookId or BookEditionId must be provided.");
            }
            if (input.Quantity <= 0)
            {
                throw new UserFriendlyException("Quantity must be greater than zero.");
            }
            var cart = await _cartRepository.GetAllIncluding(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    Items = new List<CartItem>()
                };
                await _cartRepository.InsertAsync(cart);
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            var existingCartItem = await _cartItemRepository.GetAllIncluding(ci => ci.BookEdition).FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.EditionId == input.BookEditionId);
            if (existingCartItem != null)
            {
                existingCartItem.Quantity += (int)input.Quantity;
                await _cartItemRepository.UpdateAsync(existingCartItem);
            }
            else
            {
                var bookEdition = await _bookEditionRepository.GetAllIncluding(be => be.Book).FirstOrDefaultAsync(be => be.Id == input.BookEditionId);
                if (bookEdition == null)
                {
                    throw new UserFriendlyException("Book edition not found.");
                }
                var newCartItem = new CartItem
                {
                    CartId = cart.Id,
                    EditionId = input.BookEditionId,
                    Quantity = (int)input.Quantity,
                    BookEdition = bookEdition
                };
                await _cartItemRepository.InsertAsync(newCartItem);
            }
            await CurrentUnitOfWork.SaveChangesAsync();

            var updatedCart = await _cartRepository.GetAllIncluding(
            c => c.Items).Include(c=>c.Items).ThenInclude(i => i.BookEdition).FirstOrDefaultAsync(c => c.Id == cart.Id);

            return ObjectMapper.Map<Dto.CartDto>(cart);
        }

        public async Task ClearCartAsync(long userId)
        {
            var cart = await _cartRepository.GetAllIncluding(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null || cart.Items.Count == 0)
            {
                throw new UserFriendlyException("Cart is already empty.");
            }
            foreach (var item in cart.Items.ToList())
            {
                await _cartItemRepository.DeleteAsync(item);
            }
            await CurrentUnitOfWork.SaveChangesAsync();
        }
        public async Task<Dto.CartDto> GetCartByUserIdAsync(long userId)
        {
            var cartQuery = _cartRepository.GetAllIncluding(c => c.Items)
        .Include(c => c.Items)
        .ThenInclude(i => i.BookEdition);

            var cart = await cartQuery.FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                return new Dto.CartDto
                {
                    UserId = (int)userId,
                    Items = new List<Dto.CartItemDto>()
                };
            }
            return ObjectMapper.Map<Dto.CartDto>(cart);
        }
        public async Task<Dto.CartDto> RemoveItemFromCartAsync(long userId, Dto.DeleteCartItemDto input)
        {
            var cart = await _cartRepository.GetAllIncluding(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId);
            var cartItem = cart?.Items.FirstOrDefault(i => i.Id == input.CartItemId);
            if (cartItem == null)
            {
                throw new UserFriendlyException("Cart item not found.");
            }
            await _cartItemRepository.DeleteAsync(cartItem);
            await CurrentUnitOfWork.SaveChangesAsync();
            return ObjectMapper.Map<Dto.CartDto>(cart);
        }
        public async Task<Dto.CartDto> UpdateCartItemQuantityAsync(long userId, Dto.UpdateCartItem input)
        {
            var cart = await _cartRepository.GetAllIncluding(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId);
            var cartItem = cart?.Items.FirstOrDefault(i => i.Id == input.CartItemId);
            if (cart == null)
            {
                throw new UserFriendlyException("Cart not found.");
            }
            if (cartItem == null)
            {
                throw new UserFriendlyException("Cart item not found.");
            }
            var bookEdition = await _bookEditionRepository.GetAllIncluding(be => be.Inventory)
                .FirstOrDefaultAsync(be => be.Id == cartItem.EditionId);
            if (input.NewQuantity <= 0)
            {
                throw new UserFriendlyException("Quantity must be greater than zero.");
            }
            if (input.NewQuantity > bookEdition.Inventory.StockQuantity)
            {
                throw new UserFriendlyException("Insufficient stock for the requested quantity.");
            }
            cartItem.Quantity = input.NewQuantity;
            await _cartItemRepository.UpdateAsync(cartItem);
            await CurrentUnitOfWork.SaveChangesAsync();
            return ObjectMapper.Map<Dto.CartDto>(cart);
        }
    }
}
