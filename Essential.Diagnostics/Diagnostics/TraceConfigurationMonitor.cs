using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Essential.Diagnostics
{
    /// <summary>
    /// Monitors the application configuration file and refreshes the
    /// tracing configuration when the file is changed.
    /// </summary>
    public class TraceConfigurationMonitor : IDisposable
    {
        private FileSystemWatcher watcher;
             
        /// <summary>
        /// Constructor. Monitors the configuration file based on the entry assembly.
        /// </summary>
        public TraceConfigurationMonitor()
            : this(Assembly.GetEntryAssembly().Location + ".config")
        {
        }

        /// <summary>
        /// Constructor. Monitors the specified file (monitoring enabled).
        /// </summary>
        public TraceConfigurationMonitor(string configFilePath)
            : this (configFilePath, true)
        {
        }

        /// <summary>
        /// Constructor. Configured to monitors the specified file,
        /// enabled or not as specified.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public TraceConfigurationMonitor(string configFilePath, bool enabled)
        {
            if (configFilePath == null) throw new ArgumentNullException("configFilePath");
//            Console.WriteLine("** config: {0} **", configFilePath);

            var path = Path.GetDirectoryName(configFilePath);
            var fileName = Path.GetFileName(configFilePath);
            watcher = new FileSystemWatcher(path, fileName);
            watcher.Changed += watcher_Changed;
            watcher.EnableRaisingEvents = enabled;
        }

        /// <summary>
        /// Gets or sets whether changes should be monitored.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public bool Enabled
        {
            get { return watcher.EnableRaisingEvents; }
            set { watcher.EnableRaisingEvents = value; }
        }

        /// <summary>
        /// Start monitoring changes.
        /// </summary>
        public void Start()
        {
            Enabled = true;
        }

        /// <summary>
        /// Stop monitoring changes.
        /// </summary>
        public void Stop()
        {
            Enabled = false;
        }

        /// <summary>
        /// Disposes of the object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of the object.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (watcher != null)
                {
                    watcher.Dispose();
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void watcher_Changed(object sender, FileSystemEventArgs e)
        {
//            Console.WriteLine("** watcher_Changed fired **");
            int retryCount = 0;
            while (retryCount < 3)
            {
                try
                {
                    Trace.Refresh();
                    break;
                }
                catch (Exception ex)
                {
                    // This is at the top of user code -- need to catch unhandled
                    // exceptions otherwise the application will crash.
                    // Can't rely on tracing (it's the bit that failed),
                    // so write to the error stream.
                    Console.Error.WriteLine(ex.ToString());
                    // TODO: Consider logging direct to Windows Event Log, if possible.
                    retryCount++;
                    Thread.Sleep(1);
                }
            }
        }
    }
}
