using System;

namespace PrjFunNowWebApi.Services
{
    public class LoggerService : ILoggerService
    {
        public void LogInformation(string message)
        {
            Console.WriteLine($"[INFO] {message}");
        }

        public void LogError(string message, Exception exception = null)
        {
            if (exception != null)
            {
                Console.WriteLine($"[ERROR] {message}: {exception.Message}");
            }
            else
            {
                Console.WriteLine($"[ERROR] {message}");
            }
        }
    }
}
