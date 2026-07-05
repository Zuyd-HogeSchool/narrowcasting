using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Narrowcasting.Models;

namespace Narrowcasting.Pages.Admin
{
    [AllowAnonymous]
    public class IndexModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IndexModel(SignInManager<ApplicationUser> signInManager) =>
            _signInManager = signInManager;

        public IActionResult OnGet()
        {
            if (_signInManager.IsSignedIn(User))
                return RedirectToPage("/Admin/Dashboard/Index");

            var returnUrl = Url.Page("/Admin/Dashboard/Index");
            return RedirectToPage("/Account/Login", new { area = "Identity", returnUrl });
        }
    }
}
