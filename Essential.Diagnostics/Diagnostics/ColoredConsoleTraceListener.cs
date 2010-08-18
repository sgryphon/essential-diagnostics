using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

namespace Essential.Diagnostics
{
    /// <summary>
    /// Trace listener that outputs to the console in color, optionally using a custom
    /// formatting template. 
    /// </summary>
    /// <remarks>
    /// <para>
    /// The initializeData for the ColoredConsoleTraceListener should contain a boolean
    /// (true/false) value indicating whether to use the Console.Error stream. The default is
    /// false, that is to use the normal Console.Out stream.
    /// </para>
    /// <para>
    /// Note that colored output is only used in the normal stream; it is not used
    /// in the error stream.
    /// </para>
    /// <para>
    /// <list type="">
    /// <listheader>Configuration options</listheader>
    /// <item>
    /// <term>initializeData</term>
    /// <value>false (default) to use the Console.Out stream; 
    /// true to use Console.Error</value>
    /// </item>
    /// <item>
    /// <term>template</term>
    /// <value>Template to use to format trace messages.
    /// The default format is "{Source} {EventType}: {Id} : {Message}".
    /// For more information on the template tokens available, <see cref="TraceFormatter"/>.</value>
    /// </item>
    /// <item>
    /// <term>convertWriteToEvent</term>
    /// <value>If false (default), then calls to <see cref="Write"/>,<see cref="WriteLine"/> 
    /// and similar methods are output directly to the output stream (using the Verbose color).
    /// If true, then calls to these methods are instead converted to Verbose trace events and then 
    /// output using the same format as calls to Trace methods.</value>
    /// </item>
    /// <item>
    /// <term>criticalColor</term>
    /// <value>Color to use for <see cref="TraceEventType.Critical"/> events.
    /// The default color for fatal events is <see cref="ConsoleColor.Red"/>.</value>
    /// </item>
    /// <item>
    /// <term>errorColor</term>
    /// <value>Color to use for <see cref="TraceEventType.Error"/> events.
    /// The default color for errors is <see cref="ConsoleColor.DarkRed"/>.</value>
    /// </item>
    /// <item>
    /// <term>warningColor</term>
    /// <value>Color to use for <see cref="TraceEventType.Warning"/> events.
    /// The default color for warnings is <see cref="ConsoleColor.Yellow"/>.</value>
    /// </item>
    /// <item>
    /// <term>informationColor</term>
    /// <value>Color to use for <see cref="TraceEventType.Information"/> events.
    /// The default color for information events is <see cref="ConsoleColor.Gray"/>.</value>
    /// </item>
    /// <item>
    /// <term>verboseColor</term>
    /// <value>Color to use for <see cref="TraceEventType.Verbose"/> events.
    /// The default color for verbose messages is <see cref="ConsoleColor.DarkCyan"/>.</value>
    /// </item>
    /// <item>
    /// <term>activityTracingColor</term>
    /// <value>Color to use for activity tracing (start, stop, transfer, etc) events, 
    /// unless overridden by a particular color for the specific activity event.
    /// The default color for activity tracing events is <see cref="ConsoleColor.Gray"/>,
    /// the same as information events.</value>
    /// </item>
    /// <item>
    /// <term>startColor</term>
    /// <value>Color to use for <see cref="TraceEventType.Start"/> events.</value>
    /// </item>
    /// <item>
    /// <term>stopColor</term>
    /// <value>Color to use for <see cref="TraceEventType.Stop"/> events.</value>
    /// </item>
    /// <item>
    /// <term>suspendColor</term>
    /// <value>Color to use for <see cref="TraceEventType.Suspend"/> events.</value>
    /// </item>
    /// <item>
    /// <term>resumeColor</term>
    /// <value>Color to use for <see cref="TraceEventType.Resume"/> events.</value>
    /// </item>
    /// <item>
    /// <term>transferColor</term>
    /// <value>Color to use for <see cref="TraceEventType.Transfer"/> events.</value>
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// Valid color values are those from the <see cref="ConsoleColor"/> enumeration.
    /// </para>
    /// <para>
    /// When selecting colors note that PowerShell redefines DarkYellow and DarkMagenta and uses them
    /// as default colors, so best to to avoid those two colors because result is not consistent.
    /// </para>
    /// </remarks>
    public class ColoredConsoleTraceListener : TraceListenerBase
    {
        // //////////////////////////////////////////////////////////
        // Fields

