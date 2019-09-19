using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExtensionsLoggingNetCore2
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Console.WriteLine("Microsoft.Extensions.Logging (.NET Core 2)");

            var hostBuilder = new HostBuilder()
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole(configure => configure.IncludeScopes = true);
                    logging.AddProvider(new CustomProvider());
                });

            using (var host = hostBuilder.Build())
            {
                ILogger logger = host.Services.GetService<ILogger<Program>>();

                logger.LogInformation(2001, "Test message: {MessageId}", Guid.NewGuid());
                using (var scope = logger.BeginScope("Session {SessionId}", 12345))
                {
                    var data = new Dictionary<string, int>() { { "a", 1 }, { "b", 2 } };
                    using (var scope2 = logger.BeginScope(data))
                    {
                        logger.LogInformation(2002, "Inner message: {InnerId}", Guid.NewGuid());
                    }
                    try
                    {
                        throw new NotSupportedException("Test exception");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(5001, ex, "Error occurred: {Random}", new Random().Next());
                    }
                }

                Console.ReadLine();
            }
        }
    }
}