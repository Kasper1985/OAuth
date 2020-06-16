using System.Configuration;
using System.Linq;
using System.Web.Http;
using System.Web.Mvc;

using Unity;
using Unity.AspNet.Mvc;
using Unity.Injection;

using BusinessLogic;
using BusinessLogic.Interfaces;
using Data.Interfaces;
using Data.MSSQL;

namespace OAuthServer
{
    public partial class Startup
    {
        public void ConfigureUnity(HttpConfiguration config)
        {
            var container = new UnityContainer();

            string connectionString = ConfigurationManager.ConnectionStrings[ConfigurationManager.AppSettings["DB"]].ConnectionString;
            string SMTPSettings = ConfigurationManager.AppSettings["SMTP"];
            string PCMEmail = ConfigurationManager.AppSettings["PCM-EMail"];

            // Business logic implementations
            container.RegisterType<IOAuth2Logic, OAuth2Logic>()
                     .RegisterType<IUserLogic, UserLogic>()
                     .RegisterType<IMailLogic, MailLogic>(new InjectionProperty("SMTPSettings", SMTPSettings), new InjectionProperty("PCMEmail", PCMEmail));

            // Data access logic implementations
            container.RegisterType<IUserSource, UserSource>(new InjectionConstructor(connectionString))
                     .RegisterType<IOAuth2Source, OAuth2Source>(new InjectionConstructor(connectionString))
                     .RegisterType<ITranslateSource, TranslateSource>(new InjectionConstructor(connectionString));

            // Web API Dependency Resolver
            config.DependencyResolver = new Unity.AspNet.WebApi.UnityDependencyResolver(container);

            // MVC Dependency Resolver
            FilterProviders.Providers.Remove(FilterProviders.Providers.OfType<FilterAttributeFilterProvider>().First());
            FilterProviders.Providers.Add(new UnityFilterAttributeFilterProvider(container));
            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}