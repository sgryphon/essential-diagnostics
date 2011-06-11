using System;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using Essential.IO;
using System.Threading;

namespace Essential.Diagnostics
{
	/// <summary>
	/// Trace listener that writes to a text file, rolling to a new file based on a filename template (usually including the date).
	/// </summary>
    /// <remarks>
    /// <para>
    /// A rolling log file is achieved by including the date in the filename, so that when the date changes
    /// a different file is used.
    /// </para>
    /// <para>
    /// Available tokens are DateTime (a UTC DateTimeOffset) and LocalDateTime (a local DateTimeOffset), 
    /// as well as ApplicationName, ProcessId, ProcessName and MachineName. These use standard .NET 
    /// format strings, e.g. "Trace{DateTime:yyyyMMddTHH}.log" would generate a different log name
    /// each hour.
    /// </para>
    /// <para>
    /// The default filePathTemplate is "{ApplicationName}-{DateTime:yyyy-MM-dd}.log", which matches
    /// the format used by Microsoft.VisualBasic.Logging.FileLogTraceListener (except that it uses
    /// UTC time instead of local time).
    /// </para>
    /// </remarks>
    public class RollingFileTraceListener : TraceListenerBase
	{
        // Default format matches Microsoft.VisualBasic.Logging.FileLogTraceListener
        private const string _defaultFilePathTemplate = "{ApplicationName}-{DateTime:yyyy-MM-dd}.log";
        // Default format matches Microsoft.VisualBasic.Logging.FileLogTraceListener
        private const string _defaultTemplate = "{Source}\t{EventType}\t{Id}\t{Message}";
        private static string[] _supportedAttributes = new string[] 
            { 
                "template", "Template", 
                "convertWriteToEvent", "ConvertWriteToEvent",
            };

        private string _currentPath;
        private TextWriter _currentWriter;
        private object _fileLock = new object();
        private string _filePathTemplate;
        private IFileSystem _fileSystem = new FileSystem();


