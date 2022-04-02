using System.Net;
using System.Net.Mail;

namespace BinLiteServer
{
    public static class EmailManager
    {
        public static void Send(string recipient, string subject, string body)
        {
            var url = Configuration.Get<string>("email.siteUrl");
            body = body.Replace("@p0", url);

            var client = new SmtpClient(Configuration.Get<string>("email.url"), Configuration.Get<int>("email.port"))
            {
                Credentials = new NetworkCredential(Configuration.Get<string>("email.email"), Configuration.Get<string>("email.password")),
                EnableSsl = Configuration.Get<bool>("email.enableSSL")
            };
            client.Send(Configuration.Get<string>("email.email"), recipient, subject, body);
        }
    }
}
