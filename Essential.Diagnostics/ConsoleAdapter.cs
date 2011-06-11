using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Essential
{
    /// <summary>
    /// Adapter that wraps System.Console in an interface, allowing it to be substituted.
    /// </summary>
    public class ConsoleAdapter : IConsole
    {
        /// <summary>
        /// Gets the standard error output stream.
        /// </summary>
        public TextWriter Error
        {
            get { return Console.Error; }
        }

        /// <summary>
        /// Gets or sets the foreground color of the console.
        /// </summary>
        public ConsoleColor ForegroundColor
        {
            get { return Console.ForegroundColor; }
            set { Console.ForegroundColor = value; }
        }

        /// <summary>
        /// Gets the standard output stream.
        /// </summary>
        public TextWriter Out
        {
            get { return Console.Out; }
        }

        /// <summary>
        /// Sets the foreground and background console colors to their defaults.
        /// </summary>
        public void ResetColor()
        {
            Console.ResetColor();
        }
    }
}
