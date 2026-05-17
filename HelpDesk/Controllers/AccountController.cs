using HelpDesk.Models;
using HelpDesk.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // GET /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            // Si déjà connecté, rediriger vers le bon dashboard
            if (User.Identity!.IsAuthenticated)
                return RedirectToDashboard();

            return View();
        }

        // POST /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                isPersistent: false,
                lockoutOnFailure: false);

            if (result.Succeeded)
                return RedirectToDashboard();

            ModelState.AddModelError(string.Empty, "Email ou mot de passe incorrect.");
            return View(model);
        }

        // POST /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        // Redirection selon le rôle de l'utilisateur connecté
        private IActionResult RedirectToDashboard()
        {
            if (User.IsInRole("Administrateur"))
                return RedirectToAction("Dashboard", "Admin");
            if (User.IsInRole("Technicien"))
                return RedirectToAction("Dashboard", "Technician");

            return RedirectToAction("Index", "Ticket");
        }
    }
}