using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Threading.Tasks;

namespace PrjFunNowWebApi.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IKeyVaultService _keyVaultService; // 增加了私有字段 _keyVaultService

        public EmailService(IConfiguration configuration, IKeyVaultService keyVaultService) // 注入 IKeyVaultService
        {
            _configuration = configuration;
            _keyVaultService = keyVaultService; // 将参数 keyVaultService 赋值给 _keyVaultService
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
            if (int.TryParse(_configuration["Email:Port"], out int port))
            {
                try
                {
                    // 使用 _keyVaultService 获取密码
                    string password = await _keyVaultService.GetSecretAsync("Tingsystem-secretnumber");

                    await smtp.ConnectAsync(_configuration["Email:Host"], port, MailKit.Security.SecureSocketOptions.StartTls);
                    await smtp.AuthenticateAsync(_configuration["Email:Username"], password);
                    await smtp.SendAsync(email);
                    await smtp.DisconnectAsync(true);
                }
                catch (Azure.RequestFailedException ex)
                {
                    throw new Exception($"Key Vault 访问错误: {ex.Message}", ex);
                }
            }
            else
            {
                throw new Exception($"无效的端口号: {_configuration["Email:Port"]}");
            }
        }
    }
}
