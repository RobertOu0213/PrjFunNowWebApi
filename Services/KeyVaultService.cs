using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using DotNetEnv;

namespace PrjFunNowWebApi.Services
{
    public class KeyVaultService : IKeyVaultService
    {
        private readonly IConfiguration _configuration;
        private readonly SecretClient _client;

        public KeyVaultService(IConfiguration configuration)
        {
            Env.Load();
            string clientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");
            _configuration = configuration;
            var clientSecretCredential = new ClientSecretCredential(
                _configuration["AzureAD:TenantId"],
                _configuration["AzureAD:ClientId"],
                clientSecret);

            _client = new SecretClient(new Uri(_configuration["KeyVault:BaseUrl"]), clientSecretCredential);
        }

        public async Task<string> GetSecretAsync(string secretName)
        {
            try
            {
                KeyVaultSecret secret = await _client.GetSecretAsync(secretName);
                return secret.Value;
            }
            catch (Azure.RequestFailedException ex)
            {
                throw new Exception($"Key Vault 访问错误: {ex.Message}", ex);
            }
        }
    }

    public interface IKeyVaultService
    {
        Task<string> GetSecretAsync(string secretName);
    }
}
