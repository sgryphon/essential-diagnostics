using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Essential.IO;
using System.Diagnostics;
using System.Globalization;

namespace Essential.Diagnostics
{
    class RollingTextWriter
    {
        private string _currentPath;
        private TextWriter _currentWriter;
        private object _fileLock = new object();
        private string _filePathTemplate;
        private IFileSystem _fileSystem = new FileSystem();

        public RollingTextWriter(string filePathTemplate)
        {
            _filePathTemplate = filePathTemplate;
        }

        public string FilePathTemplate
        {
            get { return _filePathTemplate; }
        }

        public IFileSystem FileSystem
        {
            get { return _fileSystem; }
            set
            {
                lock (_fileLock)
                {
                    _fileSystem = value;
                }
            }
        }

        public void Flush()
        {
            lock (_fileLock)
            {
                _currentWriter.Flush();
            }
        }

        public void Write(TraceEventCache eventCache, string value)
        {
            string filePath = GetCurrentFilePath(eventCache);
            lock (_fileLock)
            {
                EnsureCurrentWriter(filePath);
                _currentWriter.Write(value);
            }
        }

        public void WriteLine(TraceEventCache eventCache, string value)
        {
            string filePath = GetCurrentFilePath(eventCache);
            lock (_fileLock)
            {
                EnsureCurrentWriter(filePath);
                _currentWriter.WriteLine(value);
            }
        }

        private void EnsureCurrentWriter(string path)
        {
            // NOTE: This is called inside lock(_fileLock)
            if (_currentPath != path)
            {
                if (_currentWriter != null)
                {
                    _currentWriter.Close();
                }
                var stream = FileSystem.Open(path, FileMode.Append, FileAccess.Write, FileShare.None);
                _currentWriter = new StreamWriter(stream);
                _currentPath = path;
            }
        }

        private string GetCurrentFilePath(TraceEventCache eventCache)
        {
            var result = StringTemplate.Format(CultureInfo.CurrentCulture, FilePathTemplate,
                delegate(string name, out object value)
                {
                    switch (name.ToUpperInvariant())
                    {
                        case "APPLICATIONNAME":
                            value = TraceFormatter.FormatApplicationName();
                            break;
                        case "DATETIME":
                        case "UTCDATETIME":
                            value = TraceFormatter.FormatUniversalTime(eventCache);
                            break;
                        case "LOCALDATETIME":
                            value = TraceFormatter.FormatLocalTime(eventCache);
                            break;
                        case "MACHINENAME":
                            value = Environment.MachineName;
                            break;
                        case "PROCESSID":
                            value = TraceFormatter.FormatProcessId(eventCache);
                            break;
                        case "PROCESSNAME":
                            value = TraceFormatter.FormatProcessName();
                            break;
                        default:
                            value = "{" + name + "}";
                            return true;
                    }
                    return true;
                });
            return result;
        }

    }
}
