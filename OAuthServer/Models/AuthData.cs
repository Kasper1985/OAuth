using System;
using System.Security.Claims;

using Microsoft.Owin.Security;

using OAuthServer.Extensions;

namespace OAuthServer.Models
{
    public class AuthData
    {
        public int? UserIdentifier { get; private set; }
        public DateTimeOffset AuthTime { get; private set; }
        public DateTimeOffset AuthExpires { get; private set; }
        public bool IsPersistent { get; private set; }

        public AuthData(AuthenticateResult ticket)
        {
            this.UserIdentifier = ticket?.Identity?.GetClaimValue<int>(ClaimTypes.NameIdentifier);
            this.AuthTime = ticket?.Properties?.IssuedUtc ?? DateTimeOffset.Now;
            this.AuthExpires = ticket?.Properties?.ExpiresUtc ?? DateTimeOffset.Now;
            this.IsPersistent = ticket?.Properties?.IsPersistent ?? false;
        }
    }
}