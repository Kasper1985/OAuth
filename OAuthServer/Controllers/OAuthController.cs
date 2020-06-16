using System;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

using Microsoft.Owin.Security;

using Models;

using OAuthServer.Extensions;
using OAuthServer.Content.Languages;

using BusinessLogic.Interfaces;

namespace OAuthServer.Controllers
{
    [RoutePrefix("auth")]
    [Authorize]
    public class OAuthController : Controller
    {
        private readonly IOAuth2Logic oauth2Logic;
        private IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;

        public OAuthController(IOAuth2Logic oauth2Logic) => this.oauth2Logic = oauth2Logic ?? throw new ArgumentNullException(nameof(oauth2Logic));

        [HttpGet]
        [Route]
        public async Task<ActionResult> IndexGET()
        {
            if (Response.StatusCode != (int)HttpStatusCode.OK)
                return View("AuthorizeError");

            // Get user and client data
            int userId = this.User.Identity.GetClaimValue<int>(ClaimTypes.NameIdentifier);
            string clientId = Request.QueryString.Get("client_id") ?? "";
            string[] scopesRequested = (Request.QueryString.Get("scope") ?? "").Split(' ');

            var taskUserScopes = this.oauth2Logic.GetUserScopesAsync(userId, clientId, Translator.Instance.Language);
            var taskScopes = this.oauth2Logic.GetScopesAsync(Translator.Instance.Language, scopesRequested);
            var taskClient = this.oauth2Logic.GetClientAsync(clientId);
            await Task.WhenAll(taskUserScopes, taskScopes, taskClient);

            IEnumerable<UserScope> userScopes = taskUserScopes.Result?.Where(us => us.Grant);
            if (userScopes?.Count() > 0)
            {
                IEnumerable<string> scopeNamesToRequest = scopesRequested.Except(userScopes.Select(us => us.Scope.Name));
                IEnumerable<Scope> scopesToRequest = await this.oauth2Logic.GetScopesAsync(Translator.Instance.Language, scopeNamesToRequest.ToArray());

                if (scopesToRequest.Count() > 0)
                    ViewData["scopes"] = scopesToRequest;
                else
                {
                    var identity = this.User.Identity as ClaimsIdentity;
                    identity = new ClaimsIdentity(identity.Claims, "Bearer", identity.NameClaimType, identity.RoleClaimType);
                    identity.AddClaim(new Claim("Scope", string.Join(" ", scopesRequested)));
                    this.AuthenticationManager.SignIn(identity);
                }
            }
            else
                ViewData["scopes"] = taskScopes.Result;

            ViewData["client"] = taskClient.Result;
            return View("Index");
        }

        [HttpPost]
        [Route]
        public async Task<ActionResult> IndexPOST(bool silent = false)
        {
            int userId = this.User.Identity.GetClaimValue<int>(ClaimTypes.NameIdentifier);
            string clientId = Request.QueryString.Get("client_id") ?? "";
            string requestedScopes = Request.QueryString.Get("scope") ?? "";

            if (silent || !string.IsNullOrEmpty(Request.Form.Get("submit.allow")))
            {
                // Create user scopes to make changes to DB
                IEnumerable<Scope> scopes = await this.oauth2Logic.GetScopesAsync(Translator.Instance.Language, requestedScopes.Split(' '));
                UserScope[] userScopes = scopes.Select(s => new UserScope { UserID = userId, ClientID = clientId, Scope = s, Grant = true }).ToArray();

                // Remove existing granted permissions and then save them => Update permissions, if already existing
                await this.oauth2Logic.RemoveUserScopesAsync(userScopes);
                await this.oauth2Logic.AddUserScopesAsync(userScopes);

                var identity = this.User.Identity as ClaimsIdentity;
                identity = new ClaimsIdentity(identity.Claims, "Bearer", identity.NameClaimType, identity.RoleClaimType);
                identity.AddClaim(new Claim("Scope", requestedScopes));
                this.AuthenticationManager.SignIn(identity);
            }
            else
            {
                string redirectUri = Request.QueryString.Get("redirect_uri");
                if (!string.IsNullOrEmpty(redirectUri))
                    return Redirect(redirectUri);
            }

            return View("Index");
        }
    }
}