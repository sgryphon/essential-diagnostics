using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Diagnostics;
using Essential.IO;
using System.Threading;

namespace Essential.Diagnostics
{
    /// <summary>
    /// Trace listener that writes E2ETraceEvent XML fragments to a text file, rolling to a new file based on a filename template (usually including the date).
    /// </summary>
    /// <remarks>
    /// <para>
    /// The E2ETraceEvent XML fragment format can be read by the Service Trace Viewer tool.
    /// </para>
    /// <para>
    /// A rolling log file is achieved by including the date in the filename, so that when the date changes
    /// a different file is used.
    /// </para>
    /// <para>
    /// Available tokens are DateTime (a UTC DateTimeOffset) and LocalDateTime (a local DateTimeOffset), 
    /// as well as ApplicationName, ProcessId, ProcessName and MachineName. These use standard .NET 
    /// format strings, e.g. "Trace{DateTime:yyyyMMddTHH}.svclog" would generate a different log name
    /// each hour.
    /// </para>
    /// <para>
    /// The default filePathTemplate is "{ApplicationName}-{DateTime:yyyy-MM-dd}.svclog", which matches
    /// the format used by Microsoft.VisualBasic.Logging.FileLogTraceListener (except that it uses
    /// UTC time instead of local time).
    /// </para>
    /// </remarks>
    public class RollingXmlTraceListener : TraceListenerBase
    {
        private readonly string machineName = Environment.MachineName;
        // Default format matches Microsoft.VisualBasic.Logging.FileLogTraceListener
        private const string _defaultFilePathTemplate = "{ApplicationName}-{DateTime:yyyy-MM-dd}.svclog";
        private static string[] _supportedAttributes = new string[] 
            { 
            };

        private RollingTextWriter rollingTextWriter;

        /// <summary>
        /// Constructor. Writes to a rolling text file using the default name.
        /// </summary>
        public RollingXmlTraceListener()
            : this(_defaultFilePathTemplate)
        {
        }

        /// <summary>
        /// Constructor with initializeData.
        /// </summary>
        /// <param name="filePathTemplate">Template filename to log to; may use replacement parameters.</param>
        /// <remarks>
        /// <para>
        /// A rolling log file is achieved by including the date in the filename, so that when the date changes
        /// a different file is used.
        /// </para>
        /// <para>
        /// Available tokens are DateTime (a UTC DateTimeOffset) and LocalDateTime (a local DateTimeOffset), 
        /// as well as ApplicationName, ProcessId, ProcessName and MachineName. These use standard .NET 
        /// format strings, e.g. "Trace{DateTime:yyyyMMddTHH}.svclog" would generate a different log name
        /// each hour.
        /// </para>
        /// <para>
        /// The default filePathTemplate is "{ApplicationName}-{DateTime:yyyy-MM-dd}.svclog", which matches
        /// the format used by Microsoft.VisualBasic.Logging.FileLogTraceListener (except that it uses
        /// UTC time instead of local time).
        /// </para>
        /// <para>
        /// To get behaviour that exactly matches FileLogTraceListener, 
        /// use "{ApplicationName}-{LocalDateTime:yyyy-MM-dd}.svclog".
        /// </para>
        /// </remarks>
        public RollingXmlTraceListener(string filePathTemplate)
        {
            if (string.IsNullOrEmpty(filePathTemplate))
            {
                rollingTextWriter = new RollingTextWriter(_defaultFilePathTemplate);
            }
            else
            {
                rollingTextWriter = new RollingTextWriter(filePathTemplate);
            }
        }

        /// <summary>
        /// Gets or sets the file system to use; this defaults to an adapter for System.IO.File.
        /// </summary>
        public IFileSystem FileSystem
        {
            get { return rollingTextWriter.FileSystem; }
            set { rollingTextWriter.FileSystem = value; }
        }

        /// <summary>
        /// Gets whether the listener internally handles thread safety
        /// (or if the System.Diagnostics framework needs to co-ordinate threading).
        /// </summary>
        public override bool IsThreadSafe
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the template for the rolling file name.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value is part of initializeData; if the value changes the
        /// listener is recreated. See the constructor parameter for details
        /// of the supported formats.
        /// </para>
        /// </remarks>
        public string FilePathTemplate
        {
            get { return rollingTextWriter.FilePathTemplate; }
        }

