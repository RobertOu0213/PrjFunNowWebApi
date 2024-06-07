using System;

namespace PrjFunNowWebApi.Services
{
    public interface ILoggerService
    {
        void LogInformation(string message);
        void LogError(string message, Exception exception = null);
    }
}
