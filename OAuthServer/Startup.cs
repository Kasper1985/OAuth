using System.Web.Http;
using System.Web.Optimization;

using OAuthServer.Content.Languages;

using Owin;
using Microsoft.Owin;
using Microsoft.Owin.Cors;

[assembly: OwinStartup(typeof(OAuthServer.Startup))]

namespace OAuthServer
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // This has to be on the first place, otherwise CORS won't be working
            app.UseCors(CorsOptions.AllowAll);

            var config = new HttpConfiguration();

            ConfigureRoutes(config);
            ConfigureUnity(config);
            ConfigureAuth(app, config);

            BundleConfig.RegisterBundles(BundleTable.Bundles);

            app.Use(async(ctx, next) =>
            {
                string baseURL = $"{ctx.Request.Uri.Scheme}://{ctx.Request.Uri.Host}:{ctx.Request.Uri.Port}";
                string[] languages = ctx.Request.Headers.Get("Accept-Language").Split(',');
                
                Translator.InitializeTranslator(languages[0] ?? "de", $"{baseURL}/Content/Languages");
                if (Translator.Instance.Language != languages[0])
                    Translator.Instance.ChangeLanguage(languages[0]);

                await next();
            });

            app.UseWebApi(config);

        }
    }
}