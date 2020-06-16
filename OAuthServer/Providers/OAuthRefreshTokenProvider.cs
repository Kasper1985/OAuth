using System;
using System.Threading.Tasks;

using Microsoft.Owin.Security.Infrastructure;

namespace OAuthServer.Providers
{
    public class OAuthRefreshTokenProvider : IAuthenticationTokenProvider
    {
        internal string LastRefreshToken { get; private set; }
        internal DateTime ExpireTime { get; private set; }

        public void Create(AuthenticationTokenCreateContext context) => this.CreateRefreshToken(context);
        public void Receive(AuthenticationTokenReceiveContext context) => this.ReceiveRefreshToken(context);
        public Task CreateAsync(AuthenticationTokenCreateContext context) => Task.Run(() => this.CreateRefreshToken(context));
        public Task ReceiveAsync(AuthenticationTokenReceiveContext context) => Task.Run(() => this.ReceiveRefreshToken(context));


        private void CreateRefreshToken(AuthenticationTokenCreateContext context)
        {
            context.SetToken(context.SerializeTicket());
            this.LastRefreshToken = context.Token;
            this.ExpireTime = context.Ticket.Properties.ExpiresUtc?.DateTime ?? DateTime.MinValue;
        }

        private void ReceiveRefreshToken(AuthenticationTokenReceiveContext context)
        {
            this.LastRefreshToken = context.Token;
            context.DeserializeTicket(context.Token);
            this.ExpireTime = context.Ticket.Properties.ExpiresUtc?.DateTime ?? DateTime.MinValue;
        }
    }
}