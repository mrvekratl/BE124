using App.Data.Entities;
using App.Data.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace App.Admin.Controllers
{
    [Route("/comment")]
    public class CommentController : Controller
    {
        private readonly IDataRepository<ProductCommentEntity> _commentRepository;

        public CommentController(IDataRepository<ProductCommentEntity> commentRepository)
        {
            _commentRepository = commentRepository;
        }

        [Route("")]
        [HttpGet]
        public async Task<IActionResult> List()
        {
            var comments = await _commentRepository.GetAllAsync();
            return View(comments);
        }

        [Route("{commentId:int}/approve")]
        [HttpGet]
        public async Task<IActionResult> Approve([FromRoute] int commentId)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);

            return RedirectToAction(nameof(List));
        }
    }
}