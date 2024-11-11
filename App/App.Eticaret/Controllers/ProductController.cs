using App.Data.Entities;
using App.Data.Infrastructure;
using App.Eticaret.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.Eticaret.Controllers
{
    [Route("/product")]
    public class ProductController : BaseController
    {
        private readonly IDataRepository<ProductEntity> _productRepository;
        private readonly IDataRepository<ProductImageEntity> _productImageRepository;
        private readonly IDataRepository<ProductCommentEntity> _productCommentRepository;

        public ProductController(
            IDataRepository<ProductEntity> productRepository,
            IDataRepository<ProductImageEntity> productImageRepository,
            IDataRepository<ProductCommentEntity> productCommentRepository)
        {
            _productRepository = productRepository;
            _productImageRepository = productImageRepository;
            _productCommentRepository = productCommentRepository;
        }

        [HttpGet("")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("")]
        public async Task<IActionResult> Create([FromForm] SaveProductViewModel newProductModel)
        {
            if (!ModelState.IsValid)
            {
                return View(newProductModel);
            }

            var productEntity = new ProductEntity
            {
                SellerId = 2, // TODO: User'ı al
                CategoryId = newProductModel.CategoryId,
                DiscountId = newProductModel.DiscountId,
                Name = newProductModel.Name,
                Price = newProductModel.Price,
                Description = newProductModel.Description,
                StockAmount = newProductModel.StockAmount,
                CreatedAt = DateTime.UtcNow
            };

            await _productRepository.AddAsync(productEntity);
            await SaveProductImages(productEntity.Id, newProductModel.Images);

            ViewBag.SuccessMessage = "Ürün başarıyla eklendi.";
            ModelState.Clear();

            return View();
        }

        private async Task SaveProductImages(int productId, IList<IFormFile> images)
        {
            foreach (var image in images)
            {
                var productImageEntity = new ProductImageEntity
                {
                    ProductId = productId,
                    Url = $"/uploads/{Guid.NewGuid()}{Path.GetExtension(image.FileName)}"
                };

                await _productImageRepository.AddAsync(productImageEntity);

                await using var fileStream = new FileStream($"wwwroot{productImageEntity.Url}", FileMode.Create);
                await image.CopyToAsync(fileStream);
            }
        }

        [HttpGet("{productId:int}/edit")]
        public async Task<IActionResult> Edit([FromRoute] int productId)
        {
            var productEntity = await _productRepository.GetByIdAsync(productId);
            if (productEntity is null)
            {
                return NotFound();
            }

            var viewModel = new SaveProductViewModel
            {
                CategoryId = productEntity.CategoryId,
                DiscountId = productEntity.DiscountId,
                Name = productEntity.Name,
                Price = productEntity.Price,
                Description = productEntity.Description,
                StockAmount = productEntity.StockAmount
            };

            return View(viewModel);
        }

        [HttpPost("{productId:int}/edit")]
        public async Task<IActionResult> Edit([FromRoute] int productId, [FromForm] SaveProductViewModel editProductModel)
        {
            if (!ModelState.IsValid)
            {
                return View(editProductModel);
            }

            var productEntity = await _productRepository.GetByIdAsync(productId);
            if (productEntity is null)
            {
                return NotFound();
            }

            productEntity.CategoryId = editProductModel.CategoryId;
            productEntity.DiscountId = editProductModel.DiscountId;
            productEntity.Name = editProductModel.Name;
            productEntity.Price = editProductModel.Price;
            productEntity.Description = editProductModel.Description;
            productEntity.StockAmount = editProductModel.StockAmount;

            await _productRepository.UpdateAsync(productEntity);

            ViewBag.SuccessMessage = "Ürün başarıyla güncellendi.";

            return View(editProductModel);
        }

        [HttpGet("{productId:int}/delete")]
        public async Task<IActionResult> Delete([FromRoute] int productId)
        {
            var productEntity = await _productRepository.GetByIdAsync(productId);
            if (productEntity is null)
            {
                return NotFound();
            }

            await _productRepository.DeleteAsync(productId);

            ViewBag.SuccessMessage = "Ürün başarıyla silindi.";

            return View();
        }

        [HttpPost("{productId:int}/comment")]
        public async Task<IActionResult> Comment([FromRoute] int productId, [FromForm] SaveProductCommentViewModel newProductCommentModel)
        {
            var userId = GetUserId();

            if (userId == null)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var productEntity = await _productRepository.GetByIdAsync(productId);
            if (productEntity == null)
            {
                return NotFound();
            }

            if (await _productCommentRepository.GetAllAsync() is var comments && comments.Any(x => x.ProductId == productId && x.UserId == userId))
            {
                return BadRequest();
            }

            var productCommentEntity = new ProductCommentEntity
            {
                ProductId = productId,
                UserId = userId.Value,
                Text = newProductCommentModel.Text,
                StarCount = newProductCommentModel.StarCount,
                CreatedAt = DateTime.UtcNow,
            };

            await _productCommentRepository.AddAsync(productCommentEntity);

            return Ok();
        }
    }

}