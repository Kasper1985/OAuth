using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Mvc;

using OAuthServer.Models;

namespace OAuthServer.Controllers
{
    [Authorize]
    public class ApplicationsController : Controller
    {
        public ActionResult Index()
        {
            var applications = new List<Application>();

            string[] erp = ConfigurationManager.AppSettings[nameof(erp)].Split(';');
            applications.Add(new Application { Title = erp[0], Url = erp[1], Icon = "pcm-ol-lager-bestand" });

            string[] ecare = ConfigurationManager.AppSettings[nameof(ecare)].Split(';');
            applications.Add(new Application { Title = ecare[0], Url = ecare[1], Icon = "pcm-ol-verpackung" });

            string[] mt = ConfigurationManager.AppSettings[nameof(mt)].Split(';');
            applications.Add(new Application { Title = mt[0], Url = mt[1], Icon = "pcm-ol-manager" });

            string[] srm = ConfigurationManager.AppSettings[nameof(srm)].Split(';');
            applications.Add(new Application { Title = srm[0], Url = srm[1], Icon = "pcm-ol-lieferant-lkw" });

            string[] dcc = ConfigurationManager.AppSettings[nameof(dcc)].Split(';');
            applications.Add(new Application { Title = dcc[0], Url = dcc[1], Icon = "pcm-ol-daten-update" });

            ViewData["applications"] = applications;

            ViewData["general"] = new List<Application>
            {
                // new Application { Title = "Artikelsuche", Url = @"http://localhost:4201", Icon = "pcm-ol-ordersatz" },
                new Application { Title = "Kontakt", Url = @"#", Icon = "pcm-ol-chat-1" },
                // new Application { Title = "Auswertung", Url = @"http://localhost:4200", Icon = "pcm-ol-auswertung" },
                new Application { Title = "Hilfe", Url = @"#", Icon = "pcm-ol-fragezeichen" },
                new Application { Title = "Infothek", Url = @"#", Icon = "pcm-ol-information" },
            };

            ViewData["news"] = new List<NewsBundle>
            {
                new NewsBundle { Id = 0, Title = "Allgemeines", News = new List<News>
                {
                    new News { Date = new DateTime(2019, 4, 22), Message = "Rückrufwarnung Joghurt" },
                    new News { Date = new DateTime(2019, 4, 18), Message = "Systemwartung" },
                    new News { Date = new DateTime(2019, 4, 2), Message = "Update Hinweise" },
                    new News { Date = new DateTime(2019, 4, 22), Message = "Rückrufwarnung Joghurt" },
                    new News { Date = new DateTime(2019, 4, 18), Message = "Systemwartung" },
                    new News { Date = new DateTime(2019, 4, 2), Message = "Update Hinweise" },
                    new News { Date = new DateTime(2019, 4, 22), Message = "Rückrufwarnung Joghurt" },
                    new News { Date = new DateTime(2019, 4, 18), Message = "Systemwartung" },
                    new News { Date = new DateTime(2019, 4, 2), Message = "Update Hinweise" }

                }},
                new NewsBundle { Id = 1, Title = "ERP" },
                new NewsBundle { Id = 2, Title = "e@Care" }
            };

            return View();
        }
    }
}