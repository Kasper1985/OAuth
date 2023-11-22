using System.Web;
using System.Web.Mvc;

using Microsoft.Owin.Security;

namespace OAuthServer.Attributes
{
    public class PCMAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var authentication = filterContext.HttpContext.GetOwinContext().Authentication;
            var ticket = authentication.AuthenticateAsync("PCM").Result;
            if (!ticket?.Identity.IsAuthenticated ?? true)
                base.OnAuthorization(filterContext);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var authentication = filterContext.HttpContext.GetOwinContext().Authentication;
            authentication.Challenge("PCM");
        }
    }
}