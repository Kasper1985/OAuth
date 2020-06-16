using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Web.Http;

using Models;

using OAuthServer.Extensions;
using OAuthServer.Content.Languages;
using OAuthServer.Providers;

using BusinessLogic.Interfaces;

namespace OAuthServer.Controllers
{
    [Authorize]
    public class OpenIDConnectController : ApiController
    {
        private readonly IUserLogic userLogic;
        private readonly IOAuth2Logic oauth2Logic;

        public OpenIDConnectController(IUserLogic userLogic, IOAuth2Logic oauth2Logic)
        {
            this.userLogic = userLogic ?? throw new ArgumentNullException(nameof(userLogic));
            this.oauth2Logic = oauth2Logic ?? throw new ArgumentNullException(nameof(oauth2Logic));
        }

        [HttpGet]
        [Route("userinfo")]
        public async Task<Dictionary<string, object>> GetUserInfoAsync()
        {
            User user = await this.userLogic.GetUserAsync(this.User.Identity.GetClaimValue<int>(ClaimTypes.NameIdentifier));

            var userInfo = new Dictionary<string, object>();
            if (this.User.Identity.TryGetClaimValue("Scope", out string scopesCollection) && !string.IsNullOrEmpty(scopesCollection))
            {
                string[] scopes = scopesCollection.Split(' ');
                if (!scopes.Contains("openid", StringComparer.CurrentCultureIgnoreCase))
                    return userInfo;

                // Open ID info
                userInfo.Add("iss", Request.RequestUri.GetLeftPart(UriPartial.Authority));
                if (this.User.Identity.TryGetClaimValue(ClaimTypes.NameIdentifier, out int subject))
                    userInfo.Add("sub", subject);
                if (this.User.Identity.TryGetClaimValue("ClientId", out string audience))
                    userInfo.Add("aud", audience);
                if (this.User.Identity.TryGetClaimValue("IssuedUtc", out ulong issuedAt))
                    userInfo.Add("iat", issuedAt);
                if (this.User.Identity.TryGetClaimValue("ExpiresUtc", out ulong expiresAt))
                    userInfo.Add("exp", expiresAt);

                foreach (string scope in scopes)
                    switch (true)
                    {
                        case bool b when scope.Equals("email", StringComparison.CurrentCultureIgnoreCase):
                            userInfo.Add("email", user.EMail);
                            break;

                        case bool b when scope.Equals("profile", StringComparison.CurrentCultureIgnoreCase):
                            userInfo.Add("salutation", user.Salutation);
                            userInfo.Add("title", user.Title);
                            userInfo.Add("name_first", user.NameFirst);
                            userInfo.Add("name_last", user.NameLast);
                            userInfo.Add("phone", user.Phone);
                            userInfo.Add("fax", user.Fax);
                            break;
                    }
            }
            return userInfo;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("metadata")]
        public async Task<Dictionary<string, object>> GetServerMetadata()
        {
            IEnumerable<Scope> scopes = await this.oauth2Logic.GetScopesAsync(Translator.Instance.Language);
            RSAParameters publicKey = new OAuthIDTokenProvider().PublicKey;
            return new Dictionary<string, object>
            {
                { "authorization_endpoint", Paths.AUTHORIZE },
                { "userinfo_endpoint", Paths.USER_INFO },
                { "token_endpoint", Paths.TOKEN },
                { "claims_supported", scopes.Select(s => s.Name) },
                { "id_token_signing_alg", "RSA256" },
                { "public_key", new Dictionary<object, object>
                                {
                                    { "Modulus", Convert.ToBase64String(publicKey.Modulus) },
                                    { "Exponent", Convert.ToBase64String(publicKey.Exponent) }
                                }
                }
            };
        }
    }
}
