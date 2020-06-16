using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Models;
using Models.Extensions;
using Data.Interfaces;

using BusinessLogic.Interfaces;

namespace BusinessLogic
{
    public class MailLogic : IMailLogic
    {
        private readonly ITranslateSource translateSource;

        public string PCMEmail { get; set; }
        public string SMTPSettings { private get; set; }

        public MailLogic(ITranslateSource translateSource) => this.translateSource = translateSource ?? throw new ArgumentNullException(nameof(translateSource));

        private SmtpClient SMTPClient()
        {
            string[] mailSettings = this.SMTPSettings?.Split(';') ?? throw new ArgumentNullException(nameof(this.SMTPSettings));

            string host = !string.IsNullOrEmpty(mailSettings[0]) ? mailSettings[0] : throw new ArgumentException("SMTP host name cannot be empty.");
            SmtpClient client = new SmtpClient(host);

            if (mailSettings.Length > 1 && !string.IsNullOrEmpty(mailSettings[1]) && int.TryParse(mailSettings[1], out int port))
                client.Port = port;

            if (mailSettings.Length > 3 && !string.IsNullOrEmpty(mailSettings[2]) && !string.IsNullOrEmpty(mailSettings[3]))
                client.Credentials = new NetworkCredential(mailSettings[2], mailSettings[3]);

            if (mailSettings.Length > 4 && !string.IsNullOrEmpty(mailSettings[4]) && bool.TryParse(mailSettings[1], out bool ssl))
                client.EnableSsl = ssl;

            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            return client;
        }

        #region Interface functions
        public async Task SendPasswordMailAsync(string password, User user, string ccode = "de")
        {
            using (SmtpClient client = this.SMTPClient())
            {
                List<Task<string>> tasks = new List<Task<string>>
                {
                    /* 0 */ this.translateSource.GetTranslationAsync("NewPasswordRequested", ccode),
                    /* 1 */ user.Salutation.In(3, 5, 6, 10, 11, 12, 13) ? this.translateSource.GetTranslationAsync("DearSir", ccode) : this.translateSource.GetTranslationAsync("DearMadame", ccode),
                    /* 2 */ user.Salutation.In(3, 5, 6, 10, 11, 12, 13) ? this.translateSource.GetTranslationAsync("Herr", ccode) : this.translateSource.GetTranslationAsync("Frau", ccode),
                    /* 3 */ this.translateSource.GetTranslationAsync("PasswordMailText", ccode),
                    /* 4 */ this.translateSource.GetTranslationAsync("YourPasswordIs", ccode),
                    /* 5 */ this.translateSource.GetTranslationAsync("PasswordHint", ccode),
                    /* 6 */ this.translateSource.GetTranslationAsync("MitFreundlichenGruessen", ccode),
                    /* 7 */ this.translateSource.GetTranslationAsync("MailPCMPart", ccode),
                    /* 8 */ this.translateSource.GetTranslationAsync("MailDisclaimer", ccode)
                };
                string[] results = await Task.WhenAll(tasks);

                using (MailMessage message = new MailMessage(new MailAddress(this.PCMEmail), new MailAddress(user.EMail)))
                {
                    message.Subject = Regex.Replace($"{results[2]} {user.Title ?? ""} {user.NameLast} {results[0].Trim(';', ' ')}", "[ ]+", " ");

                    StringBuilder html = new StringBuilder();
                    html.Append("<html>");
                    html.AppendLine("<body style=\"font-size: 12pt; font-family: Calibri, 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;\">");
                    html.AppendLine("<p>");
                    html.Append(string.Join(" ", results[1], results[2], user.Title ?? "", (user.NameLast ?? "") + ","));
                    html.Append("</p>");
                    html.AppendLine("<p>");
                    DateTime now = DateTime.Now;
                    html.Append(Regex.Replace(string.Format(results[3], now.ToString("dd.MM.yyyy"), now.ToString("hh:mm:ss"), ""), "[ ]+", " "));
                    html.Append("</p>");
                    html.AppendLine("<p>");
                    html.Append(string.Format(results[4], $"<strong>{password}</strong>"));
                    html.Append("</p>");
                    html.AppendLine("<p>");
                    html.Append(results[5]);
                    html.Append("</p>");
                    html.AppendLine("<p>");
                    html.Append(results[6]);
                    html.Append("</p>");
                    html.AppendLine("<hr size=\"2\" width=\"100%\" align=\"center\" />");
                    html.AppendLine("<pre style=\"font-size: 8pt; color: #666666;\">");
                    html.Append(results[7]);
                    html.Append("</pre>");
                    html.AppendLine("<hr size=\"2\" width=\"100%\" align=\"center\" />");
                    html.AppendLine("<pre style=\"font-size: 8pt; color: #666666;\">");
                    html.Append(results[8]);
                    html.Append("</pre>");
                    html.AppendLine("</body>");
                    html.AppendLine("</html>");

                    message.Body = html.ToString();
                    message.BodyEncoding = Encoding.UTF8;
                    message.IsBodyHtml = true;

                    await client.SendMailAsync(message);
                }
            }
        }
        #endregion // Interface fucntions
    }
}
