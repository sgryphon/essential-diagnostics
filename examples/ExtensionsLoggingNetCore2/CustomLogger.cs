using Microsoft.Extensions.Logging;
using System;

namespace ExtensionsLoggingNetCore2
{
    internal class CustomLogger : ILogger
    {
        private readonly string _categoryName;

        public CustomLogger(string categoryName)
        {
            Console.WriteLine("* create_logger Category='{0}'", categoryName);
            _categoryName = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            Console.WriteLine("* begin_scope<{0}> State=[{1}]'{2}'",
                typeof(TState), state.GetType(), state);
            return new NullScope();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var formatted = formatter(state, exception);
            Console.WriteLine("* log<{0}> Level={1} Id={2} State=[{3}]'{4}' HasException={5} Message='{6}'",
                typeof(TState), logLevel, eventId, state.GetType(), state, (exception != null), formatted);
        }

        private class NullScope : IDisposable
        {
            public void Dispose()
            {
                Dispose(true);
            }

            protected virtual void Dispose(bool disposing)
            {
                Console.WriteLine("* dispose_scope");
            }
        }
    }
}