using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Essential.Diagnostics.Tests.Utility
{
    public class FileResetScope : IDisposable
    {
        private string originalText;
        private string path;

        public FileResetScope(string path)
        {
            this.path = path;
            originalText = File.ReadAllText(path);
        }

        public string OriginalText { get { return this.originalText; } }

        public string Path { get { return this.path; } }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                File.WriteAllText(this.path, this.originalText);
            }
        }
    }
}
