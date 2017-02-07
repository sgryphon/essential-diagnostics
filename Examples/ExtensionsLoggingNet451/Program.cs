using Microsoft.Extensions.Logging;
using System;

namespace ExtensionsLoggingNet451
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Microsoft.Extensions.Logging (.NET 4.5.1)");

            ILoggerFactory loggerFactory = new LoggerFactory();
            // loggerFactory.AddConsole();
            loggerFactory.AddTraceSource("Example.Switch");

            ILogger<Program> logger1 = loggerFactory.CreateLogger<Program>();
            logger1.LogInformation(2001, "Test message: {0}", Guid.NewGuid());
            try
            {
                throw new NotSupportedException("Test exception");
            }
            catch (Exception ex)
            {
                logger1.LogError(5001, ex, "Error occurred: {0}", new Random().Next());
            }

            ILogger logger2 = loggerFactory.CreateLogger("ExtensionsLoggingNet451.Foo.Bar");
            ILogger logger3 = loggerFactory.CreateLogger("ExtensionsLoggingNet451.Foo.Waz");

            logger2.LogInformation(3001, "Information to Foo.Bar - should not display");
            logger2.LogWarning(4001, "Warning to Foo.Bar");
            logger3.LogInformation(3002, "Information to Foo.Waz");
            logger3.LogWarning(4002, "Warning to Foo.Waz");

            Console.ReadLine();
        }
    }
}
