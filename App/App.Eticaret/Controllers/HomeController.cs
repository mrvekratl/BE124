using App.Data.Entities;
using App.Data.Infrastructure;
using App.Eticaret.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.Eticaret.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IDataRepository<ContactFormEntity> _contactFormRepository;
        private readonly IDataRepository<ProductEntity> _productRepository;
        private readonly IDataRepository<ProductCommentEntity> _commentRepository;

        public HomeController(
            IDataRepository<ContactFormEntity> contactFormRepository,
            IDataRepository<ProductEntity> productRepository,
            IDataRepository<ProductCommentEntity> commentRepository)
        {
            _contactFormRepository = contactFormRepository ?? throw new ArgumentNullException(nameof(contactFormRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _commentRepository = commentRepository ?? throw new ArgumentNullException(nameof(commentRepository));
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("/about-us")]
        public IActionResult AboutUs()
        {
            return View();
        }

        [HttpGet("/contact")]
        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost("/contact")]
        public async Task<IActionResult> Contact([FromForm] NewContactFormMessageViewModel newContactMessage)
        {
            if (!ModelState.IsValid)
            {
                return View(newContactMessage);
            }

            var contactMessageEntity = new ContactFormEntity
            {
                Name = newContactMessage.Name,
                Email = newContactMessage.Email,
                Message = newContactMessage.Message,
                CreatedAt = DateTime.UtcNow,
                SeenAt = null
            };

            // Add contact message to the repository
            await _contactFormRepository.AddAsync(contactMessageEntity);

            ViewBag.SuccessMessage = "Your message has been sent successfully.";

            return View();
        }

        [HttpGet("/product/list")]
        public async Task<IActionResult> Listing()
        {
            // TODO: add paging support
            var products = (await _productRepository.GetAllAsync())
                .Where(p => p.Enabled)
                .Select(p => new ProductListingViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    CategoryName = p.Category.Name,
                    DiscountPercentage = p.Discount == null ? null : p.Discount.DiscountRate,
                    ImageUrl = p.Images.Count != 0 ? p.Images.First().Url : null
                })
                .ToList();

            return View(products);
        }

        [HttpGet("/product/{productId:int}/details")]
        public async Task<IActionResult> ProductDetail([FromRoute] int productId)
        {
            var product = (await _productRepository.GetAllAsync())
                .Where(p => p.Enabled && p.Id == productId)
                .Select(p => new HomeProductDetailViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    DiscountRate = p.Discount == null ? null : p.Discount.DiscountRate,
                    Description = p.Description,
                    StockAmount = p.StockAmount,
                    SellerName = p.Seller.FirstName + " " + p.Seller.LastName,
                    CategoryName = p.Category.Name,
                    CategoryId = p.CategoryId,
                    ImageUrls = p.Images.Select(i => i.Url).ToArray(),
                    Reviews = p.Comments.Where(c => c.IsConfirmed) // show only confirmed comments
                        .Select(c => new ProductReviewViewModel
                        {
                            Id = c.Id,
                            Text = c.Text,
                            StarCount = c.StarCount,
                            UserName = c.User.FirstName + " " + c.User.LastName
                        }).ToArray()
                })
                .FirstOrDefault();

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
    }

}