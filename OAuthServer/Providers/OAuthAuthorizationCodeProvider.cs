using System;
using System.Threading.Tasks;
using System.Web.Http.Dependencies;

using Microsoft.Owin.Security.Infrastructure;

using Models;

using BusinessLogic.Interfaces;

namespace OAuthServer.Providers
{
    public class OAuthAuthorizationCodeProvider : IAuthenticationTokenProvider
    {
        private readonly IDependencyResolver resolver;

        public OAuthAuthorizationCodeProvider(IDependencyResolver resolver) => this.resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));

        public void Create(AuthenticationTokenCreateContext context) => CreateAuthorizationCode(context);
        public void Receive(AuthenticationTokenReceiveContext context) => ReceiveAuthorizationCode(context);
        public Task CreateAsync(AuthenticationTokenCreateContext context) => Task.Run(() => CreateAuthorizationCode(context));
        public Task ReceiveAsync(AuthenticationTokenReceiveContext context) => Task.Run(() => ReceiveAuthorizationCode(context));


        private void CreateAuthorizationCode(AuthenticationTokenCreateContext context)
        {
            context.SetToken(Guid.NewGuid().ToString("n") + Guid.NewGuid().ToString("n"));

            var oauthLogic = this.resolver.GetService(typeof(IOAuth2Logic)) as IOAuth2Logic;
            oauthLogic.SaveAuthorizationCodeAsync(new AuthCode
            {
                ClientId = context.Request.Query["client_id"],
                AuthorizationCode = context.Token,
                AuthorizationTicket = context.SerializeTicket()
            }).Wait();
        }

        private void ReceiveAuthorizationCode(AuthenticationTokenReceiveContext context)
        {
            var oauthLogic = this.resolver.GetService(typeof(IOAuth2Logic)) as IOAuth2Logic;
            var authCode = oauthLogic.FindAuthCodeAsync(context.Token).Result;
            if (authCode == null)
                return;
            context.DeserializeTicket(authCode.AuthorizationTicket);
            oauthLogic.RemoveAuthorizationCodeAsync(authCode.AuthorizationCode).Wait();
        }
    }
}