        /// <summary>
        /// Constructor. Writes to a rolling text file using the default name.
        /// </summary>
        public RollingFileTraceListener()
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
        /// format strings, e.g. "Trace{DateTime:yyyyMMddTHH}.log" would generate a different log name
        /// each hour.
        /// </para>
        /// <para>
        /// The default filePathTemplate is "{ApplicationName}-{DateTime:yyyy-MM-dd}.log", which matches
        /// the format used by Microsoft.VisualBasic.Logging.FileLogTraceListener (except that it uses
        /// UTC time instead of local time).
        /// </para>
        /// <para>
        /// To get behaviour that exactly matches FileLogTraceListener, 
        /// use "{ApplicationName}-{LocalDateTime:yyyy-MM-dd}.log".
        /// </para>
        /// </remarks>
        public RollingFileTraceListener(string filePathTemplate)
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
        /// Gets or sets whether calls to the Trace class static Write and WriteLine methods should be converted to Verbose events,
        /// and then filtered and formatted (otherwise they are output directly to the file).
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Boolean.TryParse(System.String,System.Boolean@)", Justification = "Default value is acceptable if conversion fails.")]
        public bool ConvertWriteToEvent
        {
            get
            {
                // Default behaviour is to output Trace.Write methods directly to the stream
                // (with Verbose color); setting this value to true will format as event first.
                var convertWriteToEvent = false;
                if (Attributes.ContainsKey("convertWriteToEvent"))
                {
                    bool.TryParse(Attributes["convertWriteToEvent"], out convertWriteToEvent);
                }
                return convertWriteToEvent;
            }
            set
            {
                Attributes["convertWriteToEvent"] = value.ToString(CultureInfo.InvariantCulture);
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
        /// Gets or sets the token format string to use to display trace output.
        /// </summary>
        /// <remarks>
        /// <para>
        /// See TraceFormatter for details of the supported formats.
        /// </para>
        /// <para>
        /// The default value is "{Source}\t{EventType}\t{Id}\t{Message}", which is a tab delimited format
        /// that matches the logs produced by Microsoft.VisualBasic.Logging.FileLogTraceListener.
        /// </para>
        /// <para>
        /// To get a format matching FileLogTraceListener with all TraceOutputOptions enabled, use
        /// "{Source}\t{EventType}\t{Id}\t{Message}\t{CallStack}\t{LogicalOperationStack}\t{DateTime:u}\t{ProcessId}\t{ThreadId}\t{Timestamp}\t{MachineName}".
        /// </para>
        /// <para>
        /// To get a format simliar to that produced by System.Diagnostics.TextWriterTraceListener,
        /// use "{Source} {EventType}: {Id} : {Message}".
        /// </para>
        /// </remarks>
        public string Template
        {
            get
            {
                // Default format matches Microsoft.VisualBasic.Logging.FileLogTraceListener
                if (Attributes.ContainsKey("template"))
                {
                    return Attributes["template"];
                }
                else
                {
                    return _defaultTemplate;
                }
            }
            set
            {
                Attributes["template"] = value;
            }
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
        /// Handles trace Write calls - either output directly to console or convert to verbose event
        /// based on the ConvertWriteToEvent setting.
        /// </summary>
        protected override void Write(string category, string message, object data)
        {
            // Either format as event or write direct to stream based on flag.
            if (ConvertWriteToEvent)
            {
                base.Write(category, message, data);
            }
            else
            {
                var filePath = GetCurrentFilePath(null, null, TraceEventType.Verbose, 0, message, null, new object[] { data });
                WriteToFile(filePath, message);
            }
        }

        /// <summary>
        /// Handles trace Write calls - either output directly to console or convert to verbose event
        /// based on the ConvertWriteToEvent setting.
        /// </summary>
        protected override void WriteLine(string category, string message, object data)
        {
            // Either format as event or write direct to stream based on flag.
            if (ConvertWriteToEvent)
            {
                base.WriteLine(category, message, data);
            }
            else
            {
                var filePath = GetCurrentFilePath(null, null, TraceEventType.Verbose, 0, message, null, new object[] { data });
                WriteLineToFile(filePath, null, message);
            }
        }

        /// <summary>
        /// Write trace event with data.
        /// </summary>
        protected override void WriteTrace(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message, Guid? relatedActivityId, object[] data)
        {
            // NOTE: In Microsoft.VisualBasic.Logging.FileLogTraceListener the following are output separated 
            // by a configurable delimiter: 
            //   source, TraceEventType, id, message, 
            // Then, based on TraceOutputOptions:
            //   call stack, logical operation stack, datetime (UTC, format "u"), process id, thread id, timestamp
            // Then, based on listener options:
            //   host name

            // Supporting a template string kind of makes TraceOutputOptions redundant,
            // but could support for backwards compatibility. 
            // i.e. if any T-O-O are set, then append them anyway??

            var filePath = GetCurrentFilePath(eventCache, source, eventType, id, message, relatedActivityId, data);
            var output = TraceFormatter.Format(Template,
                eventCache,
                source,
                eventType,
                id,
                message,
                relatedActivityId,
                data
                );
            WriteLineToFile(filePath, eventCache, output);
        }

        private string GetCurrentFilePath(TraceEventCache eventCache, string source, 
            TraceEventType eventType, int id, string message, 
            Guid? relatedActivityId, object[] data)
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

        private void WriteToFile(string filePath, string message)
        {
            lock (_fileLock)
            {
                EnsureCurrentWriter(filePath);
                _currentWriter.Write(message);
            }
        }

        private void WriteLineToFile(string filePath, TraceEventCache eventCache, string message)
        {
            lock (_fileLock)
            {
                EnsureCurrentWriter(filePath);
                _currentWriter.WriteLine(message);
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

        /*
		internal class ReferencedStream : IDisposable
		{

			private StreamWriter m_Stream;
			private int m_ReferenceCount;
			private object m_SyncObject;
			private bool m_Disposed;
			internal bool IsInUse
			{
				get
				{
					return this.m_Stream != null;
				}
			}
			internal long FileSize
			{
				get
				{
					return this.m_Stream.BaseStream.Length;
				}
			}
			void IDisposable.Dispose()
			{
				this.Dispose(true);
				GC.SuppressFinalize(this);
			}
			protected override void Finalize()
			{
				this.Dispose(false);
				base.Finalize();
			}
			internal ReferencedStream(StreamWriter stream)
			{
				this.m_ReferenceCount = 0;
				this.m_SyncObject = new object();
				this.m_Disposed = false;
				this.m_Stream = stream;
			}
			internal void Write(string message)
			{
				object syncObject = this.m_SyncObject;
				ObjectFlowControl.CheckForSyncLockOnValueType(syncObject);
				lock (syncObject)
				{
					this.m_Stream.Write(message);
				}
			}
			internal void WriteLine(string message)
			{
				object syncObject = this.m_SyncObject;
				ObjectFlowControl.CheckForSyncLockOnValueType(syncObject);
				lock (syncObject)
				{
					this.m_Stream.WriteLine(message);
				}
			}
			internal void AddReference()
			{
				checked
				{
					object syncObject = this.m_SyncObject;
					ObjectFlowControl.CheckForSyncLockOnValueType(syncObject);
					lock (syncObject)
					{
						this.m_ReferenceCount++;
					}
				}
			}
			internal void Flush()
			{
				object syncObject = this.m_SyncObject;
				ObjectFlowControl.CheckForSyncLockOnValueType(syncObject);
				lock (syncObject)
				{
					this.m_Stream.Flush();
				}
			}
			internal void CloseStream()
			{
				checked
				{
					object syncObject = this.m_SyncObject;
					ObjectFlowControl.CheckForSyncLockOnValueType(syncObject);
					lock (syncObject)
					{
						try
						{
							this.m_ReferenceCount--;
							this.m_Stream.Flush();
						}
						finally
						{
							if (this.m_ReferenceCount <= 0)
							{
								this.m_Stream.Close();
								this.m_Stream = null;
							}
						}
					}
				}
			}
			private void Dispose(bool disposing)
			{
				if (disposing && !this.m_Disposed)
				{
					if (this.m_Stream != null)
					{
						this.m_Stream.Close();
					}
					this.m_Disposed = true;
				}
			}
		}

		private LogFileLocation m_Location;
		private bool m_AutoFlush;
		private bool m_Append;
		private bool m_IncludeHostName;
		private DiskSpaceExhaustedOption m_DiskSpaceExhaustedBehavior;
		private string m_BaseFileName;
		private LogFileCreationScheduleOption m_LogFileDateStamp;
		private long m_MaxFileSize;
		private long m_ReserveDiskSpace;
		private string m_Delimiter;
		private Encoding m_Encoding;
		private string m_FullFileName;
		private string m_CustomLocation;
		private FileLogTraceListener.ReferencedStream m_Stream;
		private DateTime m_Day;
		private DateTime m_FirstDayOfWeek;
		private string m_HostName;
		private BitArray m_PropertiesSet;
		private static Dictionary<string, FileLogTraceListener.ReferencedStream> m_Streams = new Dictionary<string, FileLogTraceListener.ReferencedStream>();
		private string[] m_SupportedAttributes;
		private const int PROPERTY_COUNT = 12;
		private const int APPEND_INDEX = 0;
		private const int AUTOFLUSH_INDEX = 1;
		private const int BASEFILENAME_INDEX = 2;
		private const int CUSTOMLOCATION_INDEX = 3;
		private const int DELIMITER_INDEX = 4;
		private const int DISKSPACEEXHAUSTEDBEHAVIOR_INDEX = 5;
		private const int ENCODING_INDEX = 6;
		private const int INCLUDEHOSTNAME_INDEX = 7;
		private const int LOCATION_INDEX = 8;
		private const int LOGFILECREATIONSCHEDULE_INDEX = 9;
		private const int MAXFILESIZE_INDEX = 10;
		private const int RESERVEDISKSPACE_INDEX = 11;
		private const string DATE_FORMAT = "yyyy-MM-dd";
		private const string FILE_EXTENSION = ".log";
		private const int MAX_OPEN_ATTEMPTS = 2147483647;
		private const string DEFAULT_NAME = "FileLogTraceListener";
		private const int MIN_FILE_SIZE = 1000;
		private const string KEY_APPEND = "append";
		private const string KEY_APPEND_PASCAL = "Append";
		private const string KEY_AUTOFLUSH = "autoflush";
		private const string KEY_AUTOFLUSH_PASCAL = "AutoFlush";
		private const string KEY_AUTOFLUSH_CAMEL = "autoFlush";
		private const string KEY_BASEFILENAME = "basefilename";
		private const string KEY_BASEFILENAME_PASCAL = "BaseFilename";
		private const string KEY_BASEFILENAME_CAMEL = "baseFilename";
		private const string KEY_BASEFILENAME_PASCAL_ALT = "BaseFileName";
		private const string KEY_BASEFILENAME_CAMEL_ALT = "baseFileName";
		private const string KEY_CUSTOMLOCATION = "customlocation";
		private const string KEY_CUSTOMLOCATION_PASCAL = "CustomLocation";
		private const string KEY_CUSTOMLOCATION_CAMEL = "customLocation";
		private const string KEY_DELIMITER = "delimiter";
		private const string KEY_DELIMITER_PASCAL = "Delimiter";
		private const string KEY_DISKSPACEEXHAUSTEDBEHAVIOR = "diskspaceexhaustedbehavior";
		private const string KEY_DISKSPACEEXHAUSTEDBEHAVIOR_PASCAL = "DiskSpaceExhaustedBehavior";
		private const string KEY_DISKSPACEEXHAUSTEDBEHAVIOR_CAMEL = "diskSpaceExhaustedBehavior";
		private const string KEY_ENCODING = "encoding";
		private const string KEY_ENCODING_PASCAL = "Encoding";
		private const string KEY_INCLUDEHOSTNAME = "includehostname";
		private const string KEY_INCLUDEHOSTNAME_PASCAL = "IncludeHostName";
		private const string KEY_INCLUDEHOSTNAME_CAMEL = "includeHostName";
		private const string KEY_LOCATION = "location";
		private const string KEY_LOCATION_PASCAL = "Location";
		private const string KEY_LOGFILECREATIONSCHEDULE = "logfilecreationschedule";
		private const string KEY_LOGFILECREATIONSCHEDULE_PASCAL = "LogFileCreationSchedule";
		private const string KEY_LOGFILECREATIONSCHEDULE_CAMEL = "logFileCreationSchedule";
		private const string KEY_MAXFILESIZE = "maxfilesize";
		private const string KEY_MAXFILESIZE_PASCAL = "MaxFileSize";
		private const string KEY_MAXFILESIZE_CAMEL = "maxFileSize";
		private const string KEY_RESERVEDISKSPACE = "reservediskspace";
		private const string KEY_RESERVEDISKSPACE_PASCAL = "ReserveDiskSpace";
		private const string KEY_RESERVEDISKSPACE_CAMEL = "reserveDiskSpace";
		private const string STACK_DELIMITER = ", ";
		
        public unsafe LogFileLocation Location
		{
			get
			{
				if (!this.m_PropertiesSet[8] && this.Attributes.ContainsKey("location"))
				{
					TypeConverter converter = TypeDescriptor.GetConverter(typeof(LogFileLocation));
					this.Location = *unbox(Microsoft.VisualBasic.Logging.LogFileLocation, converter.ConvertFromInvariantString(this.Attributes["location"]));
				}
				return this.m_Location;
			}
			set
			{
				this.ValidateLogFileLocationEnumValue(value, "value");
				if (this.m_Location != value)
				{
					this.CloseCurrentStream();
				}
				this.m_Location = value;
				this.m_PropertiesSet[8] = true;
			}
		}
		public bool AutoFlush
		{
			get
			{
				if (!this.m_PropertiesSet[1] && this.Attributes.ContainsKey("autoflush"))
				{
					this.AutoFlush = Convert.ToBoolean(this.Attributes["autoflush"], CultureInfo.InvariantCulture);
				}
				return this.m_AutoFlush;
			}
			[SecuritySafeCritical]
			set
			{
				this.DemandWritePermission();
				this.m_AutoFlush = value;
				this.m_PropertiesSet[1] = true;
			}
		}
		public bool IncludeHostName
		{
			get
			{
				if (!this.m_PropertiesSet[7] && this.Attributes.ContainsKey("includehostname"))
				{
					this.IncludeHostName = Convert.ToBoolean(this.Attributes["includehostname"], CultureInfo.InvariantCulture);
				}
				return this.m_IncludeHostName;
			}
			[SecuritySafeCritical]
			set
			{
				this.DemandWritePermission();
				this.m_IncludeHostName = value;
				this.m_PropertiesSet[7] = true;
			}
		}
		public bool Append
		{
			get
			{
				if (!this.m_PropertiesSet[0] && this.Attributes.ContainsKey("append"))
				{
					this.Append = Convert.ToBoolean(this.Attributes["append"], CultureInfo.InvariantCulture);
				}
				return this.m_Append;
			}
			[SecuritySafeCritical]
			set
			{
				this.DemandWritePermission();
				if (value != this.m_Append)
				{
					this.CloseCurrentStream();
				}
				this.m_Append = value;
				this.m_PropertiesSet[0] = true;
			}
		}
		public unsafe DiskSpaceExhaustedOption DiskSpaceExhaustedBehavior
		{
			get
			{
				if (!this.m_PropertiesSet[5] && this.Attributes.ContainsKey("diskspaceexhaustedbehavior"))
				{
					TypeConverter converter = TypeDescriptor.GetConverter(typeof(DiskSpaceExhaustedOption));
					this.DiskSpaceExhaustedBehavior = *unbox(Microsoft.VisualBasic.Logging.DiskSpaceExhaustedOption, converter.ConvertFromInvariantString(this.Attributes["diskspaceexhaustedbehavior"]));
				}
				return this.m_DiskSpaceExhaustedBehavior;
			}
			[SecuritySafeCritical]
			set
			{
				this.DemandWritePermission();
				this.ValidateDiskSpaceExhaustedOptionEnumValue(value, "value");
				this.m_DiskSpaceExhaustedBehavior = value;
				this.m_PropertiesSet[5] = true;
			}
		}
		public string BaseFileName
		{
			get
			{
				if (!this.m_PropertiesSet[2] && this.Attributes.ContainsKey("basefilename"))
				{
					this.BaseFileName = this.Attributes["basefilename"];
				}
				return this.m_BaseFileName;
			}
			set
			{
				if (Operators.CompareString(value, "", false) == 0)
				{
					throw ExceptionUtils.GetArgumentNullException("value", "ApplicationLogBaseNameNull", new string[0]);
				}
				Path.GetFullPath(value);
				if (string.Compare(value, this.m_BaseFileName, StringComparison.OrdinalIgnoreCase) != 0)
				{
					this.CloseCurrentStream();
					this.m_BaseFileName = value;
				}
				this.m_PropertiesSet[2] = true;
			}
		}
		public string FullLogFileName
		{
			[SecuritySafeCritical]
			get
			{
				this.EnsureStreamIsOpen();
				string fullFileName = this.m_FullFileName;
				FileIOPermission fileIOPermission = new FileIOPermission(FileIOPermissionAccess.PathDiscovery, fullFileName);
				fileIOPermission.Demand();
				return fullFileName;
			}
		}
		public unsafe LogFileCreationScheduleOption LogFileCreationSchedule
		{
			get
			{
				if (!this.m_PropertiesSet[9] && this.Attributes.ContainsKey("logfilecreationschedule"))
				{
					TypeConverter converter = TypeDescriptor.GetConverter(typeof(LogFileCreationScheduleOption));
					this.LogFileCreationSchedule = *unbox(Microsoft.VisualBasic.Logging.LogFileCreationScheduleOption, converter.ConvertFromInvariantString(this.Attributes["logfilecreationschedule"]));
				}
				return this.m_LogFileDateStamp;
			}
			set
			{
				this.ValidateLogFileCreationScheduleOptionEnumValue(value, "value");
				if (value != this.m_LogFileDateStamp)
				{
					this.CloseCurrentStream();
					this.m_LogFileDateStamp = value;
				}
				this.m_PropertiesSet[9] = true;
			}
		}
		public long MaxFileSize
		{
			get
			{
				if (!this.m_PropertiesSet[10] && this.Attributes.ContainsKey("maxfilesize"))
				{
					this.MaxFileSize = Convert.ToInt64(this.Attributes["maxfilesize"], CultureInfo.InvariantCulture);
				}
				return this.m_MaxFileSize;
			}
			[SecuritySafeCritical]
			set
			{
				this.DemandWritePermission();
				if (value < 1000L)
				{
					throw ExceptionUtils.GetArgumentExceptionWithArgName("value", "ApplicationLogNumberTooSmall", new string[]
					{
						"MaxFileSize"
					});
				}
				this.m_MaxFileSize = value;
				this.m_PropertiesSet[10] = true;
			}
		}
		public long ReserveDiskSpace
		{
			get
			{
				if (!this.m_PropertiesSet[11] && this.Attributes.ContainsKey("reservediskspace"))
				{
					this.ReserveDiskSpace = Convert.ToInt64(this.Attributes["reservediskspace"], CultureInfo.InvariantCulture);
				}
				return this.m_ReserveDiskSpace;
			}
			[SecuritySafeCritical]
			set
			{
				this.DemandWritePermission();
				if (value < 0L)
				{
					throw ExceptionUtils.GetArgumentExceptionWithArgName("value", "ApplicationLog_NegativeNumber", new string[]
					{
						"ReserveDiskSpace"
					});
				}
				this.m_ReserveDiskSpace = value;
				this.m_PropertiesSet[11] = true;
			}
		}
		public string Delimiter
		{
			get
			{
				if (!this.m_PropertiesSet[4] && this.Attributes.ContainsKey("delimiter"))
				{
					this.Delimiter = this.Attributes["delimiter"];
				}
				return this.m_Delimiter;
			}
			set
			{
				this.m_Delimiter = value;
				this.m_PropertiesSet[4] = true;
			}
		}
		public Encoding Encoding
		{
			get
			{
				if (!this.m_PropertiesSet[6] && this.Attributes.ContainsKey("encoding"))
				{
					this.Encoding = Encoding.GetEncoding(this.Attributes["encoding"]);
				}
				return this.m_Encoding;
			}
			set
			{
				if (value == null)
				{
					throw ExceptionUtils.GetArgumentNullException("value");
				}
				this.m_Encoding = value;
				this.m_PropertiesSet[6] = true;
			}
		}
		public string CustomLocation
		{
			[SecuritySafeCritical]
			get
			{
				if (!this.m_PropertiesSet[3] && this.Attributes.ContainsKey("customlocation"))
				{
					this.CustomLocation = this.Attributes["customlocation"];
				}
				string fullPath = Path.GetFullPath(this.m_CustomLocation);
				FileIOPermission fileIOPermission = new FileIOPermission(FileIOPermissionAccess.PathDiscovery, fullPath);
				fileIOPermission.Demand();
				return fullPath;
			}
			set
			{
				string fullPath = Path.GetFullPath(value);
				if (!Directory.Exists(fullPath))
				{
					Directory.CreateDirectory(fullPath);
				}
				if (this.Location == LogFileLocation.Custom & string.Compare(fullPath, this.m_CustomLocation, StringComparison.OrdinalIgnoreCase) != 0)
				{
					this.CloseCurrentStream();
				}
				this.Location = LogFileLocation.Custom;
				this.m_CustomLocation = fullPath;
				this.m_PropertiesSet[3] = true;
			}
		}
		private string LogFileName
		{
			get
			{
				string path;
				switch (this.Location)
				{
					case LogFileLocation.TempDirectory:
					{
						path = Path.GetTempPath();
						break;
					}
					case LogFileLocation.LocalUserApplicationDirectory:
					{
						path = Application.UserAppDataPath;
						break;
					}
					case LogFileLocation.CommonApplicationDirectory:
					{
						path = Application.CommonAppDataPath;
						break;
					}
					case LogFileLocation.ExecutableDirectory:
					{
						path = Path.GetDirectoryName(Application.ExecutablePath);
						break;
					}
					case LogFileLocation.Custom:
					{
						if (Operators.CompareString(this.CustomLocation, "", false) == 0)
						{
							path = Application.UserAppDataPath;
						}
						else
						{
							path = this.CustomLocation;
						}
						break;
					}
					default:
					{
						path = Application.UserAppDataPath;
						break;
					}
				}
				string text = this.BaseFileName;
				switch (this.LogFileCreationSchedule)
				{
					case LogFileCreationScheduleOption.Daily:
					{
						string arg_BB_0 = text;
						string arg_BB_1 = "-";
						DateTime now = DateAndTime.Now;
						DateTime dateTime = now.Date;
						text = arg_BB_0 + arg_BB_1 + dateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
						break;
					}
					case LogFileCreationScheduleOption.Weekly:
					{
						DateTime now = DateAndTime.Now;
						double arg_DC_0 = (double)0;
						DateTime dateTime = DateAndTime.Now;
						this.m_FirstDayOfWeek = now.AddDays(checked(arg_DC_0 - (double)dateTime.DayOfWeek));
						string arg_10C_0 = text;
						string arg_10C_1 = "-";
						dateTime = this.m_FirstDayOfWeek.Date;
						text = arg_10C_0 + arg_10C_1 + dateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
						break;
					}
				}
				return Path.Combine(path, text);
			}
		}
		private FileLogTraceListener.ReferencedStream ListenerStream
		{
			get
			{
				this.EnsureStreamIsOpen();
				return this.m_Stream;
			}
		}
		private string HostName
		{
			get
			{
				if (Operators.CompareString(this.m_HostName, "", false) == 0)
				{
					this.m_HostName = Environment.MachineName;
				}
				return this.m_HostName;
			}
		}
		public FileLogTraceListener(string name) : base(name)
		{
			this.m_Location = LogFileLocation.LocalUserApplicationDirectory;
			this.m_AutoFlush = false;
			this.m_Append = true;
			this.m_IncludeHostName = false;
			this.m_DiskSpaceExhaustedBehavior = DiskSpaceExhaustedOption.DiscardMessages;
			this.m_BaseFileName = Path.GetFileNameWithoutExtension(Application.ExecutablePath);
			this.m_LogFileDateStamp = LogFileCreationScheduleOption.None;
			this.m_MaxFileSize = 5000000L;
			this.m_ReserveDiskSpace = 10000000L;
			this.m_Delimiter = "\t";
			this.m_Encoding = Encoding.UTF8;
			this.m_CustomLocation = Application.UserAppDataPath;
			DateTime now = DateAndTime.Now;
			this.m_Day = now.Date;
			now = DateAndTime.Now;
			this.m_FirstDayOfWeek = FileLogTraceListener.GetFirstDayOfWeek(now.Date);
			this.m_PropertiesSet = new BitArray(12, false);
			this.m_SupportedAttributes = new string[]
			{
				"append", 
				"Append", 
				"autoflush", 
				"AutoFlush", 
				"autoFlush", 
				"basefilename", 
				"BaseFilename", 
				"baseFilename", 
				"BaseFileName", 
				"baseFileName", 
				"customlocation", 
				"CustomLocation", 
				"customLocation", 
				"delimiter", 
				"Delimiter", 
				"diskspaceexhaustedbehavior", 
				"DiskSpaceExhaustedBehavior", 
				"diskSpaceExhaustedBehavior", 
				"encoding", 
				"Encoding", 
				"includehostname", 
				"IncludeHostName", 
				"includeHostName", 
				"location", 
				"Location", 
				"logfilecreationschedule", 
				"LogFileCreationSchedule", 
				"logFileCreationSchedule", 
				"maxfilesize", 
				"MaxFileSize", 
				"maxFileSize", 
				"reservediskspace", 
				"ReserveDiskSpace", 
				"reserveDiskSpace"
			};
		}
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public FileLogTraceListener() : this("FileLogTraceListener")
		{
		}
		public override void Write(string message)
		{
			try
			{
				this.HandleDateChange();
				long newEntrySize = (long)this.Encoding.GetByteCount(message);
				if (this.ResourcesAvailable(newEntrySize))
				{
					this.ListenerStream.Write(message);
					if (this.AutoFlush)
					{
						this.ListenerStream.Flush();
					}
				}
			}
			catch (Exception)
			{
				this.CloseCurrentStream();
				throw;
			}
		}
		public override void WriteLine(string message)
		{
			try
			{
				this.HandleDateChange();
				long newEntrySize = (long)this.Encoding.GetByteCount(message + "\r\n");
				if (this.ResourcesAvailable(newEntrySize))
				{
					this.ListenerStream.WriteLine(message);
					if (this.AutoFlush)
					{
						this.ListenerStream.Flush();
					}
				}
			}
			catch (Exception)
			{
				this.CloseCurrentStream();
				throw;
			}
		}
		public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
		{
			if (this.Filter != null && !this.Filter.ShouldTrace(eventCache, source, eventType, id, message, null, null, null))
			{
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(source + this.Delimiter);
			stringBuilder.Append(Enum.GetName(typeof(TraceEventType), eventType) + this.Delimiter);
			stringBuilder.Append(id.ToString(CultureInfo.InvariantCulture) + this.Delimiter);
			stringBuilder.Append(message);
			if ((this.TraceOutputOptions & TraceOptions.Callstack) == TraceOptions.Callstack)
			{
				stringBuilder.Append(this.Delimiter + eventCache.Callstack);
			}
			if ((this.TraceOutputOptions & TraceOptions.LogicalOperationStack) == TraceOptions.LogicalOperationStack)
			{
				stringBuilder.Append(this.Delimiter + FileLogTraceListener.StackToString(eventCache.LogicalOperationStack));
			}
			if ((this.TraceOutputOptions & TraceOptions.DateTime) == TraceOptions.DateTime)
			{
				StringBuilder arg_103_0 = stringBuilder;
				string arg_FE_0 = this.Delimiter;
				DateTime dateTime = eventCache.DateTime;
				arg_103_0.Append(arg_FE_0 + dateTime.ToString("u", CultureInfo.InvariantCulture));
			}
			if ((this.TraceOutputOptions & TraceOptions.ProcessId) == TraceOptions.ProcessId)
			{
				StringBuilder arg_133_0 = stringBuilder;
				string arg_12E_0 = this.Delimiter;
				int processId = eventCache.ProcessId;
				arg_133_0.Append(arg_12E_0 + processId.ToString(CultureInfo.InvariantCulture));
			}
			if ((this.TraceOutputOptions & TraceOptions.ThreadId) == TraceOptions.ThreadId)
			{
				stringBuilder.Append(this.Delimiter + eventCache.ThreadId);
			}
			if ((this.TraceOutputOptions & TraceOptions.Timestamp) == TraceOptions.Timestamp)
			{
				StringBuilder arg_188_0 = stringBuilder;
				string arg_183_0 = this.Delimiter;
				long timestamp = eventCache.Timestamp;
				arg_188_0.Append(arg_183_0 + timestamp.ToString(CultureInfo.InvariantCulture));
			}
			if (this.IncludeHostName)
			{
				stringBuilder.Append(this.Delimiter + this.HostName);
			}
			this.WriteLine(stringBuilder.ToString());
		}
		public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
		{
			string message = null;
			if (args != null)
			{
				message = string.Format(CultureInfo.InvariantCulture, format, args);
			}
			else
			{
				message = format;
			}
			this.TraceEvent(eventCache, source, eventType, id, message);
		}
		public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
		{
			string message = "";
			if (data != null)
			{
				message = data.ToString();
			}
			this.TraceEvent(eventCache, source, eventType, id, message);
		}
		public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
		{
			checked
			{
				StringBuilder stringBuilder = new StringBuilder();
				if (data != null)
				{
					int num = data.Length - 1;
					int arg_14_0 = 0;
					int num2 = num;
					for (int i = arg_14_0; i <= num2; i++)
					{
						stringBuilder.Append(data[i].ToString());
						if (i != num)
						{
							stringBuilder.Append(this.Delimiter);
						}
					}
				}
				this.TraceEvent(eventCache, source, eventType, id, stringBuilder.ToString());
			}
		}
		public override void Flush()
		{
			if (this.m_Stream != null)
			{
				this.m_Stream.Flush();
			}
		}
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public override void Close()
		{
			this.Dispose(true);
		}
		protected override string[] GetSupportedAttributes()
		{
			return this.m_SupportedAttributes;
		}
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				this.CloseCurrentStream();
			}
		}
		[SecuritySafeCritical]
		private FileLogTraceListener.ReferencedStream GetStream()
		{
			checked
			{
				int num = 0;
				FileLogTraceListener.ReferencedStream referencedStream = null;
				string fullPath = Path.GetFullPath(this.LogFileName + ".log");
				while (referencedStream == null && num < 2147483647)
				{
					string fullPath2;
					if (num == 0)
					{
						fullPath2 = Path.GetFullPath(this.LogFileName + ".log");
					}
					else
					{
						fullPath2 = Path.GetFullPath(this.LogFileName + "-" + num.ToString(CultureInfo.InvariantCulture) + ".log");
					}
					string key = fullPath2.ToUpper(CultureInfo.InvariantCulture);
					Dictionary<string, FileLogTraceListener.ReferencedStream> streams = FileLogTraceListener.m_Streams;
					FileLogTraceListener.ReferencedStream result;
					lock (streams)
					{
						if (FileLogTraceListener.m_Streams.ContainsKey(key))
						{
							referencedStream = FileLogTraceListener.m_Streams[key];
							if (!referencedStream.IsInUse)
							{
								FileLogTraceListener.m_Streams.Remove(key);
								referencedStream = null;
							}
							else
							{
								if (this.Append)
								{
									FileIOPermission fileIOPermission = new FileIOPermission(FileIOPermissionAccess.Write, fullPath2);
									fileIOPermission.Demand();
									referencedStream.AddReference();
									this.m_FullFileName = fullPath2;
									result = referencedStream;
									return result;
								}
								num++;
								referencedStream = null;
								continue;
							}
						}
						Encoding encoding = this.Encoding;
						try
						{
							if (this.Append)
							{
								encoding = this.GetFileEncoding(fullPath2);
								if (encoding == null)
								{
									encoding = this.Encoding;
								}
							}
							StreamWriter stream = new StreamWriter(fullPath2, this.Append, encoding);
							referencedStream = new FileLogTraceListener.ReferencedStream(stream);
							referencedStream.AddReference();
							FileLogTraceListener.m_Streams.Add(key, referencedStream);
							this.m_FullFileName = fullPath2;
							result = referencedStream;
							return result;
						}
						catch (IOException arg_14D_0)
						{
						}
						num++;
					}
					continue;
					return result;
				}
				throw ExceptionUtils.GetInvalidOperationException("ApplicationLog_ExhaustedPossibleStreamNames", new string[]
				{
					fullPath
				});
			}
		}
		private void EnsureStreamIsOpen()
		{
			if (this.m_Stream == null)
			{
				this.m_Stream = this.GetStream();
			}
		}
		private void CloseCurrentStream()
		{
			if (this.m_Stream != null)
			{
				Dictionary<string, FileLogTraceListener.ReferencedStream> streams = FileLogTraceListener.m_Streams;
				lock (streams)
				{
					this.m_Stream.CloseStream();
					if (!this.m_Stream.IsInUse)
					{
						FileLogTraceListener.m_Streams.Remove(this.m_FullFileName.ToUpper(CultureInfo.InvariantCulture));
					}
					this.m_Stream = null;
				}
			}
		}
		private bool DayChanged()
		{
			DateTime arg_18_0 = this.m_Day.Date;
			DateTime now = DateAndTime.Now;
			return DateTime.Compare(arg_18_0, now.Date) != 0;
		}
		private bool WeekChanged()
		{
			DateTime arg_1D_0 = this.m_FirstDayOfWeek.Date;
			DateTime now = DateAndTime.Now;
			return DateTime.Compare(arg_1D_0, FileLogTraceListener.GetFirstDayOfWeek(now.Date)) != 0;
		}
		private static DateTime GetFirstDayOfWeek(DateTime checkDate)
		{
			DateTime dateTime = checkDate.AddDays((double)checked(DayOfWeek.Sunday - checkDate.DayOfWeek));
			return dateTime.Date;
		}
		private void HandleDateChange()
		{
			if (this.LogFileCreationSchedule == LogFileCreationScheduleOption.Daily)
			{
				if (this.DayChanged())
				{
					this.CloseCurrentStream();
				}
			}
			else
			{
				if (this.LogFileCreationSchedule == LogFileCreationScheduleOption.Weekly && this.WeekChanged())
				{
					this.CloseCurrentStream();
				}
			}
		}
		private bool ResourcesAvailable(long newEntrySize)
		{
			checked
			{
				if (this.ListenerStream.FileSize + newEntrySize > this.MaxFileSize)
				{
					if (this.DiskSpaceExhaustedBehavior == DiskSpaceExhaustedOption.ThrowException)
					{
						throw new InvalidOperationException(Utils.GetResourceString("ApplicationLog_FileExceedsMaximumSize"));
					}
					return false;
				}
				else
				{
					if (this.GetFreeDiskSpace() - newEntrySize >= this.ReserveDiskSpace)
					{
						return true;
					}
					if (this.DiskSpaceExhaustedBehavior == DiskSpaceExhaustedOption.ThrowException)
					{
						throw new InvalidOperationException(Utils.GetResourceString("ApplicationLog_ReservedSpaceEncroached"));
					}
					return false;
				}
			}
		}
		[SecuritySafeCritical]
		private long GetFreeDiskSpace()
		{
			string pathRoot = Path.GetPathRoot(Path.GetFullPath(this.FullLogFileName));
			long num = -1L;
			FileIOPermission fileIOPermission = new FileIOPermission(FileIOPermissionAccess.PathDiscovery, pathRoot);
			fileIOPermission.Demand();
			long num2;
			long num3;
			if (UnsafeNativeMethods.GetDiskFreeSpaceEx(pathRoot, ref num, ref num2, ref num3) && num > -1L)
			{
				return num;
			}
			throw ExceptionUtils.GetWin32Exception("ApplicationLog_FreeSpaceError", new string[0]);
		}
		private Encoding GetFileEncoding(string fileName)
		{
			if (File.Exists(fileName))
			{
				StreamReader streamReader = null;
				Encoding currentEncoding;
				try
				{
					streamReader = new StreamReader(fileName, this.Encoding, true);
					if (streamReader.BaseStream.Length > 0L)
					{
						streamReader.ReadLine();
						currentEncoding = streamReader.CurrentEncoding;
						return currentEncoding;
					}
				}
				finally
				{
					if (streamReader != null)
					{
						streamReader.Close();
					}
				}
				goto IL_43;
				return currentEncoding;
			}
			IL_43:
			return null;
		}
		[SecurityCritical]
		private void DemandWritePermission()
		{
			string directoryName = Path.GetDirectoryName(this.LogFileName);
			FileIOPermission fileIOPermission = new FileIOPermission(FileIOPermissionAccess.Write, directoryName);
			fileIOPermission.Demand();
		}
		private void ValidateLogFileLocationEnumValue(LogFileLocation value, string paramName)
		{
			if (value < LogFileLocation.TempDirectory || value > LogFileLocation.Custom)
			{
				throw new InvalidEnumArgumentException(paramName, (int)value, typeof(LogFileLocation));
			}
		}
		private void ValidateDiskSpaceExhaustedOptionEnumValue(DiskSpaceExhaustedOption value, string paramName)
		{
			if (value < DiskSpaceExhaustedOption.ThrowException || value > DiskSpaceExhaustedOption.DiscardMessages)
			{
				throw new InvalidEnumArgumentException(paramName, (int)value, typeof(DiskSpaceExhaustedOption));
			}
		}
		private void ValidateLogFileCreationScheduleOptionEnumValue(LogFileCreationScheduleOption value, string paramName)
		{
			if (value < LogFileCreationScheduleOption.None || value > LogFileCreationScheduleOption.Weekly)
			{
				throw new InvalidEnumArgumentException(paramName, (int)value, typeof(LogFileCreationScheduleOption));
			}
		}
		private static string StackToString(Stack stack)
		{
			int length = ", ".Length;
			StringBuilder stringBuilder = new StringBuilder();
			try
			{
				IEnumerator enumerator = stack.GetEnumerator();
				while (enumerator.MoveNext())
				{
					object current = enumerator.Current;
					stringBuilder.Append(current.ToString() + ", ");
				}
			}
			finally
			{
				IEnumerator enumerator;
				if (enumerator is IDisposable)
				{
					(enumerator as IDisposable).Dispose();
				}
			}
			stringBuilder.Replace("\"", "\"\"");
			if (stringBuilder.Length >= length)
			{
				stringBuilder.Remove(checked(stringBuilder.Length - length), length);
			}
			return "\"" + stringBuilder.ToString() + "\"";
		}
        */
	}
}
