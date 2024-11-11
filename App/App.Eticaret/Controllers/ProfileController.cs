using App.Data.Entities;
using App.Data.Infrastructure;
using App.Eticaret.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.Eticaret.Controllers
{
    public class ProfileController : BaseController
    {
        private readonly IDataRepository<UserEntity> _userRepository;
        private readonly IDataRepository<OrderEntity> _orderRepository;
        private readonly IDataRepository<ProductEntity> _productRepository;
        private readonly IDataRepository<SellerRequestEntity> _sellerRequestRepository;

        // Constructor
        public ProfileController(IDataRepository<UserEntity> userRepository,
                                 IDataRepository<OrderEntity> orderRepository,
                                 IDataRepository<ProductEntity> productRepository,
                                 IDataRepository<SellerRequestEntity> sellerRequestRepository)
        {
            _userRepository = userRepository;
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _sellerRequestRepository = sellerRequestRepository;
        }

        [HttpGet("/profile")]
        public async Task<IActionResult> Details()
        {
            var userId = GetUserId();

            if (userId is null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _userRepository.GetByIdAsync(userId.Value);

            if (user is null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var userViewModel = new ProfileDetailsViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            };

            string? previousSuccessMessage = TempData["SuccessMessage"]?.ToString();

            if (previousSuccessMessage is not null)
            {
                SetSuccessMessage(previousSuccessMessage);
            }

            return View(userViewModel);
        }

        [HttpPost("/profile")]
        public async Task<IActionResult> Edit([FromForm] ProfileDetailsViewModel editMyProfileModel)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await GetCurrentUserAsync();

            if (user is null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                return View(editMyProfileModel);
            }

            user.FirstName = editMyProfileModel.FirstName;
            user.LastName = editMyProfileModel.LastName;

            if (!string.IsNullOrWhiteSpace(editMyProfileModel.Password) && editMyProfileModel.Password != "******")
            {
                user.Password = editMyProfileModel.Password;
            }

            await _userRepository.UpdateAsync(user);

            TempData["SuccessMessage"] = "Profiliniz başarıyla güncellendi.";

            return RedirectToAction(nameof(Details));
        }
        [HttpGet("/request-seller")]
        public async Task<IActionResult> RequestSeller()
        {
            var userId = GetUserId();

            if (userId is null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _userRepository.GetByIdAsync(userId.Value);

            // Eğer kullanıcı zaten Seller veya Admin rolündeyse, talep gönderemez.
            if (user.RoleId != 1) // 1: Buyer rolü
            {
                return RedirectToAction(nameof(Details));
            }

            return View();
        }
        [HttpPost("/request-seller")]
        public async Task<IActionResult> RequestSeller(SellerRequestViewModel model)
        {
            var userId = GetUserId();

            if (userId is null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _userRepository.GetByIdAsync(userId.Value);

            // Kullanıcı zaten Seller veya Admin rolünde olamaz.
            if (user.RoleId != 1) // 1: Buyer rolü
            {
                return RedirectToAction(nameof(Details));
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Satıcı olma talebini kaydet
            // Burada örneğin SellerRequest tablosuna kaydedebilirsiniz.
            var sellerRequest = new SellerRequestEntity
            {
                UserId = userId.Value,
                RequestMessage = model.RequestMessage,
                CreatedAt = DateTime.UtcNow,
                IsApproved = false // Admin tarafından onaylanacak
            };

            await _sellerRequestRepository.AddAsync(sellerRequest); // SellerRequestRepository veritabanı işlemleri

            TempData["SuccessMessage"] = "Satıcı olma talebiniz başarıyla gönderildi.";

            return RedirectToAction(nameof(Details));
        }


        [HttpGet("/my-orders")]
        public async Task<IActionResult> MyOrders()
        {
            var userId = GetUserId();

            if (userId is null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var orders = await _orderRepository.GetAllAsync();
            var orderViewModels = orders.Where(o => o.UserId == userId.Value)
                .Select(o => new OrderViewModel
                {
                    OrderCode = o.OrderCode,
                    Address = o.Address,
                    CreatedAt = o.CreatedAt,
                    TotalPrice = o.OrderItems.Sum(oi => oi.UnitPrice * oi.Quantity),
                    TotalProducts = o.OrderItems.Count,
                    TotalQuantity = o.OrderItems.Sum(oi => oi.Quantity),
                })
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

            return View(orderViewModels);
        }

        [HttpGet("/my-products")]
        public async Task<IActionResult> MyProducts()
        {
            var userId = GetUserId();

            if (userId is null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!await IsUserSellerAsync())
            {
                return RedirectToAction("Index", "Home");
            }

            var products = await _productRepository.GetAllAsync();
            var productViewModels = products.Where(p => p.SellerId == userId.Value)
                .Select(p => new MyProductsViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Description = p.Description,
                    Stock = p.StockAmount,
                    HasDiscount = p.DiscountId != null,
                    CreatedAt = p.CreatedAt,
                })
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            return View(productViewModels);
        }
    }

}