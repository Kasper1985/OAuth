using System;
using System.Net;
using System.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OAuthServer.Content.Languages
{
    public class Translator
    {
        private readonly string[] LANGUAGES = new string[] { "en", "de" };
        private JObject Json { get; set; }
        private string language;

        private string Source { get; set; }
        public string Language
        {
            get => this.language;
            private set => this.language = LANGUAGES.Contains(value) ? value : LANGUAGES.Contains(value.Split('-')[0]) ? value.Split('-')[0] : LANGUAGES[0];
        }

        private static Translator instance;
        public static Translator Instance => instance ?? InitializeTranslator();

        private Translator(string language, string source)
        {
            this.Language = language;
            this.Source = source;
            this.LoadTranslation();
        }

        private void LoadTranslation()
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    string jsonText = wc.DownloadString($"{this.Source}/{this.Language}.json");
                    this.Json = JsonConvert.DeserializeObject(jsonText) as JObject;
                }
            }
            catch (Exception)
            { }
        }

        public static Translator InitializeTranslator(string language = "", string source = "")
        {
            if (instance == null)
                instance = new Translator(language, source);

            return instance;
        }

        public string Translate(string path) => this.Json?.SelectToken(path)?.ToString() ?? path;

        public void ChangeLanguage(string language)
        {
            this.Language = language ?? LANGUAGES[0];
            this.LoadTranslation();
        }

        public void ChangeSource(string source)
        {
            this.Source = source;
            this.LoadTranslation();
        }
    }
}