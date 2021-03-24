﻿using System;
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
        private bool _newStreamOnError;

        public RollingTextWriter(string filePathTemplate, bool newStreamOnError)
        {
            _filePathTemplate = filePathTemplate;
            _newStreamOnError = newStreamOnError;
        }

        /// <summary>
        /// Create RollingTextWriter with filePathTemplate which might contain 1 environment variable in front.
        /// </summary>
        /// <param name="filePathTemplate"></param>
        /// <returns></returns>
        public static RollingTextWriter Create(string filePathTemplate, bool newStreamOnError)
        {
            var segments = filePathTemplate.Split('%');
            if (segments.Length > 3)
            {
                throw new ArgumentException("InitializeData should contain maximum 1 environment variable.", "filePathTemplate");
            }
            else if (segments.Length == 3)
            {
                var variableName = segments[1];
                var rootFolder = Environment.GetEnvironmentVariable(variableName);
                if (String.IsNullOrEmpty(rootFolder))
                {
                    if (variableName.Equals("ProgramData", StringComparison.CurrentCultureIgnoreCase) && (Environment.OSVersion.Version.Major <= 5))//XP or below: https://msdn.microsoft.com/en-us/library/windows/desktop/ms724832%28v=vs.85%29.aspx
                    {//So the host program could run well in XP and Windows 7 without changing the config file.
                        rootFolder = Path.Combine(Environment.GetEnvironmentVariable("AllUsersProfile"), "Application Data");
                    }
                    else
                    {
                        throw new ArgumentException("Environment variable is not recognized in InitializeData.", "filePathTemplate");
                    }
                }
                var filePath = rootFolder + segments[2];
                return new RollingTextWriter(filePath, newStreamOnError);
            }

            return new RollingTextWriter(filePathTemplate, newStreamOnError);

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
                    try
                    {
                        _currentWriter.Flush();
                    }
                    catch
                    {
                        if (_newStreamOnError)
                        {
                            DestroyCurrentWriter();
                        }
                        throw;
                    }
                }
            }
        }

        public void Write(TraceEventCache eventCache, string value)
        {
            string filePath = GetCurrentFilePath(eventCache);
            lock (_fileLock)
            {
                EnsureCurrentWriter(filePath);
                try
                {
                    _currentWriter.Write(value);
                }
                catch
                {
                    if(_newStreamOnError)
                    {
                        DestroyCurrentWriter();
                    }
                    throw;
                }
            }
        }

        public void WriteLine(TraceEventCache eventCache, string value)
        {
            string filePath = GetCurrentFilePath(eventCache);
            lock (_fileLock)
            {
                EnsureCurrentWriter(filePath);
                try
                {
                    _currentWriter.WriteLine(value);
                }
                catch
                {
                    if (_newStreamOnError)
                    {
                        DestroyCurrentWriter();
                    }
                    throw;
                }
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
                    _currentWriter.Dispose();
                    _currentWriter = null;
                    _currentPath = null;
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

                throw new InvalidOperationException(Resource_RollingFile.RollingTextWriter_ExhaustedLogfileNames);
            }
        }

        private void DestroyCurrentWriter()
        {
            // NOTE: This is called inside lock(_fileLock)
            if (_currentWriter != null)
            {
                _currentWriter.Close();
                _currentWriter.Dispose();
                _currentWriter = null;
                _currentPath = null;
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
                        case "ACTIVITYID":
                            value = Trace.CorrelationManager.ActivityId;
                            break;
                        case "APPDATA":
                            value = traceFormatter.HttpTraceContext.AppDataPath;
                            break;
                        case "APPDOMAIN":
                            value = AppDomain.CurrentDomain.FriendlyName;
                            break;
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
                        case "USER":
                            value = Environment.UserDomainName + "-" + Environment.UserName;
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
