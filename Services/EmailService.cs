using MailKit.Net.Smtp;
using MimeKit;
namespace PrjFunNowWebApi.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration["Email:From"]));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;
            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = body
            };

            using var smtp = new SmtpClient();
            int port;
            if (int.TryParse(_configuration["Email:Port"], out port))
            {
                await smtp.ConnectAsync(_configuration["Email:Host"], port, MailKit.Security.SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_configuration["Email:Username"], _configuration["Email:Password"]);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            else
            {
                // 处理无法解析端口号的情况
                throw new Exception($"无效的端口号: {_configuration["Email:Port"]}");
            }
        }
    }
}
