using System;
using System.IO;

namespace Essential
{
    /// <summary>
    /// Interface representing the standard console.
    /// </summary>
    public interface IConsole
    {
        /// <summary>
        /// Gets the standard error output stream.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Error")]
        TextWriter Error { get; }

        /// <summary>
        /// Gets or sets the foreground color of the console.
        /// </summary>
        ConsoleColor ForegroundColor { get;  set; }

        /// <summary>
        /// Gets the standard output stream.
        /// </summary>
        TextWriter Out { get; }

        /// <summary>
        /// Sets the foreground and background console colors to their defaults.
        /// </summary>
        void ResetColor();
    }
}
