using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Text;
using System.Security.Cryptography;
using System.Web.Http.Dependencies;

using Microsoft.Owin.Security.OAuth;

using OAuthServer.Extensions;

using Models;
using Models.Enumerations;

using BusinessLogic.Interfaces;

using Data.Common.Exceptions;

namespace OAuthServer.Providers
{
    public class OAuthAuthorizationServerProvider : IOAuthAuthorizationServerProvider
    {
        private readonly IDependencyResolver resolver;

        private string ClientID { get; set; }

        public OAuthAuthorizationServerProvider(IDependencyResolver resolver) => this.resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));

        #region Validation
        // Validation of the client trying to gain an access token
        public Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context) => Task.Run(async () =>
        {
            if (context.TryGetFormCredentials(out string client_id, out string client_secret))
                try
                {
                    var oauthLogic = this.resolver.GetService(typeof(IOAuth2Logic)) as IOAuth2Logic;
                    Client client = await oauthLogic.GetClientAsync(client_id);
                    switch (client.Type)
                    {
                        case ClientType.Public:
                            string code_verifier = context.Parameters["code_verifier"];
                            string code = context.Parameters["code"];
                            if (string.IsNullOrEmpty(code_verifier) || string.IsNullOrEmpty(code))
                                context.SetError("invalid_client", "\"code_verifier\" or/and \"code\" parameter are missing. Please use Authorization Code Flow with PKCE to authorize the client.");
                            else
                            {
                                PKCE pkce = await oauthLogic.FindPKCEAsync(code);
                                if (pkce != null)
                                    switch (pkce.CodeChallengeMethod)
                                    {
                                        case EncryptionMethod.Plain:
                                            if (pkce.Equals(code_verifier))
                                                context.Validated(client_id);
                                            else
                                                context.SetError("invalid_client", "Code verifier does not match the code challenge.");
                                            break;

                                        case EncryptionMethod.S256:
                                        default:
                                            using (var sha256 = SHA256.Create())
                                            {
                                                byte[] hashValue = sha256.ComputeHash(Encoding.UTF8.GetBytes(code_verifier));
                                                string codeTest = string.Join("", hashValue.Select(x => $"{x:x2}"));
                                                if (codeTest.Equals(pkce.CodeChallenge))
                                                    context.Validated(client_id);
                                                else
                                                    context.SetError("invalid_client", "Code verifier does not match the code challenge.");
                                            }
                                            break;
                                    }
                                else
                                    context.SetError("invalid_client", "code challenge couldn't be found.");
                            }
                            break;

                        case ClientType.Private:
                            if (string.IsNullOrEmpty(client_secret))
                                context.SetError("invalid_client", "Client secret is missing in the request.");
                            else if (client.Secret.Equals(client_secret))
                                context.Validated(client_id);
                            break;

                        case ClientType.FirstParty:
                            context.Validated(client_id);
                            break;

                        default:
                            context.SetError("invalid_client", "Unknown type of the client.");
                            break;
                    }
                }
                catch (AmbiguousClientException)    { context.SetError("invalid_client", $"There are more than one client with the id: \"{client_id}\"."); }
                catch (ClientNotFoundException)     { context.SetError("invalid_client", $"There is no registered clients with the id: \"{client_id}\"."); }
                catch (Exception)                   { context.SetError("invalid_client", $"An unknown error occurred while searching for the client with id \"{client_id}\"."); }
                finally
                {
                    if (context.IsValidated)
                        this.ClientID = context.ClientId;
                }
        });

        public Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context) => Task.Run(async () =>
        {
            try
            {
                var oauthLogic = this.resolver.GetService(typeof(IOAuth2Logic)) as IOAuth2Logic;

                Client client = await oauthLogic.GetClientAsync(context.ClientId);
                if (string.Compare(client.URI, context.RedirectUri, StringComparison.CurrentCultureIgnoreCase) == 0)
                    context.Validated(context.RedirectUri);
            }
            catch (AmbiguousClientException) { context.SetError("invalid_client", $"There are more than one client with the id: \"{context.ClientId}\"."); }
            catch (ClientNotFoundException) { context.SetError("invalid_client", $"There is no registered clients with the id: \"{context.ClientId}\"."); }
            catch (Exception) { context.SetError("invalid_client", $"An unknown error occurred while searching for the client with id \"{context.ClientId}\"."); }
            
        });

        public Task ValidateTokenRequest(OAuthValidateTokenRequestContext context) => Task.Run(() => context.Validated());

        public Task ValidateAuthorizeRequest(OAuthValidateAuthorizeRequestContext context) => Task.Run(() =>
        {
            if (string.Compare(context.AuthorizeRequest.ResponseType, "token", StringComparison.InvariantCultureIgnoreCase) == 0)
                context.SetError("prohibited_flow", "The implicit flow is prohibited because of security lack. Please use Authorization Code Flow with PKCE for OAuth2.");
            else
                context.Validated();
        });
        #endregion // Validation

        #region OAuth Flows
        // Authorize user by ---=== Password Flow ===---
        public Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context) => Task.Run(async () =>
        {
            try
            {
                var userLogic = this.resolver.GetService(typeof(IUserLogic)) as IUserLogic;
                User user = await userLogic.LoginAsync(context.UserName, context.Password);

                if (user != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()),
                        new Claim("ClientId", context.ClientId),
                    };
                    if (context.Scope.Count > 0)
                        claims.Add(new Claim("Scope", string.Join(" ", context.Scope)));

                    var identity = new ClaimsIdentity(claims, OAuthDefaults.AuthenticationType);
                    context.Validated(identity);
                }
                else
                    context.SetError("access_denied", "Wrong login or password data. Please check your credential or advise to your administrator.");
            }
            catch (AmbiguousLoginException) { context.SetError("access_denied", "There are more than one user with the same credentials. Please advise to your administrator."); }
            catch (Exception) { context.SetError("access_denied", "There is an unknown error occurred. Please advise to your administrator."); }
        });

        // Authorize client to access client's own resources by ---=== Client Credentials Flow ===---
        public Task GrantClientCredentials(OAuthGrantClientCredentialsContext context) => Task.Run(async () => 
        {
            try
            {
                var oauthLogic = this.resolver.GetService(typeof(IOAuth2Logic)) as IOAuth2Logic;

                Client client = await oauthLogic.GetClientAsync(context.ClientId);
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, client.ID),
                    new Claim("Scope", "client")
                };

                var identity = new ClaimsIdentity(claims, OAuthDefaults.AuthenticationType);
                context.Validated(identity);
            }
            catch (AmbiguousClientException) { context.SetError("invalid_client", $"There are more than one client with the id: \"{context.ClientId}\"."); }
            catch (ClientNotFoundException) { context.SetError("invalid_client", $"There is no registered clients with the id: \"{context.ClientId}\"."); }
            catch (Exception) { context.SetError("invalid_client", $"An unknown error occurred while searching for the client with id \"{context.ClientId}\"."); }
        });

        // Authorize user by ---=== Authorization Code Flow ===---
        public Task GrantAuthorizationCode(OAuthGrantAuthorizationCodeContext context) => Task.Run(() => 
        {
            var identity = new ClaimsIdentity(context.Ticket.Identity.Claims, OAuthDefaults.AuthenticationType);
            context.Validated(identity);
        });

        // Authorize user by ---=== Refresh Token Flow ===---
        public Task GrantRefreshToken(OAuthGrantRefreshTokenContext context) => Task.Run(async () => 
        {
            var oauthLogic = this.resolver.GetService(typeof(IOAuth2Logic)) as IOAuth2Logic;

            try
            {
                int userID = Convert.ToInt32(context.Ticket.Identity.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                LastRefreshToken lastRefreshToken = await oauthLogic.GetLastRefreshTokenAsync(userID, context.ClientId);
                if (lastRefreshToken == null)
                    context.SetError("invalid_token", "There is no refresh token, that assigned to your client.");
                else if (lastRefreshToken.RefreshToken != ((OAuthRefreshTokenProvider)context.Options.RefreshTokenProvider).LastRefreshToken)
                    context.SetError("invalid_token", "The handed over refresh token does not match the last generated refresh token. Probably it has been renewed.");
                else
                {
                    try
                    {
                        await oauthLogic.RemoveLastRefreshTokenAsync(((OAuthRefreshTokenProvider)context.Options.RefreshTokenProvider).LastRefreshToken);
                    }
                    catch (Exception) { /* Do nothing if there is an error by removing last refresh token. Client should be able to get a new access token.*/ }
                    finally { context.Validated(); }
                }
            }
            catch (Exception)
            {
                context.SetError("invalid_token", "An error occurred by getting the last refresh token.");
            }
        });

        // Authorize user by custom OAuth Flows (unspecified grant types)
        public Task GrantCustomExtension(OAuthGrantCustomExtensionContext context) => throw new NotSupportedException();
        #endregion // OAuth Flows

        #region Endpoints
        // Through defining of Authorize and Token endpoint paths not additional match checks are needed
        public Task MatchEndpoint(OAuthMatchEndpointContext context) => Task.Run(() => { });

        public Task TokenEndpoint(OAuthTokenEndpointContext context) => Task.Run(() =>
        {
            if (!context.Identity.TryGetClaimValue("AuthTime", out ulong authTime))
                context.Identity.AddClaim(new Claim("AuthTime", ((ulong)(context.Properties.IssuedUtc.Value.DateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds).ToString()));

            if (context.Identity.TryGetClaimValue("IssuedUtc", out ulong issuedUtc))
                context.Identity.TryRemoveClaim(new Claim("IssuedUtc", issuedUtc.ToString()));
            context.Identity.AddClaim(new Claim("IssuedUtc", ((ulong)(context.Properties.IssuedUtc.Value.DateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds).ToString()));
            if (!context.Identity.TryGetClaimValue("ExpiresUtc", out ulong expiresUtc))
                context.Identity.TryRemoveClaim(new Claim("ExpiresUtc", issuedUtc.ToString()));
            context.Identity.AddClaim(new Claim("ExpiresUtc", ((ulong)(context.Properties.ExpiresUtc.Value.DateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds).ToString()));

            // Create ID Token
            if (context.Identity.TryGetClaimValue("scope", out string scopes) &&
                !string.IsNullOrEmpty(scopes) &&
                scopes.Split(' ').Contains("openid", StringComparer.CurrentCultureIgnoreCase))
            {
                context.Identity.TryGetClaimValue(ClaimTypes.NameIdentifier, out int subject);
                context.Identity.TryGetClaimValue("ClientId", out string audience);
                context.Identity.TryGetClaimValue("IssuedUtc", out ulong issuedAt);
                context.Identity.TryGetClaimValue("ExpiresUtc", out ulong expiresAt);

                var idTokenPayload = new Dictionary<string, object>
                {
                    { "iss", context.Request.Uri.GetLeftPart(UriPartial.Authority) },
                    { "sub", subject },
                    { "aud", audience },
                    { "iat", issuedAt },
                    { "exp", expiresAt }
                };

                context.AdditionalResponseParameters.Add("id_token", new OAuthIDTokenProvider().CreateIDToken(idTokenPayload));
            }
        });

        public Task TokenEndpointResponse(OAuthTokenEndpointResponseContext context) => Task.Run(async () => 
        {
            try
            {
                var oauthLogic = this.resolver.GetService(typeof(IOAuth2Logic)) as IOAuth2Logic;
                int userID = Convert.ToInt32(context.Identity.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                await oauthLogic.RemoveLastRefreshTokenAsync(userID, this.ClientID);
                await oauthLogic.SaveLastRefreshTokenAsync(new LastRefreshToken
                {
                    UserID = userID,
                    ClientID = this.ClientID,
                    RefreshToken = ((OAuthRefreshTokenProvider)context.Options.RefreshTokenProvider).LastRefreshToken,
                    ExpireTime = ((OAuthRefreshTokenProvider)context.Options.RefreshTokenProvider).ExpireTime
                });
            }
            catch (Exception) { /* Do nothing. The client should gain an access token even if the refresh token couldn't be saved.*/ }
        });

        public Task AuthorizeEndpoint(OAuthAuthorizeEndpointContext context) => Task.Run(() => { });

        public Task AuthorizationEndpointResponse(OAuthAuthorizationEndpointResponseContext context) => Task.Run(async () => {
            var oauthLogic = this.resolver.GetService(typeof(IOAuth2Logic)) as IOAuth2Logic;
            await oauthLogic.SavePKCEAsync(context.AuthorizationCode, new PKCE
            {
                ClientID = context.Request.Query["client_id"],
                CodeChallenge = Encoding.UTF8.GetString(Convert.FromBase64String(context.Request.Query["code_challenge"])),
                CodeChallengeMethod = (EncryptionMethod)Enum.Parse(typeof(EncryptionMethod), context.Request.Query["code_challenge_method"])
            });
        });
        #endregion // Endpoints
    }
}