        private IConsole _console = new ConsoleAdapter();
        private object _consoleLock = new object();
        // Don't want the default to be too overpowering (garish), so limit to a few colors.
        // Use red for errors (including fatal) and yellow for warning, 
        // and standard gray for most other normal messages, including activity tracing.
        // Verbose messages use a darker shade.
        private static Dictionary<TraceEventType, ConsoleColor> _defaultColorByEventType = new Dictionary<TraceEventType, ConsoleColor>()
        {
            { TraceEventType.Critical, ConsoleColor.Red },
            { TraceEventType.Error, ConsoleColor.DarkRed },
            { TraceEventType.Warning, ConsoleColor.Yellow },
            { TraceEventType.Verbose, ConsoleColor.DarkCyan },
        };
        private const ConsoleColor _defaultColorOther = ConsoleColor.Gray;
        private const string _defaultTemplate = "{Source} {EventType}: {Id} : {Message}";
        private static string[] _supportedAttributes = new string[] 
            { 
                "template", "Template", "convertWriteToEvent", "ConvertWriteToEvent",
                "criticalColor", "CriticalColor", "criticalcolor", 
                "errorColor", "ErrorColor", "errorcolor",
                "warningColor", "WarningColor", "warningcolor",
                "informationColor", "InformationColor", "informationcolor",
                "verboseColor", "VerboseColor", "verbosecolor",
                "startColor", "StartColor", "startcolor",
                "stopColor", "StopColor", "stopcolor",
                "suspendColor", "SuspendColor", "suspendcolor",
                "resumeColor", "ResumeColor", "resumecolor",
                "transferColor", "TransferColor", "transfercolor",
                "activityTracingColor", "ActivityTracingColor", "activitytracingcolor",
            };
        private bool _useErrorStream;
        private TextWriter _writer;


        // //////////////////////////////////////////////////////////
        // Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public ColoredConsoleTraceListener()
            : this(false)
        {
        }

        /// <summary>
        /// Constructor with initializeData.
        /// </summary>
        /// <param name="useErrorStream">false to use standard output (default); true to use standard error.</param>
        public ColoredConsoleTraceListener(bool useErrorStream)
            : base(string.Empty)
        {
//#if DEBUG
//            System.Console.WriteLine("ColoredConsoleTraceListener.ctor {0}", useErrorStream);
//#endif

            // Behaviour consistent with System.Diagnostics.ConsoleTraceListener
            // -- initializeData determines whether stdout or stderr is used.
            _useErrorStream = useErrorStream;
            SetWriter();
        }


        // //////////////////////////////////////////////////////////
        // Public Properties

