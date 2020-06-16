using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Claims;
using System.Security.Principal;

namespace OAuthServer.Extensions
{
    public static class IdentityExtensions
    {
        public static T GetClaimValue<T>(this IIdentity identity, string claimName)
        {
            var claimsIdentity = identity as ClaimsIdentity;
            var claim = claimsIdentity.FindFirst(claimName);

            if (claim == null)
                throw new ArgumentNullException(nameof(claim), $"Claim '{claimName}' not found.");

            TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter != null)
                return (T)converter.ConvertFrom(claim.Value);
            else
                throw new InvalidCastException($"Cannot convert from {typeof(string)} to {typeof(T)}");
        }

        public static bool TryGetClaimValue<T>(this IIdentity identity, string claim, out T value)
        {
            try
            {
                value = identity.GetClaimValue<T>(claim);
                return true;
            }
            catch (Exception)
            {
                value = default;
                return false;
            }
        }

        public static IEnumerable<Claim> GetClaims(this IIdentity identity) => (identity as ClaimsIdentity).Claims;
    }
}