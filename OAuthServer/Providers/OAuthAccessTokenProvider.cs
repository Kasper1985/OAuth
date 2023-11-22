using System.Threading.Tasks;

using Microsoft.Owin.Security.Infrastructure;

namespace OAuthServer.Providers
{
    public class OAuthAccessTokenProvider : IAuthenticationTokenProvider
    {
        public void Create(AuthenticationTokenCreateContext context) => CreateAccessToken(context);
        public void Receive(AuthenticationTokenReceiveContext context) => ReceiveAccessToken(context);
        public Task CreateAsync(AuthenticationTokenCreateContext context) => Task.Run(() => CreateAccessToken(context));
        public Task ReceiveAsync(AuthenticationTokenReceiveContext context) => Task.Run(() => ReceiveAccessToken(context));


        private void CreateAccessToken(AuthenticationTokenCreateContext context)
        {
            context.SetToken(context.SerializeTicket());
        }

        private void ReceiveAccessToken(AuthenticationTokenReceiveContext context)
        {
            context.DeserializeTicket(context.Token);
        }
    }
}