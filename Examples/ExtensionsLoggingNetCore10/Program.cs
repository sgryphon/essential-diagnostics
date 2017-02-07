using Microsoft.Extensions.Logging;
using System;

namespace ExtensionsLoggingNetCore10
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Microsoft.Extensions.Logging (.NET Core)");

            ILoggerFactory loggerFactory = new LoggerFactory();
            loggerFactory.AddConsole();

            ILogger<Program> logger = loggerFactory.CreateLogger<Program>();

            logger.LogInformation(2001, "Test message: {0}", Guid.NewGuid());
            try
            {
                throw new NotSupportedException("Test exception");
            }
            catch (Exception ex)
            {
                logger.LogError(5001, ex, "Error occurred: {0}", new Random().Next());
            }

            Console.ReadLine();
        }
    }
}
