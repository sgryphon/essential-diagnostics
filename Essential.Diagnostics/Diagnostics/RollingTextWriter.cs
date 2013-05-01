using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Essential.IO;

namespace Essential.Diagnostics
{
    class RollingTextWriter : IDisposable
    {
        const int _maxStreamRetries = 5;

        private string _currentPath;
        private TextWriter _currentWriter;
        private object _fileLock = new object();
        private string _filePathTemplate;
        private IFileSystem _fileSystem = new FileSystem();
        TraceFormatter traceFormatter = new TraceFormatter();

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
                if (_currentWriter != null)
                {
                    _currentWriter.Flush();
                }
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

                var num = 0;
                var stream = default(Stream);

                while (stream == null && num < _maxStreamRetries)
                {
                    var fullPath = num == 0 ? path : getFullPath(path, num);
                    try
                    {
                        stream = FileSystem.Open(fullPath, FileMode.Append, FileAccess.Write, FileShare.Read);

                        this._currentWriter = new StreamWriter(stream);
                        this._currentPath = path;

                        return;
                    }
                    catch (DirectoryNotFoundException)
                    {
                        throw;
                    }
                    catch (IOException)
                    {

                    }
                    num++;
                }

                throw new InvalidOperationException(Resource.RollingTextWriter_ExhaustedLogfileNames);
            }
        }

        static string getFullPath(string path, int num)
        {
            var extension = Path.GetExtension(path);
            return path.Insert(path.Length - extension.Length, "-" + num.ToString(CultureInfo.InvariantCulture));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1903:UseOnlyApiFromTargetedFramework", MessageId = "System.DateTimeOffset", Justification = "Deliberate dependency, .NET 2.0 SP1 required.")]
        private string GetCurrentFilePath(TraceEventCache eventCache)
        {
            var result = StringTemplate.Format(CultureInfo.CurrentCulture, FilePathTemplate,
                delegate(string name, out object value)
                {
                    switch (name.ToUpperInvariant())
                    {
                        case "APPLICATIONNAME":
                            value = traceFormatter.FormatApplicationName();
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
                            value = traceFormatter.FormatProcessId(eventCache);
                            break;
                        case "PROCESSNAME":
                            value = traceFormatter.FormatProcessName();
                            break;
                        case "APPDATA":
                            value = traceFormatter.HttpTraceContext.AppDataPath;
                            break;
                        default:
                            value = "{" + name + "}";
                            return true;
                    }
                    return true;
                });
            return result;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_currentWriter != null)
                {
                    _currentWriter.Dispose();
                }
            }
        }
    }
}
