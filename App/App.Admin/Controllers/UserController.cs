using App.Admin.Models.ViewModels;
using App.Data.Entities;
using App.Data.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.Admin.Controllers
{
    public class UserController : Controller
    {
        private readonly IDataRepository<UserEntity> _userRepository;
        private readonly IDataRepository<RoleEntity> _roleRepository;

        public UserController(IDataRepository<UserEntity> userRepository, IDataRepository<RoleEntity> roleRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
        }

        [Route("/users")]
        [HttpGet]
        public async Task<IActionResult> List()
        {
            var users = await _userRepository.GetAllAsync();
            var userListViewModel = users
                .Where(u => u.RoleId != 1)
                .Select(u => new UserListItemViewModel
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Role = u.Role != null ? u.Role.Name : "Buyer", // Null kontrolü eklendi
                    Enabled = u.Enabled,
                    HasSellerRequest = u.HasSellerRequest
                })
                .ToList();

            return View(userListViewModel);
        }

        [Route("/users/{id:int}/approve")]
        [HttpGet]
        public async Task<IActionResult> ApproveSellerRequest([FromRoute] int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (!user.HasSellerRequest)
            {
                return BadRequest();
            }

            user.HasSellerRequest = false;

            var sellerRole = await _roleRepository.GetAllAsync();
            var sellerRoleEntity = sellerRole.FirstOrDefault(r => r.Name == "Seller");
            if (sellerRoleEntity != null)
            {
                user.RoleId = sellerRoleEntity.Id;
            }

            await _userRepository.UpdateAsync(user);
            return RedirectToAction(nameof(List));
        }

        [Route("/users/{id:int}/enable")]
        public async Task<IActionResult> Enable([FromRoute] int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.Enabled = true;
            await _userRepository.UpdateAsync(user);
            return RedirectToAction(nameof(List));
        }


        [Route("/users/{id:int}/disable")]
        public async Task<IActionResult> Disable([FromRoute] int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.Enabled = false;
            await _userRepository.UpdateAsync(user);
            return RedirectToAction(nameof(List));
        }
    }
}