using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Web;
using System.Web.Mvc;
using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
using Raven.Contrib.AspNet.Auth;
using Raven.Contrib.AspNet.Auth.Interfaces;
using Raven.Contrib.AspNet.Auth.Providers;
using Raven.Contrib.AspNet.Demo.Extensions;
using Raven.Contrib.AspNet.Demo.Models;

namespace Raven.Contrib.AspNet.Demo.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess
        }

        private readonly IDictionary<string, IExternalAuthProvider> _providers;

        public AccountController()
        {
            var google = new GoogleAuthenticator(new ClaimsRequest { Nickname = DemandLevel.Request });

            _providers = new Dictionary<string, IExternalAuthProvider>
            {
                { google.ProviderName.ToLowerInvariant(), google },
                // Add Facebook, Twitter, etc.
            };
        }

        //
        // GET: /Account/Login

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            return View(new LoginModel
            {
                ReturnUrl = returnUrl
            });
        }

        //
        // POST: /Account/Login

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Authenticator.Login(model.UserName, model.Password, persistent: model.RememberMe);

                    return RedirectToLocal(model.ReturnUrl);
                }
            }
            catch (InvalidCredentialException)
            {
                
            }

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "The user name or password provided is incorrect.");

            return View(model);
        }

        //
        // POST: /Account/LogOff

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            Authenticator.Logout();

            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/Register

        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                try
                {
                    Authenticator.CreateAccount(model.UserName, model.Password);
                    Authenticator.Login(model.UserName, model.Password);

                    return RedirectToAction("Index", "Home");
                }
                catch (DuplicateUserNameException)
                {
                    ModelState.AddModelError("", "User name already exists. Please enter a different user name.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/Disassociate

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Disassociate(ExternalLoginModel model)
        {
            var userName    = Authenticator.Current;
            var identifiers = Authenticator.GetIdentifiers(userName);

            bool isLocal = Authenticator.IsLocalAccount(userName);
            bool isOwner = identifiers.Any(i => i.Key == model.ProviderName && i.Value == model.UserIdentifier);

            // Only disassociate the account if the currently logged in user
            // is the owner and it is not the user's last login credential.
            if (isOwner && (isLocal || identifiers.Count > 1))
                Authenticator.RemoveIdentifier(userName, model.ProviderName);

            return RedirectToAction("Manage");
        }

        //
        // GET: /Account/Manage

        public ActionResult Manage(ManageMessageId? message)
        {
            var userName = Authenticator.Current;
            var model    = new ManageModel
            {
                UserName         = userName,
                HasLocalPassword = Authenticator.IsLocalAccount(userName)
            };

            switch (message)
            {
                case ManageMessageId.ChangePasswordSuccess:
                    model.StatusMessage = "Your password has been changed.";

                    break;

                case ManageMessageId.SetPasswordSuccess:
                    model.StatusMessage = "Your password has been set.";

                    break;

                case ManageMessageId.RemoveLoginSuccess:
                    model.StatusMessage = "The external login was removed.";

                    break;
            }

            return View(model);
        }

        //
        // POST: /Account/Manage

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Manage(ManageModel model)
        {
            var userName = Authenticator.Current;

            if (Authenticator.IsLocalAccount(userName))
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        Authenticator.ChangePassword(userName, model.OldPassword, model.NewPassword);

                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    catch (Exception)
                    {
                        ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                    }
                }
            }
            else
            {
                // User does not have a local password.
                // Remove any validation errors caused by a missing OldPassword field.
                ModelState state = ModelState["OldPassword"];

                if (state != null)
                    state.Errors.Clear();

                if (ModelState.IsValid)
                {
                    Authenticator.SetPassword(userName, model.NewPassword);

                    return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/ExternalLogin

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            var authProvier = _providers[provider.ToLowerInvariant()];
            var action      = Url.Action("ExternalLoginCallback");
            var callback    = new Uri(Url.Public(action), UriKind.Absolute);

            Session["ReturnUrl"] = returnUrl;

            authProvier.RequestAuthentication(HttpContext, callback.AddQueryParameter("provider", provider));

            // Show not get here.
            return new EmptyResult();
        }

        //
        // GET: /Account/ExternalLoginCallback

        [AllowAnonymous]
        public ActionResult ExternalLoginCallback(string provider)
        {
            var authProvider = _providers[provider.ToLowerInvariant()];
            var authResult   = authProvider.VerifyAuthentication(HttpContext);

            if (authResult.Result != AuthResult.Status.Authenticated)
                return RedirectToAction("ExternalLoginFailure");

            try
            {
                Authenticator.Login(authResult.Information.Identifier);

                return RedirectToLocal((string) Session["ReturnUrl"]);
            }
            catch (Exception)
            {
                if (Authenticator.IsAuthenticated)
                {
                    // If the current user is logged in add the new identifier.
                    Authenticator.AddIdentifier(Authenticator.Current, authResult.Information.Identifier, "Google");

                    return RedirectToLocal((string) Session["ReturnUrl"]);
                }
                else
                {
                    var profile = authResult.Information.Profile;

                    // User is new, ask for their desired username.
                    return View("ExternalLoginConfirmation", new RegisterExternalLoginModel
                    {
                        UserName       = profile == null ? null : profile.Nickname,
                        UserIdentifier = authResult.Information.Identifier,
                        ProviderName   = authProvider.ProviderName,
                    });
                }
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLoginConfirmation(RegisterExternalLoginModel model)
        {
            if (Authenticator.IsAuthenticated)
            {
                return RedirectToAction("Manage");
            }
            else if (ModelState.IsValid)
            {
                Authenticator.CreateExternalAccount(model.UserName, model.UserIdentifier, model.ProviderName);
                Authenticator.Login(model.UserIdentifier);

                return RedirectToLocal((string) Session["ReturnUrl"]);
            }
            else
            {
                return View(model);
            }
        }

        //
        // GET: /Account/ExternalLoginFailure

        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult ExternalLoginsList(string returnUrl)
        {
            return PartialView("_ExternalLoginsListPartial", new ExternalLoginsListModel
            {
                ProviderNames = _providers.Keys,
                ReturnUrl     = returnUrl,
            });
        }

        [ChildActionOnly]
        public ActionResult RemoveExternalLogins()
        {
            var userName = Authenticator.Current;
            var logins   = Authenticator.GetIdentifiers(userName).Select(i => new ExternalLoginModel
            {
                ProviderName   = i.Key,
                UserIdentifier = i.Value,
            }).ToList();

            return PartialView("_RemoveExternalLoginsPartial", new RemoveExternalLoginModel
            {
                ExternalLogins   = logins,
                ShowRemoveButton = logins.Any() || Authenticator.IsLocalAccount(userName),
            });
        }
    }
}
