using System.Web;
using System.Web.Mvc;

using Microsoft.Owin.Security;

namespace OAuthServer.Attributes
{
    public class PCMAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            IAuthenticationManager authentication = filterContext.HttpContext.GetOwinContext().Authentication;
            AuthenticateResult ticket = authentication.AuthenticateAsync("PCM").Result;
            if (!ticket?.Identity.IsAuthenticated ?? true)
                base.OnAuthorization(filterContext);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            IAuthenticationManager authentication = filterContext.HttpContext.GetOwinContext().Authentication;
            authentication.Challenge("PCM");
        }
    }
}