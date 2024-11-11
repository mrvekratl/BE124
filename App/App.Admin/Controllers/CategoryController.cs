using App.Admin.Models.ViewModels;
using App.Data.Entities;
using App.Data.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.Admin.Controllers
{
    [Route("/categories")]
    public class CategoryController : Controller
    {
        private readonly IDataRepository<CategoryEntity> _categoryRepository;

        public CategoryController(IDataRepository<CategoryEntity> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var categories = await _categoryRepository.GetAllAsync();
            var categoryListViewModel = categories.Select(c => new CategoryListViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Color = c.Color,
                IconCssClass = c.IconCssClass
            }).ToList();

            return View(categoryListViewModel);
        }

        [Route("create")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Route("create")]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] SaveCategoryViewModel newCategoryModel)
        {
            if (!ModelState.IsValid)
            {
                return View(newCategoryModel);
            }

            var categoryEntity = new CategoryEntity
            {
                Name = newCategoryModel.Name,
                Color = newCategoryModel.Color,
                IconCssClass = string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            await _categoryRepository.AddAsync(categoryEntity);

            ViewBag.SuccessMessage = "Kategori başarıyla oluşturuldu.";
            ModelState.Clear();

            return View();
        }

        [Route("{categoryId:int}/edit")]
        [HttpGet]
        public async Task<IActionResult> Edit([FromRoute] int categoryId)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null)
            {
                return NotFound();
            }

            var editCategoryModel = new SaveCategoryViewModel
            {
                Name = category.Name,
                Color = category.Color,
                IconCssClass = category.IconCssClass
            };

            return View(editCategoryModel);
        }

        [Route("{categoryId:int}/edit")]
        [HttpPost]
        public async Task<IActionResult> Edit([FromRoute] int categoryId, [FromForm] SaveCategoryViewModel editCategoryModel)
        {
            if (!ModelState.IsValid)
            {
                return View(editCategoryModel);
            }

            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null)
            {
                return NotFound();
            }

            category.Name = editCategoryModel.Name;
            category.Color = editCategoryModel.Color;

            await _categoryRepository.UpdateAsync(category);

            ViewBag.SuccessMessage = "Kategori başarıyla güncellendi.";
            ModelState.Clear();

            return View();
        }

        [Route("{categoryId:int}/delete")]
        [HttpGet]
        public async Task<IActionResult> Delete([FromRoute] int categoryId)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null)
            {
                return NotFound();
            }

            await _categoryRepository.DeleteAsync(categoryId);

            return RedirectToAction(nameof(List));
        }
    }
}