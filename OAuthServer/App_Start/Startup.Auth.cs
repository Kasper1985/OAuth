using System;
using System.Web.Http;
using System.Web.Helpers;
using System.Security.Claims;

using Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Owin.Security.Cookies;

namespace OAuthServer
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app, HttpConfiguration config)
        {
            // Enable Application Sign In Cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "PCM",
                AuthenticationMode = Microsoft.Owin.Security.AuthenticationMode.Passive,
                LoginPath = new PathString(Paths.LOGIN),
                LogoutPath = new PathString(Paths.LOGOUT),
                ExpireTimeSpan = TimeSpan.FromHours(24)
            });

            // Enable Application Sign In Bearer
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());

            // Setup OAuth2 Server
            app.UseOAuthAuthorizationServer(new OAuthAuthorizationServerOptions
            {
                AuthenticationType = OAuthDefaults.AuthenticationType,

                Provider = new Providers.OAuthAuthorizationServerProvider(config.DependencyResolver),

                AuthorizeEndpointPath = new PathString(Paths.AUTHORIZE),
                AuthorizationCodeExpireTimeSpan = TimeSpan.FromMinutes(1),
                AuthorizationCodeProvider = new Providers.OAuthAuthorizationCodeProvider(config.DependencyResolver),

                TokenEndpointPath = new PathString(Paths.TOKEN),
                AccessTokenExpireTimeSpan = TimeSpan.FromHours(1),
                AccessTokenProvider = new Providers.OAuthAccessTokenProvider(),

                RefreshTokenProvider = new Providers.OAuthRefreshTokenProvider(),
                ApplicationCanDisplayErrors = true,

                AllowInsecureHttp = true
            });

            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;
        }
    }
}