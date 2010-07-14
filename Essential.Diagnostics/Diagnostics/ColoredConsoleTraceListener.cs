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
    /// Trace listener that outputs to the Console in color. 
    /// </summary>
    public class ColoredConsoleTraceListener : TraceListenerBase
    {
        // //////////////////////////////////////////////////////////
        // Fields

        private Dictionary<TraceEventType, ConsoleColor> _colorByEventType = new Dictionary<TraceEventType, ConsoleColor>();
        private bool _convertWriteToText;
        private bool _convertWriteToTextInitialized;
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
        private string _template;
        private static string[] _supportedAttributes = new string[] 
            { 
                "template", "Template", "convertWriteToEvent", "convertWriteToEvent",
                "fatalColor", "FatalColor", "fatalcolor", 
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
        private TextWriter _writer;
        bool _useErrorStream;


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
            // Behaviour consistent with System.Diagnostics.ConsoleTraceListener
            // -- initializeData determines whether stdout or stderr is used.
            _useErrorStream = useErrorStream;
            _writer = _useErrorStream ? Console.Error : Console.Out;
        }


        // //////////////////////////////////////////////////////////
        // Public Properties

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
                if( ! _convertWriteToTextInitialized )
                {
                    if (Attributes.ContainsKey("convertWriteToEvent"))
                    {
                        // Default is false
                        bool.TryParse(Attributes["convertWriteToEvent"], out _convertWriteToText);
                    }
                    _convertWriteToTextInitialized = true;
                }
                return _convertWriteToText;
            }
            set
            {
                _convertWriteToText = value;
                _convertWriteToTextInitialized = true;
            }
        }

        /// <summary>
        /// Gets or sets the token format string to use to display trace output.
        /// </summary>
        public string Template
        {
            get
            {
                // Default format matches System.Diagnostics.TraceListener
                if (_template == null)
                {
                    if (Attributes.ContainsKey("template"))
                    {
                        _template = Attributes["template"];
                    }
                    else
                    {
                        _template = _defaultTemplate;
                    }
                }
                return _template;
            }
            set
            {
                _template = value;
            }
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
            ConsoleColor color;

            if (_colorByEventType.ContainsKey(eventType))
            {
                color = _colorByEventType[eventType];
            }
            else
            {
                string key = eventType.ToString() + "Color";
                if (Attributes.ContainsKey(key))
                {
                    string setting = Attributes[key];
                    if (Enum.IsDefined(typeof(ConsoleColor), setting))
                    {
                        color = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), setting);
                    }
                    else
                    {
                        color = GetDefaultColor(eventType);
                    }
                }
                else
                {
                    if (((int)eventType & (int)SourceLevels.ActivityTracing) > 0
                        && Attributes.ContainsKey("ActivityTracingColor"))
                    {
                        string setting = Attributes["ActivityTracingColor"];
                        if (Enum.IsDefined(typeof(ConsoleColor), setting))
                        {
                            color = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), setting);
                        }
                        else
                        {
                            color = GetDefaultColor(eventType);
                        }
                    }
                    else
                    {
                        color = GetDefaultColor(eventType);
                    }
                }
                _colorByEventType[eventType] = color;
            }
            return color;
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
            if (!Enum.IsDefined(typeof(ConsoleColor),color))
            {
                throw new ArgumentOutOfRangeException("color", Resource.InvalidConsoleColor);
            }
            _colorByEventType[eventType] = color;
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
                // Set color
                if (!_useErrorStream)
                {
                    ConsoleColor color = GetConsoleColor(TraceEventType.Verbose);
                    Console.ForegroundColor = color;
                }
                _writer.Write(message);
                // Reset back
                if (!_useErrorStream)
                {
                    Console.ResetColor();
                }
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
                WriteColored(null, TraceEventType.Verbose, message);
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

            var output = TraceTemplate.Format(Template,
                eventCache,
                source,
                eventType,
                id,
                message,
                relatedActivityId,
                data
                );

            WriteColored(eventCache, eventType, output);
        }


        // //////////////////////////////////////////////////////////
        // Private

        private static ConsoleColor GetDefaultColor(TraceEventType eventType)
        {
            ConsoleColor defaultForEvent;
            if (_defaultColorByEventType.ContainsKey(eventType))
            {
                defaultForEvent = _defaultColorByEventType[eventType];
            }
            else
            {
                defaultForEvent = _defaultColorOther;
            }
            return defaultForEvent;
        }

        private void WriteColored(TraceEventCache eventCache, TraceEventType eventType, string message)
        {
            // Set color
            if (!_useErrorStream)
            {
                ConsoleColor color = GetConsoleColor(eventType);
                Console.ForegroundColor = color;
            }
            // Write log message
            _writer.WriteLine(message);
            WriteFooter(_writer, IndentSize, eventCache);
            // Reset back
            if (!_useErrorStream)
            {
                Console.ResetColor();
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