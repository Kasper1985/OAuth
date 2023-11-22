using System;
using System.Security.Principal;
using System.Web;
using System.Net;

using Microsoft.Owin.Security;

namespace OAuthServer
{
    public class Global : HttpApplication
    {
        private IAuthenticationManager AuthenticationManager => Request.GetOwinContext().Authentication;

        protected void  Application_AuthenticateRequest(object sender, EventArgs e)
        {
            var ticket = this.AuthenticationManager.AuthenticateAsync("PCM").Result;
            if (ticket?.Identity != null)
                HttpContext.Current.User = new GenericPrincipal(ticket.Identity, Array.Empty<string>());
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            if (Response.StatusCode == (int)HttpStatusCode.Unauthorized)
            {
                Response.ClearContent();
                Response.RedirectToRoute("Default", new { controller = "account", action = "login", ReturnUrl = Request.Url.GetComponents(UriComponents.PathAndQuery, UriFormat.UriEscaped) });
            }
        }
    }
}