using System;
using System.Web;
using System.Web.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.Owin.Security;

using OAuthServer.Models;
using OAuthServer.Extensions;

using Models;
using OAuthServer.Models.Enumerations;

using BusinessLogic.Interfaces;

namespace OAuthServer.Controllers
{
    [Authorize]
    [RoutePrefix("account")]
    public class AccountController : Controller
    {
        private readonly IUserLogic userLogic;
        private IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;

        public AccountController(IUserLogic userLogic) => this.userLogic = userLogic ?? throw new ArgumentNullException(nameof(userLogic));

        [HttpGet]
        [Route]
        [Route("index")]
        public async Task<ActionResult> Index()
        {
            ViewData["auth-data"] = new AuthData(await this.AuthenticationManager.AuthenticateAsync("PCM"));
            User user = await this.userLogic.GetUserAsync(this.User.Identity.GetClaimValue<int>(ClaimTypes.NameIdentifier));

            return View(user);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("login")]
        public ActionResult LoginGET()
        {
            if (this.User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Applications");

            return View("login");
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        public async Task<ActionResult> LoginPOST()
        {
            bool isPersistent = !string.IsNullOrEmpty(Request.Form.Get("isPersistent"));

            if (!string.IsNullOrEmpty(Request.Form.Get("submit.login")))
                try
                {
                    User user = await this.userLogic.LoginAsync(Request.Form["login"], Request.Form["password"]);
                    if (user != null)
                    {
                        var identiy = new ClaimsIdentity(new[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()),
                            new Claim(ClaimTypes.Name, user.NameFirst),
                            new Claim(ClaimTypes.Surname, user.NameLast),
                            new Claim(ClaimTypes.HomePhone, user.Phone),
                            new Claim(ClaimTypes.Email, user.EMail),
                            new Claim(ClaimTypes.Role, user.ID == 62018 ? "SuperUser" : "User")
                        }, "PCM");
                        this.AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistent }, identiy);

                        if (!Request.QueryString.HasKeys())
                            return RedirectToAction("Index", "Applications");
                    }
                    else
                        ViewData["notification"] = ("NOTIFICATION.ACCESS-DENIED", "Wrong credentials or the DB is in an update mode.", Status.ERROR);
                }
                catch (Exception ex)
                {
                    ViewData["notification"] = ("NOTIFICATION.LOGIN-FAILED", ex.ToString(), Status.ERROR);
                }

            return View("login");
        }

        [HttpGet]
        [Route("logout")]
        public ActionResult Logout()
        {
            this.AuthenticationManager.SignOut("PCM");
            return View();
        }
    }
}