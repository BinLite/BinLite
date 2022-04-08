using MailKit.Net.Smtp;
using MailKit.Security;

using MimeKit;
using MimeKit.Text;

namespace BinLiteServer
{
    public static class EmailManager
    {
        public static void Send(User recipient, string subject, string body)
        {
            var template = File.ReadAllText(Configuration.Get<string>("email.template"));
            template = template
                .Replace("@content", body.Replace("\n", "<br>"))
                .Replace("@subject", subject)
                .Replace("@url", Configuration.Get<string>("email.siteUrl"));

            var sender = Configuration.Get<string>("email.email");

            var message = new MimeMessage()
            {
                Subject = subject,
                Body = new TextPart(TextFormat.Html)
                {
                    Text = template,
                }
            };
            message.From.Add(new MailboxAddress(Configuration.Get<string>("email.from"), sender));
            message.To.Add(new MailboxAddress(recipient.Username, recipient.Email));

            using var client = new SmtpClient();
            client.Connect(Configuration.Get<string>("email.smtpUrl"), Configuration.Get<int>("email.port"), SecureSocketOptions.StartTls);
            client.Authenticate(sender, Configuration.Get<string>("email.password"));
            client.Send(message);
            client.Disconnect(true);
        }
    }
}
