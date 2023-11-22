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
            UserIdentifier = ticket?.Identity?.GetClaimValue<int>(ClaimTypes.NameIdentifier);
            AuthTime = ticket?.Properties?.IssuedUtc ?? DateTimeOffset.Now;
            AuthExpires = ticket?.Properties?.ExpiresUtc ?? DateTimeOffset.Now;
            IsPersistent = ticket?.Properties?.IsPersistent ?? false;
        }
    }
}