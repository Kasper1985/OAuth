using System.Collections.Generic;
using System.Web.Mvc;

using OAuthServer.Models;

namespace OAuthServer.Controllers
{
    [Authorize]
    public class ApplicationsController : Controller
    {
        public ActionResult Index()
        {
            ViewData["applications"] = new List<Application>
            {
                new Application { Title = "e@Care", Url = @"http://localhost:4200" },
                new Application { Title = "ERP", Url = @"http://localhost:4201" },
                new Application { Title = "MCC", Url = @"http://localhost:4202" },
                new Application { Title = "ManagementToday", Url = @"https://procareline.com/MT" },
            };

            return View();
        }
    }
}