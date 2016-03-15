using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Essential.Diagnostics.Tests.Utility
{
    class MockConsole : IConsole
    {
        public MockConsole()
        {
            OutWriter = new StringWriter();
            ErrorWriter = new StringWriter();
            ForegroundColorSet = new List<ConsoleColor>();
        }

        public StringWriter ErrorWriter { get; set; }

        public StringWriter OutWriter { get; set; }

        public IList<ConsoleColor> ForegroundColorSet { get; set; }

        public int ResetColorCount { get; set; }

        public ConsoleColor ForegroundColor
        {
            get { throw new NotImplementedException(); }
            set { ForegroundColorSet.Add(value); }
        }

        public TextWriter Out
        {
            get { return OutWriter; }
        }

        public TextWriter Error
        {
            get { return ErrorWriter; }
        }

        public void ResetColor()
        {
            ResetColorCount++;
        }
    }
}