        /// <summary>
        /// Flushes the output buffer.
        /// </summary>
        public override void Flush()
        {
            rollingTextWriter.Flush();
        }

        /// <summary>
        /// Allowed attributes for this trace listener.
        /// </summary>
        protected override string[] GetSupportedAttributes()
        {
            return _supportedAttributes;
        }

        /// <summary>
        /// Write trace event with data.
        /// </summary>
        protected override void WriteTrace(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message, Guid? relatedActivityId, object[] data)
        {
            var output = new StringBuilder();
            AppendHeader(output, source, eventType, id, eventCache, relatedActivityId);
            if (message != null)
            {
                AppendEscaped(output, message);
            }

            if (data != null)
            {
                output.Append("<TraceData>");
                for (int i = 0; i < data.Length; i++)
                {
                    output.Append("<DataItem>");
                    if (data[i] != null)
                    {
                        AppendData(output, data[i]);
                    }
                    output.Append("</DataItem>");
                }
                output.Append("</TraceData>");
            }

            AppendFooter(output, eventCache);

            rollingTextWriter.WriteLine(eventCache, output.ToString());
        }

        private static void AppendData(StringBuilder output, object data)
        {
            XPathNavigator xPathNavigator = data as XPathNavigator;
            if (xPathNavigator == null)
            {
                AppendEscaped(output, data.ToString());
                return;
            }
            var xmlBlob = new StringBuilder();
            using (var xmlBlobWriter = new XmlTextWriter(new StringWriter(xmlBlob, CultureInfo.CurrentCulture)))
            {
                try
                {
                    xPathNavigator.MoveToRoot();
                    xmlBlobWriter.WriteNode(xPathNavigator, false);
                    output.Append(xmlBlob.ToString());
                }
                catch (Exception)
                {
                    output.Append(data.ToString());
                }
            }
        }

