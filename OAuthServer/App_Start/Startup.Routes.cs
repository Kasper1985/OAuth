using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace OAuthServer
{
    public partial class Startup
    {
        public void ConfigureRoutes(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                name: "DefaultAPI",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );


            RouteCollection routes = RouteTable.Routes;
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapMvcAttributeRoutes();

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Applications", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "ErrorHandler",
                url: "Error/{action}/{errMsg}",
                defaults: new { controller = "Error", action = "Index", errMsg = UrlParameter.Optional }
            );
        }
    }
}