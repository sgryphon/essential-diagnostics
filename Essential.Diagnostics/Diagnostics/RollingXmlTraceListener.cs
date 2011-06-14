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

        private string _currentPath;
        private TextWriter _currentWriter;
        private object _fileLock = new object();
        private string _filePathTemplate;
        private IFileSystem _fileSystem = new FileSystem();


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
                _filePathTemplate = _defaultFilePathTemplate;
            }
            else
            {
                _filePathTemplate = filePathTemplate;
            }
        }

        /// <summary>
        /// Gets or sets the file system to use; this defaults to an adapter for System.IO.File.
        /// </summary>
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
            get { return _filePathTemplate; }
        }

        /// <summary>
        /// Flushes the output buffer.
        /// </summary>
        public override void Flush()
        {
            lock (_fileLock)
            {
                _currentWriter.Flush();
            }
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
            var filePath = GetCurrentFilePath(eventCache);

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

            WriteOutputToFile(filePath, output.ToString());
        }

        private string GetCurrentFilePath(TraceEventCache eventCache)
        {
            //var result = TraceFormatter.Format(FilePathTemplate, eventCache, source,
            //    eventType, id, message, relatedActivityId, data);

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

        private void WriteOutputToFile(string filePath, string output)
        {
            lock (_fileLock)
            {
                EnsureCurrentWriter(filePath);
                _currentWriter.WriteLine(output);
            }
        }

        private void EnsureCurrentWriter(string path)
        {
            // NOTE: This should be called inside lock(_fileLock)
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

        private void AppendData(StringBuilder output, object data)
        {
            XPathNavigator xPathNavigator = data as XPathNavigator;
            if (xPathNavigator == null)
            {
                AppendEscaped(output, data.ToString());
                return;
            }
            var xmlBlob = new StringBuilder();
            var xmlBlobWriter = new XmlTextWriter(new StringWriter(xmlBlob, CultureInfo.CurrentCulture));
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

        private void AppendStartHeader(StringBuilder output, string source, TraceEventType eventType, int id, TraceEventCache eventCache)
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

        private void AppendEscaped(StringBuilder output, string str)
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

        /*
		private readonly string machineName = Environment.MachineName;
		private StringBuilder strBldr;
		private XmlTextWriter xmlBlobWriter;
		private const string fixedHeader = "<E2ETraceEvent xmlns=\"http://schemas.microsoft.com/2004/06/E2ETraceEvent\"><System xmlns=\"http://schemas.microsoft.com/2004/06/windows/eventlog/system\">";

		public RollingXmlTraceListener(Stream stream) : base(stream)
		{
		}
		public RollingXmlTraceListener(Stream stream, string name) : base(stream, name)
		{
		}
		public RollingXmlTraceListener(TextWriter writer) : base(writer)
		{
		}
		public RollingXmlTraceListener(TextWriter writer, string name) : base(writer, name)
		{
		}
		public RollingXmlTraceListener(string filename) : base(filename)
		{
		}
		public RollingXmlTraceListener(string filename, string name) : base(filename, name)
		{
		}
		public override void Write(string message)
		{
			this.WriteLine(message);
		}
		public override void WriteLine(string message)
		{
			this.TraceEvent(null, SR.GetString("TraceAsTraceSource"), TraceEventType.Information, 0, message);
		}
		public override void Fail(string message, string detailMessage)
		{
			StringBuilder stringBuilder = new StringBuilder(message);
			if (detailMessage != null)
			{
				stringBuilder.Append(" ");
				stringBuilder.Append(detailMessage);
			}
			this.TraceEvent(null, SR.GetString("TraceAsTraceSource"), TraceEventType.Error, 0, stringBuilder.ToString());
		}
		public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
		{
			if (base.Filter != null && !base.Filter.ShouldTrace(eventCache, source, eventType, id, format, args))
			{
				return;
			}
			this.WriteHeader(source, eventType, id, eventCache);
			string str;
			if (args != null)
			{
				str = string.Format(CultureInfo.InvariantCulture, format, args);
			}
			else
			{
				str = format;
			}
			this.WriteEscaped(str);
			this.WriteFooter(eventCache);
		}
		public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
		{
			if (base.Filter != null && !base.Filter.ShouldTrace(eventCache, source, eventType, id, message))
			{
				return;
			}
			this.WriteHeader(source, eventType, id, eventCache);
			this.WriteEscaped(message);
			this.WriteFooter(eventCache);
		}
		public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
		{
			if (base.Filter != null && !base.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, data))
			{
				return;
			}
			this.WriteHeader(source, eventType, id, eventCache);
			this.InternalWrite("<TraceData>");
			if (data != null)
			{
				this.InternalWrite("<DataItem>");
				this.WriteData(data);
				this.InternalWrite("</DataItem>");
			}
			this.InternalWrite("</TraceData>");
			this.WriteFooter(eventCache);
		}
		public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
		{
			if (base.Filter != null && !base.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, data))
			{
				return;
			}
			this.WriteHeader(source, eventType, id, eventCache);
			this.InternalWrite("<TraceData>");
			if (data != null)
			{
				for (int i = 0; i < data.Length; i++)
				{
					this.InternalWrite("<DataItem>");
					if (data[i] != null)
					{
						this.WriteData(data[i]);
					}
					this.InternalWrite("</DataItem>");
				}
			}
			this.InternalWrite("</TraceData>");
			this.WriteFooter(eventCache);
		}
		public override void Close()
		{
			base.Close();
			if (this.xmlBlobWriter != null)
			{
				this.xmlBlobWriter.Close();
			}
			this.xmlBlobWriter = null;
			this.strBldr = null;
		}
		public override void TraceTransfer(TraceEventCache eventCache, string source, int id, string message, Guid relatedActivityId)
		{
			this.WriteHeader(source, TraceEventType.Transfer, id, eventCache, relatedActivityId);
			this.WriteEscaped(message);
			this.WriteFooter(eventCache);
		}
		private void WriteData(object data)
		{
			XPathNavigator xPathNavigator = data as XPathNavigator;
			if (xPathNavigator == null)
			{
				this.WriteEscaped(data.ToString());
				return;
			}
			if (this.strBldr == null)
			{
				this.strBldr = new StringBuilder();
				this.xmlBlobWriter = new XmlTextWriter(new StringWriter(this.strBldr, CultureInfo.CurrentCulture));
			}
			else
			{
				this.strBldr.Length = 0;
			}
			try
			{
				xPathNavigator.MoveToRoot();
				this.xmlBlobWriter.WriteNode(xPathNavigator, false);
				this.InternalWrite(this.strBldr.ToString());
			}
			catch (Exception)
			{
				this.InternalWrite(data.ToString());
			}
		}
          
		private void WriteHeader(string source, TraceEventType eventType, int id, TraceEventCache eventCache, Guid relatedActivityId)
		{
			this.WriteStartHeader(source, eventType, id, eventCache);
			this.InternalWrite("\" RelatedActivityID=\"");
			this.InternalWrite(relatedActivityId.ToString("B"));
			this.WriteEndHeader(eventCache);
		}
		private void WriteHeader(string source, TraceEventType eventType, int id, TraceEventCache eventCache)
		{
			this.WriteStartHeader(source, eventType, id, eventCache);
			this.WriteEndHeader(eventCache);
		}
		private void WriteStartHeader(string source, TraceEventType eventType, int id, TraceEventCache eventCache)
		{
			this.InternalWrite("<E2ETraceEvent xmlns=\"http://schemas.microsoft.com/2004/06/E2ETraceEvent\"><System xmlns=\"http://schemas.microsoft.com/2004/06/windows/eventlog/system\">");
			this.InternalWrite("<EventID>");
			uint num = (uint)id;
			this.InternalWrite(num.ToString(CultureInfo.InvariantCulture));
			this.InternalWrite("</EventID>");
			this.InternalWrite("<Type>3</Type>");
			this.InternalWrite("<SubType Name=\"");
			this.InternalWrite(eventType.ToString());
			this.InternalWrite("\">0</SubType>");
			this.InternalWrite("<Level>");
			int num2 = (int)eventType;
			if (num2 > 255)
			{
				num2 = 255;
			}
			if (num2 < 0)
			{
				num2 = 0;
			}
			this.InternalWrite(num2.ToString(CultureInfo.InvariantCulture));
			this.InternalWrite("</Level>");
			this.InternalWrite("<TimeCreated SystemTime=\"");
			if (eventCache != null)
			{
				DateTime dateTime = eventCache.DateTime;
				this.InternalWrite(dateTime.ToString("o", CultureInfo.InvariantCulture));
			}
			else
			{
				DateTime now = DateTime.Now;
				this.InternalWrite(now.ToString("o", CultureInfo.InvariantCulture));
			}
			this.InternalWrite("\" />");
			this.InternalWrite("<Source Name=\"");
			this.WriteEscaped(source);
			this.InternalWrite("\" />");
			this.InternalWrite("<Correlation ActivityID=\"");
			if (eventCache != null)
			{
				Guid activityId = eventCache.ActivityId;
				this.InternalWrite(activityId.ToString("B"));
				return;
			}
			Guid empty = Guid.Empty;
			this.InternalWrite(empty.ToString("B"));
		}
		private void WriteEndHeader(TraceEventCache eventCache)
		{
			this.InternalWrite("\" />");
			this.InternalWrite("<Execution ProcessName=\"");
			this.InternalWrite(TraceEventCache.GetProcessName());
			this.InternalWrite("\" ProcessID=\"");
			uint processId = (uint)TraceEventCache.GetProcessId();
			this.InternalWrite(processId.ToString(CultureInfo.InvariantCulture));
			this.InternalWrite("\" ThreadID=\"");
			if (eventCache != null)
			{
				this.WriteEscaped(eventCache.ThreadId.ToString(CultureInfo.InvariantCulture));
			}
			else
			{
				int threadId = TraceEventCache.GetThreadId();
				this.WriteEscaped(threadId.ToString(CultureInfo.InvariantCulture));
			}
			this.InternalWrite("\" />");
			this.InternalWrite("<Channel/>");
			this.InternalWrite("<Computer>");
			this.InternalWrite(this.machineName);
			this.InternalWrite("</Computer>");
			this.InternalWrite("</System>");
			this.InternalWrite("<ApplicationData>");
		}
		private void WriteFooter(TraceEventCache eventCache)
		{
			bool flag = base.IsEnabled(TraceOptions.LogicalOperationStack);
			bool flag2 = base.IsEnabled(TraceOptions.Callstack);
			if (eventCache != null && (flag || flag2))
			{
				this.InternalWrite("<System.Diagnostics xmlns=\"http://schemas.microsoft.com/2004/08/System.Diagnostics\">");
				if (flag)
				{
					this.InternalWrite("<LogicalOperationStack>");
					Stack logicalOperationStack = eventCache.LogicalOperationStack;
					if (logicalOperationStack != null)
					{
						IEnumerator enumerator = logicalOperationStack.GetEnumerator();
						try
						{
							while (enumerator.MoveNext())
							{
								object current = enumerator.Current;
								this.InternalWrite("<LogicalOperation>");
								this.WriteEscaped(current.ToString());
								this.InternalWrite("</LogicalOperation>");
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
					this.InternalWrite("</LogicalOperationStack>");
				}
				this.InternalWrite("<Timestamp>");
				long timestamp = eventCache.Timestamp;
				this.InternalWrite(timestamp.ToString(CultureInfo.InvariantCulture));
				this.InternalWrite("</Timestamp>");
				if (flag2)
				{
					this.InternalWrite("<Callstack>");
					this.WriteEscaped(eventCache.Callstack);
					this.InternalWrite("</Callstack>");
				}
				this.InternalWrite("</System.Diagnostics>");
			}
			this.InternalWrite("</ApplicationData></E2ETraceEvent>");
		}
		private void WriteEscaped(string str)
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
							this.InternalWrite(str.Substring(num, i - num));
							this.InternalWrite("&#xD;");
							num = i + 1;
						}
					}
					else
					{
						this.InternalWrite(str.Substring(num, i - num));
						this.InternalWrite("&#xA;");
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
								this.InternalWrite(str.Substring(num, i - num));
								this.InternalWrite("&amp;");
								num = i + 1;
								break;
							}
							case '\'':
							{
								this.InternalWrite(str.Substring(num, i - num));
								this.InternalWrite("&apos;");
								num = i + 1;
								break;
							}
							default:
							{
								switch (c)
								{
									case '<':
									{
										this.InternalWrite(str.Substring(num, i - num));
										this.InternalWrite("&lt;");
										num = i + 1;
										break;
									}
									case '>':
									{
										this.InternalWrite(str.Substring(num, i - num));
										this.InternalWrite("&gt;");
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
						this.InternalWrite(str.Substring(num, i - num));
						this.InternalWrite("&quot;");
						num = i + 1;
					}
				}
			}
			this.InternalWrite(str.Substring(num, str.Length - num));
		}
		private void InternalWrite(string message)
		{
			if (!base.EnsureWriter())
			{
				return;
			}
			this.writer.Write(message);
		}
         */
	}
}