        private void AppendHeader(StringBuilder output, string source, TraceEventType eventType, int id, TraceEventCache eventCache, Guid? relatedActivityId)
        {
            AppendStartHeader(output, source, eventType, id, eventCache);
            if (relatedActivityId.HasValue)
            {
                output.Append("\" RelatedActivityID=\"");
                output.Append(relatedActivityId.Value.ToString("B"));
            }
            AppendEndHeader(output, eventCache);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        private static void AppendStartHeader(StringBuilder output, string source, TraceEventType eventType, int id, TraceEventCache eventCache)
        {
            output.Append("<E2ETraceEvent xmlns=\"http://schemas.microsoft.com/2004/06/E2ETraceEvent\"><System xmlns=\"http://schemas.microsoft.com/2004/06/windows/eventlog/system\">");
            output.Append("<EventID>");
            uint num = (uint)id;
            output.Append(num.ToString(CultureInfo.InvariantCulture));
            output.Append("</EventID>");
            output.Append("<Type>3</Type>");
            output.Append("<SubType Name=\"");
            output.Append(eventType.ToString());
            output.Append("\">0</SubType>");
            output.Append("<Level>");
            int num2 = (int)eventType;
            if (num2 > 255)
            {
                num2 = 255;
            }
            if (num2 < 0)
            {
                num2 = 0;
            }
            output.Append(num2.ToString(CultureInfo.InvariantCulture));
            output.Append("</Level>");
            output.Append("<TimeCreated SystemTime=\"");
            if (eventCache != null)
            {
                // Cast to DateTimeOffset first, to get consistent behaviour of ToUniversal/ToLocal
                // (i.e. Unspecified Kind is treated as Local)
                var dateTimeOffset = ((DateTimeOffset)eventCache.DateTime).ToUniversalTime();
                output.Append(dateTimeOffset.DateTime.ToString("o", CultureInfo.InvariantCulture));
            }
            else
            {
                var now = DateTimeOffset.UtcNow;
                output.Append(now.DateTime.ToString("o", CultureInfo.InvariantCulture));
            }
            output.Append("\" />");
            output.Append("<Source Name=\"");
            AppendEscaped(output, source);
            output.Append("\" />");
            output.Append("<Correlation ActivityID=\"");
            if (eventCache != null)
            {
                Guid activityId = Trace.CorrelationManager.ActivityId;
                output.Append(activityId.ToString("B"));
                return;
            }
            Guid empty = Guid.Empty;
            output.Append(empty.ToString("B"));
        }

        private void AppendEndHeader(StringBuilder output, TraceEventCache eventCache)
        {
            output.Append("\" />");
            output.Append("<Execution ProcessName=\"");
            output.Append(TraceFormatter.FormatProcessName());
            output.Append("\" ProcessID=\"");
            uint processId = (uint)(int)TraceFormatter.FormatProcessId(eventCache);
            output.Append(processId.ToString(CultureInfo.InvariantCulture));
            output.Append("\" ThreadID=\"");
            if (eventCache != null)
            {
                AppendEscaped(output, eventCache.ThreadId.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                int threadId = Thread.CurrentThread.ManagedThreadId;
                AppendEscaped(output, threadId.ToString(CultureInfo.InvariantCulture));
            }
            output.Append("\" />");
            output.Append("<Channel/>");
            output.Append("<Computer>");
            output.Append(this.machineName);
            output.Append("</Computer>");
            output.Append("</System>");
            output.Append("<ApplicationData>");
        }

        private void AppendFooter(StringBuilder output, TraceEventCache eventCache)
        {
            bool flag = (TraceOutputOptions & TraceOptions.LogicalOperationStack) == TraceOptions.LogicalOperationStack;
            bool flag2 = (TraceOutputOptions & TraceOptions.Callstack) == TraceOptions.Callstack;

            if (eventCache != null && (flag || flag2))
            {
                output.Append("<System.Diagnostics xmlns=\"http://schemas.microsoft.com/2004/08/System.Diagnostics\">");
                if (flag)
                {
                    output.Append("<LogicalOperationStack>");
                    Stack logicalOperationStack = eventCache.LogicalOperationStack;
                    if (logicalOperationStack != null)
                    {
                        IEnumerator enumerator = logicalOperationStack.GetEnumerator();
                        try
                        {
                            while (enumerator.MoveNext())
                            {
                                object current = enumerator.Current;
                                output.Append("<LogicalOperation>");
                                AppendEscaped(output, current.ToString());
                                output.Append("</LogicalOperation>");
                            }
                        }
                        finally
                        {
                            IDisposable disposable = enumerator as IDisposable;
                            if (disposable != null)
                            {
                                disposable.Dispose();
                            }
                        }
                    }
                    output.Append("</LogicalOperationStack>");
                }
                output.Append("<Timestamp>");
                long timestamp = eventCache.Timestamp;
                output.Append(timestamp.ToString(CultureInfo.InvariantCulture));
                output.Append("</Timestamp>");
                if (flag2)
                {
                    output.Append("<Callstack>");
                    AppendEscaped(output, eventCache.Callstack);
                    output.Append("</Callstack>");
                }
                output.Append("</System.Diagnostics>");
            }
            output.Append("</ApplicationData></E2ETraceEvent>");
        }

        private static void AppendEscaped(StringBuilder output, string str)
        {
            if (str == null)
            {
                return;
            }
            int num = 0;
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                if (c <= '\r')
                {
                    if (c != '\n')
                    {
                        if (c == '\r')
                        {
                            output.Append(str.Substring(num, i - num));
                            output.Append("&#xD;");
                            num = i + 1;
                        }
                    }
                    else
                    {
                        output.Append(str.Substring(num, i - num));
                        output.Append("&#xA;");
                        num = i + 1;
                    }
                }
                else
                {
                    if (c != '"')
                    {
                        switch (c)
                        {
                            case '&':
                                {
                                    output.Append(str.Substring(num, i - num));
                                    output.Append("&amp;");
                                    num = i + 1;
                                    break;
                                }
                            case '\'':
                                {
                                    output.Append(str.Substring(num, i - num));
                                    output.Append("&apos;");
                                    num = i + 1;
                                    break;
                                }
                            default:
                                {
                                    switch (c)
                                    {
                                        case '<':
                                            {
                                                output.Append(str.Substring(num, i - num));
                                                output.Append("&lt;");
                                                num = i + 1;
                                                break;
                                            }
                                        case '>':
                                            {
                                                output.Append(str.Substring(num, i - num));
                                                output.Append("&gt;");
                                                num = i + 1;
                                                break;
                                            }
                                    }
                                    break;
                                }
                        }
                    }
                    else
                    {
                        output.Append(str.Substring(num, i - num));
                        output.Append("&quot;");
                        num = i + 1;
                    }
                }
            }
            output.Append(str.Substring(num, str.Length - num));
        }
    }
}
