using App.Data.Entities;
using App.Data.Infrastructure;
using App.Eticaret.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace App.Eticaret.Controllers
{
    public class CartController : BaseController
    {
        private readonly IDataRepository<CartItemEntity> _cartItemRepository;
        private readonly IDataRepository<ProductEntity> _productRepository;
        private readonly IDataRepository<ProductImageEntity> _imageRepository;

        public CartController(
            IDataRepository<CartItemEntity> cartItemRepository,
            IDataRepository<ProductEntity> productRepository,
            IDataRepository<ProductImageEntity> imageRepository)
        {
            _cartItemRepository = cartItemRepository ?? throw new ArgumentNullException(nameof(cartItemRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _imageRepository = imageRepository ?? throw new ArgumentNullException(nameof(imageRepository));
        }

        [HttpGet("/add-to-cart/{productId:int}")]
        public async Task<IActionResult> AddProduct([FromRoute] int productId)
        {
            var userId = GetUserId();

            if (userId is null)
            {
                return RedirectToAction(nameof(AuthController.Login), "Auth");
            }

            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            var cartItems = await _cartItemRepository.GetAllAsync();
            var cartItem = cartItems.FirstOrDefault(ci => ci.UserId == userId && ci.ProductId == productId);

            if (cartItem != null)
            {
                cartItem.Quantity++;
                await _cartItemRepository.UpdateAsync(cartItem);  // Update if exists
            }
            else
            {
                cartItem = new CartItemEntity
                {
                    UserId = userId.Value,
                    ProductId = productId,
                    Quantity = 1,
                    CreatedAt = DateTime.UtcNow
                };

                await _cartItemRepository.AddAsync(cartItem);  // Add new item
            }

            var prevUrl = Request.Headers.Referer.FirstOrDefault();
            if (prevUrl is null)
            {
                return RedirectToAction(nameof(Edit));
            }

            return Redirect(prevUrl);
        }

        [HttpGet("/cart")]
        public async Task<IActionResult> Edit()
        {
            var userId = GetUserId();

            if (userId is null)
            {
                return RedirectToAction(nameof(AuthController.Login), "Auth");
            }

            List<CartItemViewModel> cartItems = await GetCartItemsAsync();
            return View(cartItems);
        }

        [HttpGet("/cart/{cartItemId:int}/remove")]
        public async Task<IActionResult> Remove([FromRoute] int cartItemId)
        {
            var userId = GetUserId();

            if (userId is null)
            {
                return RedirectToAction(nameof(AuthController.Login), "Auth");
            }

            var cartItems = await _cartItemRepository.GetAllAsync();
            var cartItem = cartItems.FirstOrDefault(ci => ci.UserId == userId && ci.Id == cartItemId);

            if (cartItem == null)
            {
                return NotFound();
            }

            await _cartItemRepository.DeleteAsync(cartItem.Id);  // Delete cart item by Id

            return RedirectToAction(nameof(Edit));
        }

        [HttpPost("/cart/update")]
        public async Task<IActionResult> UpdateCart(int cartItemId, byte quantity)
        {
            var userId = GetUserId();

            if (userId is null)
            {
                return RedirectToAction(nameof(AuthController.Login), "Auth");
            }

            var cartItems = await _cartItemRepository.GetAllAsync();
            var cartItem = cartItems.FirstOrDefault(ci => ci.UserId == userId && ci.Id == cartItemId);

            if (cartItem == null)
            {
                return NotFound();
            }

            cartItem.Quantity = quantity;
            await _cartItemRepository.UpdateAsync(cartItem);  // Update the cart item

            var model = new CartItemViewModel
            {
                Id = cartItem.Id,
                ProductName = cartItem.Product.Name,
                ProductImage = cartItem.Product.Images.FirstOrDefault()?.Url,
                Quantity = cartItem.Quantity,
                Price = cartItem.Product.Price
            };

            return View(model);
        }

        [HttpGet("/checkout")]
        public async Task<IActionResult> Checkout()
        {
            var userId = GetUserId();

            if (userId is null)
            {
                return RedirectToAction(nameof(AuthController.Login), "Auth");
            }

            List<CartItemViewModel> cartItems = await GetCartItemsAsync();
            return View(cartItems);
        }

        private async Task<List<CartItemViewModel>> GetCartItemsAsync()
        {
            var userId = GetUserId() ?? -1;

            var cartItems = await _cartItemRepository.GetAllAsync();
            return cartItems
                .Where(ci => ci.UserId == userId)
                .Select(ci => new CartItemViewModel
                {
                    Id = ci.Id,
                    ProductName = ci.Product.Name,
                    ProductImage = ci.Product.Images.FirstOrDefault()?.Url,
                    Quantity = ci.Quantity,
                    Price = ci.Product.Price
                })
                .ToList();
        }
    }


}