using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
