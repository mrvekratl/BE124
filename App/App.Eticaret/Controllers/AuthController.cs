using App.Data.Entities;
using App.Data.Infrastructure;
using App.Eticaret.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace App.Eticaret.Controllers
{
    

    public class AuthController : Controller
    {
        private readonly IDataRepository<UserEntity> _userRepository;

        public AuthController(IDataRepository<UserEntity> userRepository)
        {
            _userRepository = userRepository;
        }

        // Kayıt sayfası
        [Route("/register")]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Kayıt işlemi
        [Route("/register")]
        [HttpPost]
        public async Task<IActionResult> Register([FromForm] RegisterUserViewModel newUser)
        {
            if (!ModelState.IsValid)
            {
                return View(newUser);
            }

            // Kullanıcının rolünü kontrol et
            int userRoleId = 3; // Default Buyer olarak
            if (newUser.Role == "Seller") // Eğer kullanıcı Seller rolü istiyorsa
            {
                userRoleId = 2; // Seller rolü için RoleId 2
            }
            else if (newUser.Role == "Admin") // Eğer kullanıcı Admin rolü istiyorsa
            {
                userRoleId = 1; // Admin rolü için RoleId 1
            }

            // Kullanıcıyı oluşturuyoruz
            var user = new UserEntity
            {
                FirstName = newUser.FirstName,
                LastName = newUser.LastName,
                Email = newUser.Email,
                Password = newUser.Password, // Şifreyi burada hashleyebilirsiniz
                RoleId = userRoleId, // Yeni kullanıcı rolünü ayarlıyoruz
                CreatedAt = DateTime.UtcNow,
            };
            if(user.RoleId == 2)
            {
                user.Enabled = false;
            }

            // Kullanıcıyı repository üzerinden ekliyoruz
            await _userRepository.AddAsync(user);

            ViewBag.SuccessMessage = "Kayıt işlemi başarılı. Giriş yapabilirsiniz.";
            ModelState.Clear();

            return View();
        }


        // Giriş sayfası
        [Route("/login")]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Giriş işlemi
        [Route("/login")]
        [HttpPost]
        public async Task<IActionResult> Login([FromForm] LoginViewModel loginModel)
        {
            if (!ModelState.IsValid)
            {
                return View(loginModel);
            }

            // Kullanıcıyı repository üzerinden alıyoruz
            var users = await _userRepository.GetAllAsync();
            var user = users.FirstOrDefault(u => u.Email == loginModel.Email && u.Password == loginModel.Password);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Kullanıcı adı veya şifre hatalı.");
                return View(loginModel);
            }

            await LogInAsync(user);

            return RedirectToAction("Index", "Home");
        }

        // Kullanıcıyı oturum açtırma
        private async Task LogInAsync(UserEntity user)
        {
            if (user == null || !user.Enabled)
            {
                return;
            }

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.FirstName + " " + user.LastName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.RoleId.ToString()), // Kullanıcının rolü
            new Claim("userId", user.Id.ToString()) // Kullanıcının Id'si
        };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // Kullanıcıyı oturum açtırıyoruz
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            await Task.CompletedTask;
        }

        // Çıkış işlemi
        [Route("/logout")]
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            // Manual olarak cookie'leri temizle
            Response.Cookies.Delete("userId");
            Response.Cookies.Delete("mail");
            Response.Cookies.Delete("name");
            Response.Cookies.Delete("surname");
            Response.Cookies.Delete("role");

            return RedirectToAction(nameof(Login));
        }

        // Şifre unuttum sayfası
        [Route("/forgot-password")]
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // Şifre sıfırlama işlemi
        [Route("/forgot-password")]
        [HttpPost]
        public async Task<IActionResult> ForgotPassword([FromForm] ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Kullanıcıyı repository üzerinden alıyoruz
            var users = await _userRepository.GetAllAsync();
            var foundUser = users.FirstOrDefault(u => u.Email == model.Email);

            if (foundUser == null)
            {
                ModelState.AddModelError(string.Empty, "Kullanıcı bulunamadı.");
                return View(model);
            }

            // Şifre sıfırlama işlemi yapılacak (Örn: E-posta gönderme)
            await SendResetPasswordEmailAsync(foundUser);

            ViewBag.SuccessMessage = "Şifre sıfırlama maili gönderildi. Lütfen e-posta adresinizi kontrol edin.";
            ModelState.Clear();

            return View();
        }

        // Şifre sıfırlama e-postası gönderme (örnek)
        private async Task SendResetPasswordEmailAsync(UserEntity user)
        {
            // Burada şifre sıfırlama kodu ve e-posta gönderimi yapılacak...
            await Task.CompletedTask;
        }

        // Şifre yenileme sayfası
        [Route("/renew-password/{verificationCode}")]
        [HttpGet]
        public async Task<IActionResult> RenewPassword([FromRoute] string verificationCode)
        {
            // Şifre yenileme sayfasına yönlendirme yapılacak
            return View();
        }

        // Şifre yenileme işlemi
        [Route("/renew-password")]
        [HttpPost]
        public async Task<IActionResult> RenewPassword([FromForm] ForgotPasswordViewModel model)
        {
            // Şifre değiştirme işlemi yapılacak
            return View();
        }

        // Yetkilendirilmiş kullanıcı için admin paneli
        [Authorize(Roles = "Admin")]
        [Route("/admin")]
        public IActionResult AdminPanel()
        {
            return View();
        }
    }

}