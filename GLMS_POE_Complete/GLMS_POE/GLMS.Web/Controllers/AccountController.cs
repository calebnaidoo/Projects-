using GLMS.Web.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GLMS.Web.Controllers
{
    
    /// Handles user authentication — Register, Login, Logout.
    /// Uses ASP.NET Core Identity for secure password hashing and session management.
    
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        //REGISTER 

        [HttpGet]
        public IActionResult Register()
        {
            // Redirect already-authenticated users to dashboard
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = new IdentityUser
            {
                UserName = vm.Email,
                Email = vm.Email,
                EmailConfirmed = true // skip email confirmation for academic demo
            };

            var result = await _userManager.CreateAsync(user, vm.Password);

            if (result.Succeeded)
            {
                // Automatically sign in after successful registration
                await _signInManager.SignInAsync(user, isPersistent: false);
                TempData["Success"] = $"Welcome to GLMS, {vm.FullName}! Your account has been created.";
                return RedirectToAction("Index", "Home");
            }

            // Surface Identity errors to the form (e.g. "Password too short")
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(vm);
        }

        // LOGIN 

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _signInManager.PasswordSignInAsync(
                vm.Email, vm.Password, vm.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                TempData["Success"] = "You have signed in successfully.";
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid email or password. Please try again.");
            return View(vm);
        }

        //LOGOUT

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}
