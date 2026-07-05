// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Narrowcasting.Models;

namespace Narrowcasting.Areas.Identity.Pages.Account;

public class LoginWithRecoveryCodeModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<LoginWithRecoveryCodeModel> _logger;

    public LoginWithRecoveryCodeModel(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ILogger<LoginWithRecoveryCodeModel> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = default!;

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public string? ReturnUrl { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InputModel
    {
        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Herstelcode")]
        public string RecoveryCode { get; set; } = default!;
    }

    public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
    {
        // Ensure the user has gone through the username & password screen first
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            throw new InvalidOperationException($"Het lukt niet om de gebruiker voor tweefactorauthenticatie te laden.");
        }

        ReturnUrl = returnUrl;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            throw new InvalidOperationException($"Het lukt niet om de gebruiker voor tweefactorauthenticatie te laden.");
        }

        var recoveryCode = Input.RecoveryCode.Replace(" ", string.Empty);

        var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

        var userId = await _userManager.GetUserIdAsync(user);

        if (result.Succeeded)
        {
            _logger.LogInformation("Gebruiker met ID '{UserId}' ingelogd met een herstelcode.", user.Id);
            return LocalRedirect(returnUrl ?? Url.Content("~/"));
        }
        if (result.IsLockedOut)
        {
            _logger.LogWarning("Gebruikersaccount geblokkeerd.");
            return RedirectToPage("./Lockout");
        }
        else
        {
            _logger.LogWarning("Ongeldige herstelcode ingevoerd voor gebruiker met ID '{UserId}' ", user.Id);
            ModelState.AddModelError(string.Empty, "Ongeldige herstelcode ingevoerd.");
            return Page();
        }
    }
}
