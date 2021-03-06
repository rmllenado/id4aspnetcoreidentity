// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer.Models;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer
{
    /// <summary>
    /// This sample controller implements a typical login/logout/provision workflow for local and external accounts.
    /// The login service encapsulates the interactions with the user data store. This data store is in-memory only and cannot be used for production!
    /// The interaction service provides a way for the UI to communicate with identityserver for validation and context retrieval
    /// </summary>
    [SecurityHeaders]
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IEventService _events;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IAuthenticationSchemeProvider schemeProvider,
            IEventService events)
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
            _interaction = interaction;
            _clientStore = clientStore;
            _schemeProvider = schemeProvider;
            _events = events;
        }

        /// <summary>
        /// Entry point into the login workflow
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl)
        {
            // build a model so we know what to show on the login page
            var vm = await BuildLoginViewModelAsync(returnUrl);

            if (vm.IsExternalLoginOnly)
            {
                // we only have one option for logging in and it's an external provider
                return RedirectToAction("Challenge", "External", new { provider = vm.ExternalLoginScheme, returnUrl });
            }

            return View(vm);
        }

        /// <summary>
        /// Handle postback from username/password login
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginInputModel model, string button)
        {
            // check if we are in the context of an authorization request
            var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);

            // the user clicked the "cancel" button
            if (button != "login")
            {
                if (context != null)
                {
                    // if the user cancels, send a result back into IdentityServer as if they 
                    // denied the consent (even if this client does not require consent).
                    // this will send back an access denied OIDC error response to the client.
                    await _interaction.GrantConsentAsync(context, ConsentResponse.Denied);

                    // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                    if (await _clientStore.IsPkceClientAsync(context.ClientId))
                    {
                        // if the client is PKCE then we assume it's native, so this change in how to
                        // return the response is for better UX for the end user.
                        return View("Redirect", new RedirectViewModel { RedirectUrl = model.ReturnUrl });
                    }

                    return Redirect(model.ReturnUrl);
                }
                else
                {
                    // since we don't have a valid context, then we just go back to the home page
                    return Redirect("~/");
                }
            }

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberLogin, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByNameAsync(model.Username);
                    await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id, user.UserName));

                    if (context != null)
                    {
                        if (await _clientStore.IsPkceClientAsync(context.ClientId))
                        {
                            // if the client is PKCE then we assume it's native, so this change in how to
                            // return the response is for better UX for the end user.
                            return View("Redirect", new RedirectViewModel { RedirectUrl = model.ReturnUrl });
                        }

                        // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                        return Redirect(model.ReturnUrl);
                    }

                    // request for a local page
                    if (Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    else if (string.IsNullOrEmpty(model.ReturnUrl))
                    {
                        return Redirect("~/");
                    }
                    else
                    {
                        // user might have clicked on a malicious link - should be logged
                        throw new Exception("invalid return URL");
                    }
                }
                else if (result.RequiresTwoFactor)
                {
                    var user = await _userManager.FindByNameAsync(model.Username);
                    var validProviders = await _userManager.GetValidTwoFactorProvidersAsync(user);
                    var emailProvider = validProviders.SingleOrDefault(p => p.Contains("email", StringComparison.InvariantCultureIgnoreCase));
                    if (emailProvider != null)
                    {
                        var token = await _userManager.GenerateTwoFactorTokenAsync(user, emailProvider);
                        System.IO.File.WriteAllText("2svToken.txt", token);
                        await HttpContext.SignInAsync(IdentityConstants.TwoFactorUserIdScheme, Store2FA(user.UserName, "Email"));

                        if (context != null || Url.IsLocalUrl(model.ReturnUrl))
                        {
                            return RedirectToAction("TwoFactor", new { returnUrl = model.ReturnUrl });
                        }
                        else
                        {
                            return RedirectToAction("TwoFactor", new { returnUrl = "~/" });
                        }
                    }

                    //var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
                    //if (context != null || Url.IsLocalUrl(model.ReturnUrl))
                    //{
                    //    return RedirectToAction("TwoFactor", new { returnUrl = model.ReturnUrl });
                    //}
                    //else
                    //{
                    //    return RedirectToAction("TwoFactor", new { returnUrl = "~/" });
                    //}
                }

                await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "invalid credentials"));
                ModelState.AddModelError(string.Empty, AccountOptions.InvalidCredentialsErrorMessage);
            }

            // something went wrong, show form with error
            var vm = await BuildLoginViewModelAsync(model);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> TwoFactor(string returnUrl)
        {
            //var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            //if (user == null)
            //{

            //}

            //var validProviders = await _userManager.GetValidTwoFactorProvidersAsync(user);
            //var emailProvider = validProviders.SingleOrDefault(p => p.Contains("email", StringComparison.InvariantCultureIgnoreCase));
            //if (emailProvider != null)
            //{
            //    var token = await _userManager.GenerateTwoFactorTokenAsync(user, emailProvider);
            //    System.IO.File.WriteAllText("2svToken.txt", token);
            //    await HttpContext.SignInAsync(IdentityConstants.TwoFactorUserIdScheme, Store2FA(user.Id, "Email"));
            //}

            return View(new TwoFactorViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TwoFactor(TwoFactorViewModel model)
        {
            var result = await HttpContext.AuthenticateAsync(IdentityConstants.TwoFactorUserIdScheme);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Your login request has expired. Please start over.");
                return View();
            }

            if (ModelState.IsValid)
            {
                //var user = await _userManager.FindByIdAsync(result.Principal.FindFirstValue("sub"));
                var user = await _userManager.FindByNameAsync(result.Principal.FindFirstValue("sub"));

                if (user != null)
                {
                    var provider = result.Principal.FindFirstValue("amr");

                    //var signInResult = await _signInManager.TwoFactorSignInAsync(provider, model.Token, isPersistent: false, rememberClient: false);

                    //if (signInResult.Succeeded)
                    //{
                    //    //await HttpContext.SignOutAsync(IdentityConstants.TwoFactorUserIdScheme);
                    //    return LocalRedirect(model.ReturnUrl);
                    //}

                    //if (signInResult.IsLockedOut)
                    //{
                    //    ModelState.AddModelError("", "Account is locked.");
                    //}
                    //else
                    //{
                    //    ModelState.AddModelError("", "Invalid token");
                    //}

                    if (await _userManager.IsLockedOutAsync(user))
                    {
                        ModelState.AddModelError("", "Account is locked.");
                        return View();
                    }

                    if (await _userManager.VerifyTwoFactorTokenAsync(user, provider, model.Token))
                    {
                        //await HttpContext.SignOutAsync(IdentityConstants.TwoFactorUserIdScheme);
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        //await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme);
                        return LocalRedirect(model.ReturnUrl);
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invalid token");
                    }
                }
            }
            return View();
        }

        /// <summary>
        /// Show logout page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {
            // build a model so the logout page knows what to display
            var vm = await BuildLogoutViewModelAsync(logoutId);

            if (vm.ShowLogoutPrompt == false)
            {
                // if the request for logout was properly authenticated from IdentityServer, then
                // we don't need to show the prompt and can just log the user out directly.
                return await Logout(vm);
            }

            return View(vm);
        }

        /// <summary>
        /// Handle logout page postback
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(LogoutInputModel model)
        {
            // build a model so the logged out page knows what to display
            var vm = await BuildLoggedOutViewModelAsync(model.LogoutId);

            if (User?.Identity.IsAuthenticated == true)
            {
                // delete local authentication cookie
                await _signInManager.SignOutAsync();

                // raise the logout event
                await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
            }

            // check if we need to trigger sign-out at an upstream identity provider
            if (vm.TriggerExternalSignout)
            {
                // build a return URL so the upstream provider will redirect back
                // to us after the user has logged out. this allows us to then
                // complete our single sign-out processing.
                string url = Url.Action("Logout", new { logoutId = vm.LogoutId });

                // this triggers a redirect to the external provider for sign-out
                return SignOut(new AuthenticationProperties { RedirectUri = url }, vm.ExternalAuthenticationScheme);
            }

            return View("LoggedOut", vm);
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var resetUrl = Url.Action("ResetPassword", "Account", new { token = token, email = user.Email }, Request.Scheme);
                    System.IO.File.WriteAllText("resetlink.txt", resetUrl);
                }
                else
                {
                    // email user and inform them that they do not have an account
                }
                return View("Success", BuildSuccessViewModel("Please check your email to reset your password", autoRedirect: false));
            }
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            return View(new ResetPasswordViewModel { Token = token, Email = email }); ;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                        return View();
                    }
                    return View("Success", BuildSuccessViewModel("Password changed.", autoRedirect: true));
                }
                ModelState.AddModelError("", "Invalid Request");
            }
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.UserName) ?? await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    // Send an email to user
                    // You're already registered. If you forgot your password, please use the forgot password using your registered email to reset your password to regain access.
                    // You can also call Customer Service for assistance.
                }

                if (user == null)
                {
                    user = new ApplicationUser { UserName = model.UserName, Email = model.Email };

                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var confirmEmailUrl = Url.Action("ConfirmEmail", "Account", new { token = token, email = user.Email }, Request.Scheme);
                        System.IO.File.WriteAllText("confirmEmailLink.txt", confirmEmailUrl);

                    }
                    else if (result.Errors.Any(e => e.Description.Contains("password", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        foreach (var e in result.Errors)
                        {
                            ModelState.AddModelError("", e.Description);
                        }
                        return View();
                    }
                }

                // We always return success to prevent account enumeration attacks
                return View("Success", BuildSuccessViewModel("Registered.", autoRedirect: false));
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);

                if (result.Succeeded)
                {
                    return View("Success", BuildSuccessViewModel("Thank you for confirming your email.", autoRedirect: false));
                }
            }

            return View("Error");
        }

        /*****************************************/
        /* helper APIs for the AccountController */
        /*****************************************/
        private SuccessViewModel BuildSuccessViewModel(string message, string redirectUrl = "Login", bool autoRedirect = false)
        {
            var vm = new SuccessViewModel
            {
                Message = message,
                RedirectUri = redirectUrl,
                AutoRedirect = autoRedirect
            };

            return vm;
        }

        private async Task<LoginViewModel> BuildLoginViewModelAsync(string returnUrl)
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (context?.IdP != null)
            {
                // this is meant to short circuit the UI and only trigger the one external IdP
                return new LoginViewModel
                {
                    EnableLocalLogin = false,
                    ReturnUrl = returnUrl,
                    Username = context?.LoginHint,
                    ExternalProviders = new ExternalProvider[] { new ExternalProvider { AuthenticationScheme = context.IdP } }
                };
            }

            var schemes = await _schemeProvider.GetAllSchemesAsync();

            var providers = schemes
                .Where(x => x.DisplayName != null ||
                            (x.Name.Equals(AccountOptions.WindowsAuthenticationSchemeName, StringComparison.OrdinalIgnoreCase))
                )
                .Select(x => new ExternalProvider
                {
                    DisplayName = x.DisplayName,
                    AuthenticationScheme = x.Name
                }).ToList();

            var allowLocal = true;
            if (context?.ClientId != null)
            {
                var client = await _clientStore.FindEnabledClientByIdAsync(context.ClientId);
                if (client != null)
                {
                    allowLocal = client.EnableLocalLogin;

                    if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                    {
                        providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
                    }
                }
            }

            return new LoginViewModel
            {
                AllowRememberLogin = AccountOptions.AllowRememberLogin,
                EnableLocalLogin = allowLocal && AccountOptions.AllowLocalLogin,
                ReturnUrl = returnUrl,
                Username = context?.LoginHint,
                ExternalProviders = providers.ToArray()
            };
        }

        private async Task<LoginViewModel> BuildLoginViewModelAsync(LoginInputModel model)
        {
            var vm = await BuildLoginViewModelAsync(model.ReturnUrl);
            vm.Username = model.Username;
            vm.RememberLogin = model.RememberLogin;
            return vm;
        }

        private async Task<LogoutViewModel> BuildLogoutViewModelAsync(string logoutId)
        {
            var vm = new LogoutViewModel { LogoutId = logoutId, ShowLogoutPrompt = AccountOptions.ShowLogoutPrompt };

            if (User?.Identity.IsAuthenticated != true)
            {
                // if the user is not authenticated, then just show logged out page
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            var context = await _interaction.GetLogoutContextAsync(logoutId);
            if (context?.ShowSignoutPrompt == false)
            {
                // it's safe to automatically sign-out
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            // show the logout prompt. this prevents attacks where the user
            // is automatically signed out by another malicious web page.
            return vm;
        }

        private async Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(string logoutId)
        {
            // get context information (client name, post logout redirect URI and iframe for federated signout)
            var logout = await _interaction.GetLogoutContextAsync(logoutId);

            var vm = new LoggedOutViewModel
            {
                AutomaticRedirectAfterSignOut = AccountOptions.AutomaticRedirectAfterSignOut,
                PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
                ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout?.ClientName,
                SignOutIframeUrl = logout?.SignOutIFrameUrl,
                LogoutId = logoutId
            };

            if (User?.Identity.IsAuthenticated == true)
            {
                var idp = User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
                if (idp != null && idp != IdentityServer4.IdentityServerConstants.LocalIdentityProvider)
                {
                    var providerSupportsSignout = await HttpContext.GetSchemeSupportsSignOutAsync(idp);
                    if (providerSupportsSignout)
                    {
                        if (vm.LogoutId == null)
                        {
                            // if there's no current logout context, we need to create one
                            // this captures necessary info from the current logged in user
                            // before we signout and redirect away to the external IdP for signout
                            vm.LogoutId = await _interaction.CreateLogoutContextAsync();
                        }

                        vm.ExternalAuthenticationScheme = idp;
                    }
                }
            }

            return vm;
        }

        private ClaimsPrincipal Store2FA(string email, string v)
        {
            var identity = new ClaimsIdentity(new Claim[]
            {
                new Claim("sub", email)
                ,new Claim("amr", v)
            }, IdentityConstants.TwoFactorUserIdScheme);
            return new ClaimsPrincipal(identity);
        }
    }
}
