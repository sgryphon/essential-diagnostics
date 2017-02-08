using Essential.Net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace Essential.Diagnostics
{
    public class SeqTraceListener : TraceListenerBase
    {
        private const int DefaultBatchSize = 100;
        private static TimeSpan DefaultBatchTimeOut = TimeSpan.FromMilliseconds(1000);
        private const int DefaultMaxQueueSize = 1000;
        private const int DefaultMaxRetries = 10; // 2^10 = 1,024 secs = 17 mins 

        List<string> _additionalPropertyNames = null;
        SeqBatchSender _batchSender;
        int _batchSize;
        bool _batchSizeParsed;
        TimeSpan _batchTimeout;
        bool _batchTimeoutParsed;
        int _maxQueueSize;
        bool _maxQueueSizeParsed;
        int _maxRetries;
        bool _maxRetriesParsed;
        bool _propertiesParsed;
        bool _propertyCallstack;
        bool _propertyLogicalOperationStack;
        bool _propertyMachineName;
        bool _propertyPrincipalName;
        bool _propertyProcessId;
        bool _propertyThreadId;
        bool _propertyUser;
        string _serverUrl;

        private static string[] _supportedAttributes = new string[]
        {
            "apiKey", "ApiKey", "apikey",
            "additionalProperties", "AdditionalProperties", "additionalproperties",
            "batchSize", "BatchSize", "batchsize",
            "batchTimeout", "BatchTimeout", "batchtimeout", "batchTimeOut", "BatchTimeOut",
            "maxQueueSize", "MaxQueueSize", "maxqueuesize",
            "maxRetries", "MaxRetries", "maxretries",
        };

        /// <summary>
        /// Constructor with initializeData.
        /// </summary>
        /// <param name="serverUrl">URL of the server to write to</param>
        public SeqTraceListener(string serverUrl)
        {
            _serverUrl = serverUrl;
            _batchSender = new SeqBatchSender(this, new WebRequestAdapter());
        }

        /// <summary>
        /// Gets the address of the Seq server to write to.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value is part of initializeData; if the value changes the
        /// listener is recreated. See the constructor parameter for details
        /// of the supported formats.
        /// </para>
        /// </remarks>
        public string ServerUrl
        {
            get { return _serverUrl; }
        }

        /// <summary>
        /// Gets or sets the comma separated names of additional properties that should be sent to Seq.
        /// </summary>
        public string[] AdditionalProperties
        {
            get
            {
                if (_additionalPropertyNames == null)
                {
                    _additionalPropertyNames = new List<string>();
                    if (Attributes.ContainsKey("additionalproperties"))
                    {
                        var propertyNamesAttribute = Attributes["additionalproperties"];
                        var propertyNames = propertyNamesAttribute.Split(',');
                        foreach (var propertyName in propertyNames)
                        {
                            _additionalPropertyNames.Add(propertyName.Trim());
                        }
                    }
                }
                return _additionalPropertyNames.ToArray();
            }
            set
            {
                _additionalPropertyNames = new List<string>();
                _additionalPropertyNames.AddRange(value);
                var propertyNamesAttributes = string.Join(",", value);
                Attributes["additionalproperties"] = propertyNamesAttributes;
            }
        }

        /// <summary>
        /// Gets or sets the Seq <i>API key</i> that authenticates the client to the Seq server.
        /// </summary>
        public string ApiKey {
            get
            {
                if (Attributes.ContainsKey("apikey"))
                {
                    return Attributes["apikey"];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                Attributes["apikey"] = value;
            }
        }

        public int BatchSize
        {
            get
            {
                if (!_batchSizeParsed)
                {
                    if (Attributes.ContainsKey("batchSize"))
                    {
                        int batchSize;
                        if (int.TryParse(Attributes["batchSize"], NumberStyles.Any,
                            CultureInfo.InvariantCulture, out batchSize))
                        {
                            _batchSize = batchSize;
                        }
                        else
                        {
                            _batchSize = DefaultBatchSize;
                        }
                    }
                    else
                    {
                        _batchSize = DefaultBatchSize;
                    }
                    _batchSizeParsed = true;
                }
                return _batchSize;
            }
            set
            {
                _batchSize = value;
                _batchSizeParsed = true;
                Attributes["batchSize"] = value.ToString(CultureInfo.InvariantCulture);
            }
        }

        public TimeSpan BatchTimeout
        {
            get
            {
                if (!_batchTimeoutParsed)
                {
                    if (Attributes.ContainsKey("batchSize"))
                    {
                        TimeSpan batchTimeout;
                        if (TimeSpan.TryParse(Attributes["batchTimeout"], out batchTimeout))
                        {
                            _batchTimeout = batchTimeout;
                        }
                        else
                        {
                            _batchTimeout = DefaultBatchTimeOut;
                        }
                    }
                    else
                    {
                        _batchTimeout = DefaultBatchTimeOut;
                    }
                    _batchTimeoutParsed = true;
                }
                return _batchTimeout;
            }
            set
            {
                _batchTimeout = value;
                _batchTimeoutParsed = true;
                Attributes["batchTimeout"] = value.ToString();
            }
        }

        public int MaxQueueSize
        {
            get
            {
                if (!_maxQueueSizeParsed)
                {
                    if (Attributes.ContainsKey("maxBatchSize"))
                    {
                        int maxQueueSize;
                        if (int.TryParse(Attributes["maxBatchSize"], NumberStyles.Any,
                            CultureInfo.InvariantCulture, out maxQueueSize))
                        {
                            _maxQueueSize = maxQueueSize;
                        }
                        else
                        {
                            _maxQueueSize = DefaultMaxQueueSize;
                        }
                    }
                    else
                    {
                        _maxQueueSize = DefaultMaxQueueSize;
                    }
                    _maxQueueSizeParsed = true;
                }
                return _maxQueueSize;
            }
            set
            {
                _maxQueueSize = value;
                _maxQueueSizeParsed = true;
                Attributes["maxBatchSize"] = value.ToString(CultureInfo.InvariantCulture);
            }
        }

        public int MaxRetries
        {
            get
            {
                if (!_maxRetriesParsed)
                {
                    if (Attributes.ContainsKey("maxRetries"))
                    {
                        int maxRetries;
                        if (int.TryParse(Attributes["maxRetries"], NumberStyles.Any,
                            CultureInfo.InvariantCulture, out maxRetries))
                        {
                            _maxRetries = maxRetries;
                        }
                        else
                        {
                            _maxRetries = DefaultMaxRetries;
                        }
                    }
                    else
                    {
                        _maxRetries = DefaultMaxRetries;
                    }
                    _maxRetriesParsed = true;
                }
                return _maxRetries;
            }
            set
            {
                _maxRetries = value;
                _maxRetriesParsed = true;
                Attributes["maxRetries"] = value.ToString(CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Allowed attributes for this trace listener.
        /// </summary>
        protected override string[] GetSupportedAttributes()
        {
            return _supportedAttributes;
        }
        internal SeqBatchSender BatchSender
        {
            get { return _batchSender; }
            set { _batchSender = value; }
        }

        /// <summary>
        /// Handle the format strings
        /// before the args are resolved.
        /// </summary>
        protected override void WriteTraceFormat(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            var traceData = CreateTraceData(eventCache, source, eventType, id, format, args, null, null);
            _batchSender.Enqueue(traceData);
        }

        /// <summary>
        /// Write the trace to the listener output.
        /// </summary>
        protected override void WriteTrace(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message, Guid? relatedActivityId, object[] data)
        {
            var traceData = CreateTraceData(eventCache, source, eventType, id, message, null, relatedActivityId, data);
            _batchSender.Enqueue(traceData);
        }

        private TraceData CreateTraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string messageFormat, object[] messageArgs, Guid? relatedActivityId, object[] data)
        {
            // Standard properies (always record)

            // TraceOptions.DateTime
            var traceTime = default(DateTimeOffset);
            if (eventCache != null)
            {
                traceTime = new DateTimeOffset(eventCache.DateTime);
            }
            else
            {
                traceTime = DateTimeOffset.UtcNow;
            }

            // Record Message Args
            var recordedArgs = default(List<object>);
            var exception = default(Exception);
            if (messageArgs != null)
            {
                recordedArgs = new List<object>();
                foreach (var arg in messageArgs)
                {
                    if (arg is bool || arg is char || arg is byte || arg is sbyte
                        || arg is short || arg is ushort || arg is int || arg is uint
                        || arg is long || arg is ulong || arg is float || arg is double
                        || arg is decimal || arg is DateTime || arg is DateTimeOffset
                        || arg is string)
                    {
                        recordedArgs.Add(arg);
                    }
                    else if (arg is Exception)
                    {
                        exception = (Exception)arg;
                        recordedArgs.Add(arg.ToString());
                    }
                    else
                    {
                        // TODO: Should really take into account the format specifier in the formatString
                        // (before serializing)
                        recordedArgs.Add(arg.ToString());
                    }
                }
            }

            // Record Data
            var recordedData = default(List<object>);
            if (data != null)
            {
                recordedData = new List<object>();
                foreach (var dataItem in data)
                {
                    if (dataItem is bool || dataItem is char || dataItem is byte || dataItem is sbyte
                        || dataItem is short || dataItem is ushort || dataItem is int || dataItem is uint
                        || dataItem is long || dataItem is ulong || dataItem is float || dataItem is double
                        || dataItem is decimal || dataItem is DateTime || dataItem is DateTimeOffset
                        || dataItem is string)
                    {
                        recordedData.Add(dataItem);
                    }
                    else
                    {
                        recordedData.Add(dataItem.ToString());
                    }
                }
                if (messageFormat == null)
                {
                    messageFormat = "{Data}";
                }
            }

            // Activity ID
            var activityId = Trace.CorrelationManager.ActivityId;

            // Optional properties (based on TraceOptions, etc)
            var properties = new Dictionary<string, object>();

            if (!_propertiesParsed)
            {
                foreach (var propertyName in AdditionalProperties)
                {
                    switch (propertyName.ToUpperInvariant())
                    {
                        case "CALLSTACK":
                            _propertyCallstack = true;
                            break;
                        case "LOGICALOPERATIONSTACK":
                            _propertyLogicalOperationStack = true;
                            break;
                        case "MACHINENAME":
                            _propertyMachineName = true;
                            break;
                        case "PRINCIPALNAME":
                            _propertyPrincipalName = true;
                            break;
                        case "PROCESSID":
                            _propertyProcessId = true;
                            break;
                        case "THREADID":
                            _propertyThreadId = true;
                            break;
                        case "USER":
                            _propertyUser = true;
                            break;
                    }
                }
                _propertiesParsed = true;
            }

            // TraceOptions.Timestamp

            // Callstack
            if (_propertyCallstack || (TraceOutputOptions & TraceOptions.Callstack) == TraceOptions.Callstack)
            {
                if (eventCache != null)
                {
                    properties.Add("Callstack", eventCache.Callstack);
                }
            }

            // Convert stack to string for serialization
            if (_propertyLogicalOperationStack || (TraceOutputOptions & TraceOptions.LogicalOperationStack) == TraceOptions.LogicalOperationStack)
            {
                var stack = (eventCache?.LogicalOperationStack) ?? Trace.CorrelationManager.LogicalOperationStack;

                var logicalOperationStack = new List<object>();
                if (stack != null && stack.Count > 0)
                {
                    foreach (object stackItem in stack)
                    {
                        if (stackItem is bool || stackItem is char || stackItem is byte || stackItem is sbyte
                            || stackItem is short || stackItem is ushort || stackItem is int || stackItem is uint
                            || stackItem is long || stackItem is ulong || stackItem is float || stackItem is double
                            || stackItem is decimal || stackItem is DateTime || stackItem is DateTimeOffset
                            || stackItem is string)
                        {
                            logicalOperationStack.Add(stackItem);
                        }
                        else
                        {
                            logicalOperationStack.Add(stackItem.ToString());
                        }
                    }
                    properties.Add("LogicalOperationStack", logicalOperationStack.ToArray());
                }
            }

            if (_propertyProcessId || (TraceOutputOptions & TraceOptions.ProcessId) == TraceOptions.ProcessId)
            {
                var processId = eventCache != null ? eventCache.ProcessId : 0;
                properties.Add("ProcessId", processId);
            }

            if (_propertyThreadId || (TraceOutputOptions & TraceOptions.ThreadId) == TraceOptions.ThreadId)
            {
                var threadId = eventCache != null ? eventCache.ThreadId : Thread.CurrentThread.ManagedThreadId.ToString();
                properties.Add("ThreadId", threadId);
            }

            if (_propertyMachineName)
            {
                properties.Add("MachineName", Environment.MachineName);
            }

            if (_propertyUser)
            {
                properties.Add("User", Environment.UserDomainName + "\\" + Environment.UserName);
            }

            if (_propertyPrincipalName)
            {
                properties.Add("PrincipalName", Thread.CurrentPrincipal?.Identity?.Name);
            }

            //var thread = Thread.CurrentThread.Name ?? threadId;

            //payload.Properties.Add("Thing", new Thing("Foo"));

            var traceData = new TraceData(traceTime, source, activityId, eventType, id, messageFormat, 
                recordedArgs?.ToArray(), exception, relatedActivityId, recordedData?.ToArray(), properties);
            return traceData;
        }


    }
}