        /// <summary>
        /// Gets or sets the console to use; this defaults to an adapter for System.Console.
        /// </summary>
        public IConsole Console
        {
            get { return _console; }
            set
            {
                lock (_consoleLock)
                {
                    _console = value;
                    SetWriter();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether calls to the Trace class static Write and WriteLine methods should be converted to Verbose events,
        /// and then filtered and formatted (otherwise they are output directly to the console).
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
        public string Template
        {
            get
            {
                // Default format matches System.Diagnostics.TraceListener
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
        /// Gets whether to use the error stream or the standard out stream.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value is part of initializeData; if the value changes the
        /// listener is recreated.
        /// </para>
        /// </remarks>
        public bool UseErrorStream
        {
            get { return _useErrorStream; }
        }


        // //////////////////////////////////////////////////////////
        // Public Methods

        /// <summary>
        /// Gets the console color set for the specified event type, or the default color.
        /// </summary>
        /// <param name="eventType">TraceEventType to get the ConsoleColor for.</param>
        /// <returns>The ConsoleColor used to display the specified TraceEventType.</returns>
        public ConsoleColor GetConsoleColor(TraceEventType eventType)
        {
            var key = eventType.ToString() + "Color";
            if (Attributes.ContainsKey(key))
            {
                var setting = Attributes[key];
                if (Enum.IsDefined(typeof (ConsoleColor), setting))
                {
                    return (ConsoleColor) Enum.Parse(typeof (ConsoleColor), setting);
                }
            }

            if (((int)eventType & (int)SourceLevels.ActivityTracing) > 0
                && Attributes.ContainsKey("activityTracingColor"))
            {
                var setting = Attributes["activityTracingColor"];
                if (Enum.IsDefined(typeof (ConsoleColor), setting))
                {
                    return (ConsoleColor) Enum.Parse(typeof (ConsoleColor), setting);
                }
            }

            return GetDefaultColor(eventType);
        }

        /// <summary>
        /// Sets the console color to use for the specified event type.
        /// </summary>
        /// <param name="eventType">The TraceEventType to set the color for.</param>
        /// <param name="color">The ConsoleColor to use for outputing trace messages of the specified type.</param>
        /// <remarks>
        /// <para>
        /// When selecting colors note that PowerShell redefines DarkYellow and DarkMagenta and uses them
        /// as default colors, so best to to avoid those two colors because result is not consistent.
        /// </para>
        /// </remarks>
        public void SetConsoleColor(TraceEventType eventType, ConsoleColor color)
        {
            lock (_consoleLock)
            {
                if (!Enum.IsDefined(typeof (ConsoleColor), color))
                {
                    throw new ArgumentOutOfRangeException("color", Resource.InvalidConsoleColor);
                }

                var key = eventType.ToString() + "Color";
                Attributes[key] = color.ToString();
            }
        }


        // //////////////////////////////////////////////////////////
        // Protected Methods

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
                WriteColored(TraceEventType.Verbose, message);
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
                WriteLineColored(null, TraceEventType.Verbose, message);
            }
        }

        /// <summary>
        /// Write trace event with data.
        /// </summary>
        protected override void WriteTrace(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message, Guid? relatedActivityId, object[] data)
        {
            /*
            TraceTemplate templateArguments = new TraceTemplate() {
                eventCache = eventCache,
                source = source,
                eventType = eventType,
                id = id,
                message = message,
                relatedActivityId = relatedActivityId,
                data = data,
            };
            string output = StringTemplate.Format(Template, templateArguments.GetArgument);
            */

            var output = TraceFormatter.Format(Template,
                eventCache,
                source,
                eventType,
                id,
                message,
                relatedActivityId,
                data
                );

            WriteLineColored(eventCache, eventType, output);
        }


        // //////////////////////////////////////////////////////////
        // Private

        private static ConsoleColor GetDefaultColor(TraceEventType eventType)
        {
            if (_defaultColorByEventType.ContainsKey(eventType))
            {
                return _defaultColorByEventType[eventType];
            }
            return _defaultColorOther;
        }

        private void SetWriter()
        {
            _writer = _useErrorStream ? _console.Error : _console.Out;
        }

        private void WriteColored(TraceEventType eventType, string message)
        {
            lock (_consoleLock)
            {
                // Set color
                if (!_useErrorStream)
                {
                    ConsoleColor color = GetConsoleColor(eventType);
                    _console.ForegroundColor = color;
                }
                _writer.Write(message);
                // Reset back
                if (!_useErrorStream)
                {
                    _console.ResetColor();
                }
            }
        }

        private void WriteLineColored(TraceEventCache eventCache, TraceEventType eventType, string message)
        {
            lock (_consoleLock)
            {
                // Set color
                if (!_useErrorStream)
                {
                    ConsoleColor color = GetConsoleColor(eventType);
                    _console.ForegroundColor = color;
                }
                // Write log message
                _writer.WriteLine(message);
                WriteFooter(_writer, IndentSize, eventCache);
                // Reset back
                if (!_useErrorStream)
                {
                    _console.ResetColor();
                }
            }
        }

        // Implement footer behaviour of standard trace listeners --
        // Note that normally values like the timestamp, process id, etc, can be
        // put into the format token string, although the footer may still
        // be useful for things like Callstack.
        private void WriteFooter(TextWriter writer, int indentSize, TraceEventCache eventCache)
        {
            if (TraceOutputOptions > 0 && eventCache != null)
            {
                string indent = new string(' ', indentSize);
                if ((TraceOutputOptions & TraceOptions.ProcessId) == TraceOptions.ProcessId)
                {
                    writer.Write(indent);
                    writer.WriteLine("ProcessId=" + eventCache.ProcessId);
                }
                if ((TraceOutputOptions & TraceOptions.LogicalOperationStack) == TraceOptions.LogicalOperationStack)
                {
                    writer.Write(indent);
                    writer.Write("LogicalOperationStack=");
                    Stack logicalOperationStack = eventCache.LogicalOperationStack;
                    bool flag = true;
                    foreach (object obj2 in logicalOperationStack)
                    {
                        if (!flag)
                        {
                            writer.Write(", ");
                        }
                        else
                        {
                            flag = false;
                        }
                        writer.Write(obj2.ToString());
                    }
                    writer.WriteLine(string.Empty);
                }
                if ((TraceOutputOptions & TraceOptions.ThreadId) == TraceOptions.ThreadId)
                {
                    writer.Write(indent);
                    writer.WriteLine("ThreadId=" + eventCache.ThreadId);
                }
                if ((TraceOutputOptions & TraceOptions.DateTime) == TraceOptions.DateTime)
                {
                    writer.Write(indent);
                    writer.WriteLine("DateTime=" + eventCache.DateTime.ToString("o", CultureInfo.InvariantCulture));
                }
                if ((TraceOutputOptions & TraceOptions.Timestamp) == TraceOptions.Timestamp)
                {
                    writer.Write(indent);
                    writer.WriteLine("Timestamp=" + eventCache.Timestamp);
                }
                if ((TraceOutputOptions & TraceOptions.Callstack) == TraceOptions.Callstack)
                {
                    writer.Write(indent);
                    writer.WriteLine("Callstack=" + eventCache.Callstack);
                }
            }
        }

   }
}