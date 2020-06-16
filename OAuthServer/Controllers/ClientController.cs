using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Microsoft.Owin.Security;

using Models;
using Models.Enumerations;

using OAuthServer.Extensions;
using OAuthServer.Content.Languages;
using OAuthServer.Models.Enumerations;

using BusinessLogic.Interfaces;

namespace OAuthServer.Controllers
{
    [Authorize]
    [RoutePrefix("client")]
    public class ClientController : Controller
    {
        private readonly IOAuth2Logic oauth2Logic;

        public ClientController(IOAuth2Logic oauth2Logic) => this.oauth2Logic = oauth2Logic ?? throw new ArgumentNullException(nameof(oauth2Logic));

        [HttpGet]
        [Route]
        [Route("index")]
        public async Task<ActionResult> Index()
        {
            // Clients registered by user
            int userId = this.User.Identity.GetClaimValue<int>(ClaimTypes.NameIdentifier);

            var ownClientsTask = this.oauth2Logic.FindClientsAsync(userId);
            var userScopesTask = this.oauth2Logic.GetAllUserScopesAsync(userId, Translator.Instance.Language);
            await Task.WhenAll(ownClientsTask, userScopesTask);

            ViewData["userScopes"] = userScopesTask.Result;
            ViewData["clients"] = ownClientsTask.Result.Concat(await this.oauth2Logic.FindClientsAsync(userScopesTask.Result.Select(us => us.ClientID).Distinct().ToArray())).Distinct();
            ViewData["userId"] = userId;

            if (TempData["notification"] != null)
            {
                ViewData["notification"] = TempData["notification"];
                TempData.Remove("notification");
            }

            return View();
        }

        [HttpGet]
        [Route("add")]
        public ActionResult AddGet()
        {
            var client = new Client
            {
                UserID = this.User.Identity.GetClaimValue<int>(ClaimTypes.NameIdentifier),
                Type = ClientType.Public
            };
            return View("Add", client);
        }

        [HttpPost]
        [Route("add")]
        public async Task<ActionResult> AddPost(Client client)
        {
            try
            {
                if (await this.oauth2Logic.RegisterClientAsync(client) != null)
                {
                    TempData["notification"] = ("NOTIFICATION.CREATE-SUCCESS", "", Status.SUCCESS);
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewData["notification"] = ("NOTIFICATION.CREATE-FAILED", "Could not create or save the new client into DB", Status.ERROR);
                    return View("Add", client);
                }
            }
            catch (Exception ex)
            {
                ViewData["notification"] = ("NOTIFICATION.CREATE-FAILED", ex.ToString(), Status.ERROR);
                return View("Add", client);
            }
        }

        [HttpGet]
        [Route("delete/{clientId}")]
        public async Task<ActionResult> Delete(string clientId)
        {
            try
            {
                await this.oauth2Logic.RemoveClientAsync(clientId);
                TempData["notification"] = ("NOTIFICATION.DELETE-SUCCESS", "", Status.SUCCESS);
            }
            catch (Exception ex)
            {
                TempData["notification"] = ("NOTIFICATION.DELETE-FAILED", ex.ToString(), Status.ERROR);
            }
            return RedirectToAction("Index");
        }
    }
}