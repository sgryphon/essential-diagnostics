using Microsoft.Extensions.Logging;
using System;

namespace ExtensionsLoggingNetCore2
{
    internal class CustomLogger : ILogger
    {
        private readonly string _categoryName;

        public CustomLogger(string categoryName)
        {
            Console.WriteLine("** [{0}]create_logger **", categoryName);
            _categoryName = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            // .NET classes, e.g. ConsoleLogger, user LoggerExternalScopeProvider, which stores in AsyncLocal (so, cross-threads)
            Console.WriteLine("** [{0}]begin_scope<{1}> State=[{2}]'{3}' **",
                _categoryName, typeof(TState), state.GetType(), state);
            return new NullScope<TState>(_categoryName, state);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var formatted = formatter(state, exception);
            Console.WriteLine("** [{0}]log<{1}> Level={2} Id={3} State=[{4}]'{5}' HasException={6} Message='{7}' **",
                _categoryName, typeof(TState), logLevel, eventId, state.GetType(), state, (exception != null), formatted);
        }

        private class NullScope<T> : IDisposable
        {
            private readonly string _categoryName;
            private readonly T _state;

            public NullScope(string categoryName, T state)
            {
                _categoryName = categoryName;
                _state = state;
            }

            public void Dispose()
            {
                Dispose(true);
            }

            protected virtual void Dispose(bool disposing)
            {
                Console.WriteLine("** [0]dispose_scope<{1}> State=[{2}]'{3}' **",
                    _categoryName, typeof(T), _state.GetType(), _state);
            }
        }
    }
}