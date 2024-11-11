using App.Admin.Models.ViewModels;
using App.Data.Entities;
using App.Data.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace App.Admin.Controllers
{
    public class AuthController : Controller
    {
        private readonly IDataRepository<UserEntity> _userRepository;

        public AuthController(IDataRepository<UserEntity> userRepository)
        {
            _userRepository = userRepository;
        }

        [Route("/login")]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [Route("/login")]
        [HttpPost]
        public async Task<IActionResult> Login([FromForm] LoginViewModel loginModel)
        {
            var user = await _userRepository.GetAllAsync();
            // Giriş kontrolü burada yapılabilir
            return View();
        }

        [Route("/logout")]
        [HttpGet]
        public IActionResult Logout()
        {
            // logout kodları...
            return RedirectToAction(nameof(Login));
        }
    }
}