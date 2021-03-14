using Microsoft.Extensions.Logging;

namespace ExtensionsLoggingNetCore2
{
    internal class CustomProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new CustomLogger(categoryName);
        }

        public void Dispose()
        {
        }
    }
}