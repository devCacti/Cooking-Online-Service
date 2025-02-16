using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Cooking_Service.Models;
using Cooking_Service.CSFunctions;
using System.Web.Management;
using Antlr.Runtime.Tree;
using Cooking_Service.DAL;
using Cooking_Service.Logging;
using Microsoft.Ajax.Utilities;
using System.Web.Http.Results;

namespace Cooking_Service.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private CookingContext db = new CookingContext();
        private Functions _cl = new Functions();

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Isso não conta falhas de login em relação ao bloqueio de conta
            // Para permitir que falhas de senha acionem o bloqueio da conta, altere para shouldLockout: true
            var user = await UserManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "Tentativa de login inválida.");
                return View(model);
            }
            var result = await SignInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                    ModelState.AddModelError("", "Tentativa de login inválida e desconhecida.");
                    return View(model);
                default:
                    ModelState.AddModelError("", "Tentativa de login inválida. " + result);
                    return View(model);
            }
        }

        // GET: /Account/AmILoggedIn
        [AllowAnonymous]
        public ActionResult AmILoggedIn()
        {
            if (User.Identity.IsAuthenticated)
            {
                // Returns a json response with the user's username, saying, yes, you are logged in
                return Json(new
                {
                    success = true,
                    message = "Yes, ou are logged in.",
                    username = User.Identity.GetUserName()
                }, JsonRequestBehavior.AllowGet);
            }

            // Returns a json response saying that the user is not logged in
            return Json(new
            {
                success = false,
                message = "No, you are not logged in."
            }, JsonRequestBehavior.AllowGet);
        }

        // Custom login method
        // To be used in the app
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> AppLogin(LoginViewModel model)
        {
            // First check if the client version is correct before proceeding
            var iCVV = _cl.isClientVersionValid(Request);
            if (iCVV.Item1 == 404)
            {
                return HttpNotFound();
            }
            else if (iCVV.Item1 == 403)
            {
                return new HttpStatusCodeResult(403, iCVV.Item2);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await UserManager.FindByEmailAsync(model.Email);

                    if (user == null)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "User not found",
                            error = new
                            {
                                code = "6",
                                description = "User not found"
                            }
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        // When this line executes, a login attempt is made
                        // and if it succeeds, the user is logged in and is given a login cookie
                        var result = await SignInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, shouldLockout: false);

                        var userInfo = db.Users.FirstOrDefault(u => u.GUID == user.Id);

                        switch (result)
                        {
                            case SignInStatus.Success:
                                return Json(new
                                {
                                    success = true,
                                    message = "Login successful",
                                    info = new
                                    {
                                        name = userInfo.Name,
                                        surname = userInfo.Surname,
                                        username = user.UserName,
                                        email = user.Email,
                                        id = user.Id
                                    },
                                    error = new
                                    {
                                        code = "0",
                                        description = "Success"
                                    }
                                }, JsonRequestBehavior.AllowGet);
                            case SignInStatus.LockedOut:
                                return Json(new
                                {
                                    success = false,
                                    message = "User locked out",
                                    error = new
                                    {
                                        code = "1",
                                        description = "User locked out"
                                    }
                                }, JsonRequestBehavior.AllowGet);
                            case SignInStatus.RequiresVerification:
                                return Json(new
                                {
                                    success = false,
                                    message = "Requires verification",
                                    error = new
                                    {
                                        code = "2",
                                        description = "Requires verification"
                                    }
                                }, JsonRequestBehavior.AllowGet);
                            case SignInStatus.Failure:
                                return Json(new
                                {
                                    success = false,
                                    message = "Login failed",
                                    error = new
                                    {
                                        code = "3",
                                        description = "Login failed"
                                    }
                                }, JsonRequestBehavior.AllowGet);
                            default:
                                return HttpNotFound("Unhandled.");
                        }
                    }
                }
                catch (Exception e)
                {
                    return Json(new
                    {
                        success = false,
                        message = "User not found: " + e,
                        error = new
                        {
                            code = "6",
                            description = "User not found"
                        }
                    }, JsonRequestBehavior.AllowGet);
                }

            }
            return Json(new
            {
                success = false,
                message = "Model not valid, please try again later",
                error = new
                {
                    code = "5",
                    description = "Model not valid"
                }
            }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> AppRegister(RegisterViewModel model)
        {
            // First check if the client version is correct before proceeding
            var iCVV = _cl.isClientVersionValid(Request);
            if (iCVV.Item1 == 404)
            {
                return HttpNotFound();
            }
            else if (iCVV.Item1 == 403)
            {
                return new HttpStatusCodeResult(403, iCVV.Item2);
            }

            // Checks if the model is valid
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.UserName, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent: true, rememberBrowser: false);

                    var uInfo = new User
                    {
                        GUID = user.Id,
                        Name = model.Name,
                        Surname = model.Surname,
                        Type = TypeUser.User
                    };

                    db.Users.Add(uInfo);
                    db.SaveChanges();

                    return Json(new
                    {
                        success = true,
                        message = "User created",
                        info = new
                        {
                            name = model.Name,
                            surname = model.Surname,
                            username = model.UserName,
                            email = model.Email,
                            id = user.Id
                        },
                        error = new
                        {
                            code = "0",
                            description = "Success"
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
                AddErrors(result);

            }

            // If the model is not valid, it will return a json response with the error message
            return Json(new
            {
                success = false,
                message = "Model not valid, please try again later",
                error = new
                {
                    code = "5",
                    description = "Model not valid"
                }
            });
        }
        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Exija que o usuário efetue login via nome de usuário/senha ou login externo
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // O código a seguir protege de ataques de força bruta em relação aos códigos de dois fatores. 
            // Se um usuário inserir códigos incorretos para uma quantidade especificada de tempo, então a conta de usuário 
            // será bloqueado por um período especificado de tempo. 
            // Você pode configurar os ajustes de bloqueio da conta em IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Código inválido.");
                    return View(model);
            }
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
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            //User is preset to User - this is the default value, even if the http post contains a different value
            
            if (model.Name == null)
            {
                model.Name = "";
            }

            if (model.Surname == null)
            {
                model.Surname = "";
            }

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.UserName, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);

                    var uInfo = new User
                    {
                        GUID = user.Id,
                        Name = model.Name,
                        Surname = model.Surname,
                        Type = TypeUser.User
                    };

                    db.Users.Add(uInfo);
                    db.SaveChanges();

                    // Para obter mais informações sobre como habilitar a confirmação da conta e redefinição de senha, visite https://go.microsoft.com/fwlink/?LinkID=320771
                    // Enviar um email com este link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirme sua conta", "Confirme sua conta clicando <a href=\"" + callbackUrl + "\">aqui</a>");

                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            // Se chegamos até aqui, algo falhou, reexibir formulário
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Não revelar que o usuário não existe ou não está confirmado
                    return View("ForgotPasswordConfirmation");
                }

                // Para obter mais informações sobre como habilitar a confirmação da conta e redefinição de senha, visite https://go.microsoft.com/fwlink/?LinkID=320771
                // Enviar um email com este link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Confirme sua conta", "Confirme sua conta clicando <a href=\"" + callbackUrl + "\">aqui</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // Se chegamos até aqui, algo falhou, reexibir formulário
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Não revelar que o usuário não existe
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //Função que devolve as receitas em JSON segundo o email/id do utilizador
        [HttpGet]
        [Route("Account/AppGetRecipes")]
        public async Task<ActionResult> AppGetRecipes()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                var recipes = db.Recipes.Where(r => r.Author.GUID == user.Id).ToList();

                if (recipes.Count == 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "No recipes found in user: " + user.UserName,
                        error = new
                        {
                            code = "1",
                            description = "No recipes found"
                        }
                    }, JsonRequestBehavior.AllowGet);
                }

                var response = new
                {
                    success = true,
                    message = recipes,
                    error = new
                    {
                        code = "0",
                        description = "Success"
                    }
                };
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            { success = false, message = "User not found" },
                JsonRequestBehavior.AllowGet
            );
        }

        // Function that works for the app login and returns a json response with the username, if the login was
        // successful or an error message if the login failed
        //App Login

        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Solicitar um redirecionamento para o provedor de logon externo
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Gerar o token e enviá-lo
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Faça logon do usuário com este provedor de logon externo se o usuário já tiver um logon
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // Se o usuário não tiver uma conta, solicite que o usuário crie uma conta
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Obter as informações sobre o usuário do provedor de logon externo
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Auxiliares
        // Usado para proteção XSRF ao adicionar logons externos
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}