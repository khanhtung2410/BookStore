using Abp.UI;
using Bookstore.Authorization.Users;
using Bookstore.Books;
using Bookstore.Carts;
using Bookstore.Carts.Dto;
using Bookstore.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bookstore.Web.Controllers
{
    public class CartsController : BookstoreControllerBase
    {
        private readonly ICartAppService _cartAppService;
        private readonly IBookAppService _bookAppService;

        public CartsController(ICartAppService cartAppService,IBookAppService bookAppService)
        {
            _cartAppService = cartAppService;
            _bookAppService = bookAppService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var cart = await _cartAppService.GetCartByUserIdAsync(userId);
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddItem(int bookId, int bookEditionId, int quantity)
        {
            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var input = new AddCartItemDto
            {
                BookId = bookId,
                BookEditionId = bookEditionId,
                Quantity = quantity
            };
            var book = await _bookAppService.GetBookEditionByIdAsync(bookId, bookEditionId);
            try
            {
                await _cartAppService.AddItemToCartAsync(userId, input);
            }
            catch (UserFriendlyException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("Index","Books");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveItem(DeleteCartItemDto input)
        {
            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            try
            {
                await _cartAppService.RemoveItemFromCartAsync(userId, input);
            }
            catch (UserFriendlyException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                await _cartAppService.ClearCartAsync(userId);
            }
            catch (UserFriendlyException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCartItemQuantity(UpdateCartItemDto input)
        {
            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                await _cartAppService.UpdateCartItemQuantityAsync(userId, input);
            }
            catch (UserFriendlyException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}
