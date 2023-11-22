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
            var userId = User.Identity.GetClaimValue<int>(ClaimTypes.NameIdentifier);
            var clientId = Request.QueryString.Get("client_id") ?? "";
            var scopesRequested = (Request.QueryString.Get("scope") ?? "").Split(' ');

            var taskUserScopes = oauth2Logic.GetUserScopesAsync(userId, clientId, Translator.Instance.Language);
            var taskScopes = oauth2Logic.GetScopesAsync(Translator.Instance.Language, scopesRequested);
            var taskClient = oauth2Logic.GetClientAsync(clientId);
            await Task.WhenAll(taskUserScopes, taskScopes, taskClient);

            var userScopes = taskUserScopes.Result?.Where(us => us.Grant);
            if (userScopes?.Count() > 0)
            {
                var scopeNamesToRequest = scopesRequested.Except(userScopes.Select(us => us.Scope.Name));
                var scopesToRequest = await this.oauth2Logic.GetScopesAsync(Translator.Instance.Language, scopeNamesToRequest.ToArray());

                if (scopesToRequest.Count() > 0)
                    ViewData["scopes"] = scopesToRequest;
                else
                {
                    var identity = User.Identity as ClaimsIdentity;
                    identity = new ClaimsIdentity(identity.Claims, "Bearer", identity.NameClaimType, identity.RoleClaimType);
                    identity.AddClaim(new Claim("Scope", string.Join(" ", scopesRequested)));
                    AuthenticationManager.SignIn(identity);
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
            var userId = this.User.Identity.GetClaimValue<int>(ClaimTypes.NameIdentifier);
            var clientId = Request.QueryString.Get("client_id") ?? "";
            var requestedScopes = Request.QueryString.Get("scope") ?? "";

            if (silent || !string.IsNullOrEmpty(Request.Form.Get("submit.allow")))
            {
                // Create user scopes to make changes to DB
                var scopes = await oauth2Logic.GetScopesAsync(Translator.Instance.Language, requestedScopes.Split(' '));
                var userScopes = scopes.Select(s => new UserScope { UserId = userId, ClientId = clientId, Scope = s, Grant = true }).ToArray();

                // Remove existing granted permissions and then save them => Update permissions, if already existing
                await oauth2Logic.RemoveUserScopesAsync(userScopes);
                await oauth2Logic.AddUserScopesAsync(userScopes);

                var identity = User.Identity as ClaimsIdentity;
                identity = new ClaimsIdentity(identity.Claims, "Bearer", identity.NameClaimType, identity.RoleClaimType);
                identity.AddClaim(new Claim("Scope", requestedScopes));
                AuthenticationManager.SignIn(identity);
            }
            else
            {
                var redirectUri = Request.QueryString.Get("redirect_uri");
                if (!string.IsNullOrEmpty(redirectUri))
                    return Redirect(redirectUri);
            }

            return View("Index");
        }
    }
}