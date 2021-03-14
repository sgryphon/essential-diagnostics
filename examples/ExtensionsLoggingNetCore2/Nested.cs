using Microsoft.Extensions.Logging;

namespace ExtensionsLoggingNetCore2
{
    internal class Nested
    {
        private readonly ILogger _logger;

        public Nested(ILogger<Nested> logger)
        {
            _logger = logger;
        }

        public void Operation()
        {
            _logger.LogInformation("Nested operation");
        }
    }
}