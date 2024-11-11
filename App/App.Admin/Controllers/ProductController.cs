using App.Data.Entities;
using App.Data.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace App.Admin.Controllers
{
    public class ProductController : Controller
    {
        private readonly IDataRepository<ProductEntity> _productRepository;

        public ProductController(IDataRepository<ProductEntity> productRepository)
        {
            _productRepository = productRepository;
        }

        [Route("/products/")]
        [HttpGet]
        public async Task<IActionResult> List()
        {
            var products = await _productRepository.GetAllAsync();
            return View(products);
        }

        [Route("/products/filter")]
        [HttpGet]
        public IActionResult Filter([FromQuery] object filterOptions)
        {
            // Here you could add logic to filter the products based on filterOptions
            var filteredProducts = new List<ProductEntity>(); // Simulating filtered products
            return Json(filteredProducts);
        }

        [Route("/products/{productId:int}/delete")]
        [HttpGet]
        public async Task<IActionResult> Delete([FromRoute] int productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            await _productRepository.DeleteAsync(productId);
            return RedirectToAction(nameof(List));
        }
    }
}