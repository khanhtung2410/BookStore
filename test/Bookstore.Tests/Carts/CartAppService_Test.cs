using Abp.UI;
using Bookstore.Carts;
using Bookstore.Carts.Dto;
using Bookstore.Entities.Books;
using Bookstore.Entities.Carts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Bookstore.Tests.Carts
{
    public class CartAppService_Test : BookstoreTestBase
    {
        private readonly ICartAppService _cartAppService;

        public CartAppService_Test()
        {
            _cartAppService = Resolve<ICartAppService>();
        }

        private async Task<BookEdition> CreateTestBookEdition()
        {
            return await UsingDbContextAsync(async context =>
            {
                var book = new Book
                {
                    Title = "Cart Test Book",
                    Author = "Author",
                    Genre = BookConsts.Genre.Fiction,
                    Description = "Description"
                };

                var edition = new BookEdition
                {
                    Book = book,
                    Format = BookConsts.Format.Paperback,
                    Publisher = "Test Publisher",
                    PublishedDate = DateTime.Now,
                    ISBN = "1234567890123",
                    Inventory = new BookInventory
                    {
                        BuyPrice = 10.0m,
                        SellPrice = 15.0m,
                        StockQuantity = 10
                    }
                };

                context.Books.Add(book);
                context.BookEditions.Add(edition);
                await context.SaveChangesAsync();

                return edition;
            });
        }

        [Fact]
        public async Task AddItemToCart_Should_Create_New_Cart_If_Not_Exists()
        {
            // Arrange
            var userId = 1L;
            var edition = await CreateTestBookEdition();

            var input = new AddCartItemDto
            {
                BookEditionId = edition.Id,
                Quantity = 2
            };

            // Act
            var result = await _cartAppService.AddItemToCartAsync(userId, input);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.Single(result.Items);
            Assert.Equal(2, result.Items.First().Quantity);
        }

        [Fact]
        public async Task AddItemToCart_Should_Increase_Quantity_If_Item_Already_Exists()
        {
            // Arrange
            var userId = 2L;
            var edition = await CreateTestBookEdition();

            var input = new AddCartItemDto
            {
                BookEditionId = edition.Id,
                Quantity = 1
            };

            await _cartAppService.AddItemToCartAsync(userId, input);
            await _cartAppService.AddItemToCartAsync(userId, input);

            // Act
            var cart = await _cartAppService.GetCartByUserIdAsync(userId);

            // Assert
            Assert.Single(cart.Items);
            Assert.Equal(2, cart.Items.First().Quantity);
        }

        [Fact]
        public async Task GetCartByUserId_Should_Return_Empty_When_No_Cart()
        {
            // Arrange
            var userId = 999L;

            // Act
            var result = await _cartAppService.GetCartByUserIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.Empty(result.Items);
        }

        [Fact]
        public async Task UpdateCartItemQuantity_Should_Update_Successfully()
        {
            // Arrange
            var userId = 3L;
            var edition = await CreateTestBookEdition();

            var addInput = new AddCartItemDto
            {
                BookEditionId = edition.Id,
                Quantity = 1
            };

            await _cartAppService.AddItemToCartAsync(userId, addInput);
            var cart = await _cartAppService.GetCartByUserIdAsync(userId);
            var itemId = cart.Items.First().Id;

            var updateInput = new UpdateCartItemDto
            {
                CartItemId = itemId,
                NewQuantity = 5
            };

            // Act
            var result = await _cartAppService.UpdateCartItemQuantityAsync(userId, updateInput);

            // Assert
            Assert.Single(result.Items);
            Assert.Equal(5, result.Items.First().Quantity);
        }

        [Fact]
        public async Task UpdateCartItemQuantity_Should_Throw_If_Stock_Not_Enough()
        {
            // Arrange
            var userId = 4L;
            var edition = await CreateTestBookEdition();
            edition.Inventory.StockQuantity = 3;

            await UsingDbContextAsync(async context =>
            {
                context.BookInventories.Update(edition.Inventory);
                await context.SaveChangesAsync();
            });

            var addInput = new AddCartItemDto
            {
                BookEditionId = edition.Id,
                Quantity = 1
            };

            await _cartAppService.AddItemToCartAsync(userId, addInput);
            var cart = await _cartAppService.GetCartByUserIdAsync(userId);
            var itemId = cart.Items.First().Id;

            var updateInput = new UpdateCartItemDto
            {
                CartItemId = itemId,
                NewQuantity = 10
            };

            // Act & Assert
            await Assert.ThrowsAsync<UserFriendlyException>(() =>
                _cartAppService.UpdateCartItemQuantityAsync(userId, updateInput));
        }

        [Fact]
        public async Task RemoveItemFromCart_Should_Remove_Item_Successfully()
        {
            // Arrange
            var userId = 5L;
            var edition = await CreateTestBookEdition();

            var addInput = new AddCartItemDto
            {
                BookEditionId = edition.Id,
                Quantity = 1
            };

            await _cartAppService.AddItemToCartAsync(userId, addInput);
            var cart = await _cartAppService.GetCartByUserIdAsync(userId);
            var itemId = cart.Items.First().Id;

            var deleteInput = new DeleteCartItemDto
            {
                CartItemId = itemId
            };

            // Act
            await _cartAppService.RemoveItemFromCartAsync(userId, deleteInput);
            var updatedCart = await _cartAppService.GetCartByUserIdAsync(userId);

            // Assert
            Assert.Empty(updatedCart.Items);
        }

        [Fact]
        public async Task ClearCart_Should_Remove_All_Items()
        {
            // Arrange
            var userId = 6L;
            var edition = await CreateTestBookEdition();

            var addInput = new AddCartItemDto
            {
                BookEditionId = edition.Id,
                Quantity = 2
            };

            await _cartAppService.AddItemToCartAsync(userId, addInput);
            await _cartAppService.AddItemToCartAsync(userId, addInput);

            var beforeClear = await _cartAppService.GetCartByUserIdAsync(userId);
            Assert.NotEmpty(beforeClear.Items);

            // Act
            await _cartAppService.ClearCartAsync(userId);
            var afterClear = await _cartAppService.GetCartByUserIdAsync(userId);

            // Assert
            Assert.Empty(afterClear.Items);
        }

        [Fact]
        public async Task AddItemToCart_Should_Throw_When_Quantity_Is_Zero()
        {
            var userId = 7L;
            var edition = await CreateTestBookEdition();

            var input = new AddCartItemDto
            {
                BookEditionId = edition.Id,
                Quantity = 0
            };

            await Assert.ThrowsAsync<UserFriendlyException>(() => _cartAppService.AddItemToCartAsync(userId, input));
        }

        [Fact]
        public async Task AddItemToCart_Should_Throw_When_BookEdition_Not_Found()
        {
            var userId = 8L;

            var input = new AddCartItemDto
            {
                BookEditionId = 999,
                Quantity = 1
            };

            await Assert.ThrowsAsync<UserFriendlyException>(() => _cartAppService.AddItemToCartAsync(userId, input));
        }
    }
}
