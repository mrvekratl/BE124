using App.Data.Entities;
using App.Data.Infrastructure;
using App.Eticaret.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.Eticaret.Controllers
{
    public class OrderController : BaseController
    {
        private readonly IDataRepository<OrderEntity> _orderRepository;
        private readonly IDataRepository<OrderItemEntity> _orderItemRepository;
        private readonly IDataRepository<CartItemEntity> _cartItemRepository;

        public OrderController(
            IDataRepository<OrderEntity> orderRepository,
            IDataRepository<OrderItemEntity> orderItemRepository,
            IDataRepository<CartItemEntity> cartItemRepository)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _orderItemRepository = orderItemRepository ?? throw new ArgumentNullException(nameof(orderItemRepository));
            _cartItemRepository = cartItemRepository ?? throw new ArgumentNullException(nameof(cartItemRepository));
        }

        [HttpPost("/order")]
        public async Task<IActionResult> Create([FromForm] CheckoutViewModel model)
        {
            var userId = GetUserId();

            if (userId == null)
            {
                return RedirectToAction(nameof(AuthController.Login), "Auth", new { returnUrl = Request.Path });
            }

            if (!ModelState.IsValid)
            {
                var viewModel = await GetCartItemsAsync();
                return View(viewModel);
            }

            var cartItems = await _cartItemRepository.GetAllAsync();
            cartItems = cartItems.Where(ci => ci.UserId == userId).ToList();

            if (cartItems == null)
            {
                return RedirectToAction(nameof(CartController.Edit), "Cart");
            }

            var order = new OrderEntity
            {
                UserId = userId.Value,
                Address = model.Address,
                OrderCode = CreateOrderCode(),
                CreatedAt = DateTime.UtcNow
            };

            // Add Order to DB using DataRepository
            await _orderRepository.AddAsync(order);

            var orderItems = cartItems.Select(cartItem => new OrderItemEntity
            {
                OrderId = order.Id,
                ProductId = cartItem.ProductId,
                Quantity = cartItem.Quantity,
                UnitPrice = cartItem.Product.Price,
                CreatedAt = DateTime.UtcNow,
            }).ToList();

            foreach (var orderItem in orderItems)
            {
                await _orderItemRepository.AddAsync(orderItem);
            }

            // Remove cart items from cart after adding to order
            foreach (var cartItem in cartItems)
            {
                await _cartItemRepository.DeleteAsync(cartItem.Id);
            }

            return RedirectToAction(nameof(Details), new { orderCode = order.OrderCode });
        }

        [HttpGet("/order/{orderCode}/details")]
        public async Task<IActionResult> Details([FromRoute] string orderCode)
        {
            var userId = GetUserId();

            if (userId == null)
            {
                return RedirectToAction(nameof(AuthController.Login), "Auth", new { returnUrl = Request.Path });
            }

            var order = await _orderRepository.GetAllAsync();
            var orderEntity = order.FirstOrDefault(o => o.UserId == userId && o.OrderCode == orderCode);

            if (orderEntity == null)
            {
                return NotFound();
            }

            var orderDetailsViewModel = new OrderDetailsViewModel
            {
                OrderCode = orderEntity.OrderCode,
                CreatedAt = orderEntity.CreatedAt,
                Address = orderEntity.Address,
                Items = (await _orderItemRepository.GetAllAsync())
                    .Where(oi => oi.OrderId == orderEntity.Id)
                    .Select(oi => new OrderItemViewModel
                    {
                        ProductName = oi.Product.Name,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                    })
                    .ToList()
            };

            return View(orderDetailsViewModel);
        }

        private string CreateOrderCode()
        {
            return Guid.NewGuid().ToString("n")[..16].ToUpperInvariant();
        }

        private async Task<List<CartItemViewModel>> GetCartItemsAsync()
        {
            var userId = GetUserId() ?? -1;

            return (await _cartItemRepository.GetAllAsync())
                .Where(ci => ci.UserId == userId)
                .Select(ci => new CartItemViewModel
                {
                    Id = ci.Id,
                    ProductName = ci.Product.Name,
                    ProductImage = ci.Product.Images.Count != 0 ? ci.Product.Images.First().Url : null,
                    Quantity = ci.Quantity,
                    Price = ci.Product.Price
                })
                .ToList();
        }
    }

}