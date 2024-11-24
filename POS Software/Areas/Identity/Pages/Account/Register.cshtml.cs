// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using POS.Models;
using POS.Utility;

namespace POS_Software.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

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
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            public string Role { get; set; }

            [Required]
            public string Name { get; set; }
            [Required]
            public string StreetAddress { get; set; }
            [Required]
            public string City { get; set; }
            [Required]
            public string Division { get; set; }
            [Required]
            public string PostalCode { get; set; }
            [Required]
            public string PhoneNumber { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string returnUrl = null, string role = null)
        {
            if (string.IsNullOrEmpty(role))
            {
                // If no role is provided, render the login partial view
                return new PartialViewResult
                {
                    ViewName = "_LoginPartial",
                    ViewData = ViewData
                };
            }

            // Create roles if they don't exist
            if (!_roleManager.RoleExistsAsync(SD.Role_Admin).GetAwaiter().GetResult())
            {
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Manager));
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Cashier));
            }

            // Ensure the provided role is valid
            if (role != SD.Role_Admin && role != SD.Role_Manager && role != SD.Role_Cashier)
            {
                // If the role is invalid, render the login partial view
                return new PartialViewResult
                {
                    ViewName = "_LoginPartial",
                    ViewData = ViewData
                };
            }

            // Set the role in ViewData to use in the registration form
            ViewData["Role"] = role;
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null, string role = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = CreateUser();

                // Set user properties
                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                user.Name = Input.Name;
                user.StreetAddress = Input.StreetAddress;
                user.City = Input.City;
                user.Division = Input.Division;
                user.PostalCode = Input.PostalCode;
                user.PhoneNumber = Input.PhoneNumber;

                // Create the user in the database
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    // Assign role if specified
                    if (!string.IsNullOrEmpty(role))
                    {
                        // Validate if the role exists
                        var roleExists = await _roleManager.RoleExistsAsync(role);
                        if (roleExists)
                        {
                            var roleResult = await _userManager.AddToRoleAsync(user, role);
                            if (!roleResult.Succeeded)
                            {
                                _logger.LogError("Failed to assign role {Role} to user {User}: {Errors}", role, user.UserName, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                                ModelState.AddModelError(string.Empty, "Failed to assign the specified role.");
                                return Page();
                            }
                            _logger.LogInformation("Successfully assigned role {Role} to user {User}.", role, user.UserName);
                        }
                        else
                        {
                            _logger.LogWarning("Role {Role} does not exist.", role);
                            ModelState.AddModelError(string.Empty, "Specified role does not exist.");
                            return Page();
                        }
                    }

                    // Email confirmation logic
                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    // Log the user registration and the confirmation email
                    _logger.LogInformation("User {User} registered successfully and requires email confirmation.", user.UserName);

                    TempData["success"] = "Account is registered successfully!";

                    // Instead of automatically signing in the user, we redirect them to the login page
                    return RedirectToPage("/Account/Login");  // Redirect to login page

                }

                // Log and handle creation errors
                foreach (var error in result.Errors)
                {
                    _logger.LogError("Error creating user {User}: {Error}", Input.Email, error.Description);
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Return with validation errors if ModelState is invalid
            return Page();
        }


        private async Task AssignRoleAsync(ApplicationUser user, string role)
        {
            var roleExists = await _roleManager.RoleExistsAsync(role);
            if (roleExists)
            {
                var roleResult = await _userManager.AddToRoleAsync(user, role);
                if (!roleResult.Succeeded)
                {
                    _logger.LogError("Failed to assign role {Role} to user {User}: {Errors}", role, user.UserName, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    ModelState.AddModelError(string.Empty, "Failed to assign the specified role.");
                    return;
                }
                _logger.LogInformation("Successfully assigned role {Role} to user {User}.", role, user.UserName);
            }
            else
            {
                _logger.LogWarning("Role {Role} does not exist.", role);
                ModelState.AddModelError(string.Empty, "Specified role does not exist.");
            }
        }


        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
