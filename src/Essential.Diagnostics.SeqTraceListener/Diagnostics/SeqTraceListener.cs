using Essential.Net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Threading;

namespace Essential.Diagnostics
{
    public class SeqTraceListener : TraceListenerBase
    {
        private const int DefaultBatchSize = 100;
        private static TimeSpan DefaultBatchTimeOut = TimeSpan.FromMilliseconds(1000);
        private const int DefaultMaxQueueSize = 1000;
        private const int DefaultMaxRetries = 10; // 2^10 = 1,024 secs = 17 mins 
        private const bool DefaultSyncErrorHandling = false;

        List<string> _additionalPropertyNames = null;
        SeqBatchSender _batchSender;
        bool _syncErrorHandling;
        bool _syncErrorHandlingParsed;
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
            "processDictionaryData", "ProcessDictionaryData", "processdictionarydata",
            "processDictionaryLogicalOperationStack", "ProcessDictionaryLogicalOperationStack", "processdictionarylogicaloperationstack",
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
        public string ApiKey
        {
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

        /// <summary>
        /// Gets or sets the (maximum) size of batches to send. Use 0 to send each trace individually. Default is 100.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the error handling of sync batches. Use true to catch each exception. Default is false.
        /// </summary>
        public bool SyncErrorHandling
        {
            get
            {
                if (!_syncErrorHandlingParsed)
                {
                    if (Attributes.ContainsKey("syncErrorHandling"))
                    {
                        bool syncErrorHandling;
                        if (bool.TryParse(Attributes["batchSize"], out syncErrorHandling))
                        {
                            _syncErrorHandling = syncErrorHandling;
                        }
                        else
                        {
                            _syncErrorHandling = DefaultSyncErrorHandling;
                        }
                    }
                    else
                    {
                        _syncErrorHandling = DefaultSyncErrorHandling;
                    }
                    _syncErrorHandlingParsed = true;
                }
                return _syncErrorHandling;
            }
            set
            {
                _syncErrorHandling = value;
                _syncErrorHandlingParsed = true;
                Attributes["batchSize"] = value.ToString(CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Gets or sets the timeout to wait before sending incomplete batches. Default is 1 second.
        /// </summary>
        public TimeSpan BatchTimeout
        {
            get
            {
                if (!_batchTimeoutParsed)
                {
                    if (Attributes.ContainsKey("batchTimeout"))
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

        /// <summary>
        /// Gets or sets the maximum number of traces to queue in memory, to limit memory use. Excess traces are discarded. Default is 1000.
        /// </summary>
        public int MaxQueueSize
        {
            get
            {
                if (!_maxQueueSizeParsed)
                {
                    if (Attributes.ContainsKey("maxQueueSize"))
                    {
                        int maxQueueSize;
                        if (int.TryParse(Attributes["maxQueueSize"], NumberStyles.Any,
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
                Attributes["maxQueueSize"] = value.ToString(CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of retries to deliver a batch. If exceeded, the batch is dropped, to prevent poison messages. Default is 10.
        /// </summary>
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
        /// Gets or sets whether a single data of type IDictionary&lt;string,object&gt; is treated as structured data. Default is true.
        /// </summary>
        public bool ProcessDictionaryData
        {
            get
            {
                var processDictionaryData = true;
                if (Attributes.ContainsKey("processDictionaryData"))
                {
                    bool.TryParse(Attributes["processDictionaryData"], out processDictionaryData);
                }
                return processDictionaryData;
            }
            set
            {
                Attributes["processDictionaryData"] = value.ToString(CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Gets or sets whether logical operation stack items of type IDictionary&lt;string,object&gt; are treated as structured data. Default is true.
        /// </summary>
        public bool ProcessDictionaryLogicalOperationStack
        {
            get
            {
                var processDictionaryLogicalOperationStack = true;
                if (Attributes.ContainsKey("processDictionaryLogicalOperationStack"))
                {
                    bool.TryParse(Attributes["processDictionaryLogicalOperationStack"], out processDictionaryLogicalOperationStack);
                }
                return processDictionaryLogicalOperationStack;
            }
            set
            {
                Attributes["processDictionaryLogicalOperationStack"] = value.ToString(CultureInfo.InvariantCulture);
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

        internal SeqBatchSender BatchSender
        {
            get { return _batchSender; }
            set { _batchSender = value; }
        }

        private void AddAttributeProperties(Dictionary<string, object> properties, TraceEventCache eventCache)
        {
            EnsureAttributesParsed();

            if (_propertyCallstack || (TraceOutputOptions & TraceOptions.Callstack) == TraceOptions.Callstack)
            {
                if (eventCache != null)
                {
                    properties.Add("Callstack", eventCache.Callstack);
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
                string principalName = null;
                if (Thread.CurrentPrincipal != null && Thread.CurrentPrincipal.Identity != null)
                {
                    principalName = Thread.CurrentPrincipal.Identity.Name;
                }
                properties.Add("PrincipalName", principalName);
            }
        }

        private void AddLogicalStack(Dictionary<string, object> properties, TraceEventCache eventCache)
        {
            EnsureAttributesParsed();

            var stack = (eventCache != null ? eventCache.LogicalOperationStack : null) ?? Trace.CorrelationManager.LogicalOperationStack;
            if (stack != null && stack.Count > 0)
            {
                var recordStack = _propertyLogicalOperationStack || (TraceOutputOptions & TraceOptions.LogicalOperationStack) == TraceOptions.LogicalOperationStack;
                List<object> logicalOperationStack = null;
                if (recordStack)
                {
                    logicalOperationStack = new List<object>();
                }
                foreach (object stackItem in stack)
                {
                    if ((stackItem is IStructuredData) ||
                        (stackItem is IDictionary<string, object> && ProcessDictionaryLogicalOperationStack))
                    {
                        var stackItemDictionary = (IDictionary<string, object>)stackItem;
                        foreach (var kvp in stackItemDictionary)
                        {
                            if (kvp.Key != StructuredData.MessageTemplateProperty)
                            {
                                properties[kvp.Key] = kvp.Value;
                            }
                        }
                    }
                    if (recordStack)
                    {
                        logicalOperationStack.Add(GetRecordedValue(stackItem));
                    }
                }
                if (recordStack)
                {
                    properties.Add("LogicalOperationStack", logicalOperationStack.ToArray());
                }
            }
        }

        private void AddStructuredData(Dictionary<string, object> properties, IDictionary<string, object> structuredData, ref Exception exception, ref string messageFormat)
        {
            foreach (var kvp in structuredData)
            {
                if (kvp.Key.StartsWith("@"))
                {
                    Dictionary<string, object> destructuredObject = null;
                    if (kvp.Value != null)
                    {
                        destructuredObject = new Dictionary<string, object>();
                        var type = kvp.Value.GetType();
                        var publicProperties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
                        foreach (var propertyInfo in publicProperties)
                        {
                            var propertyValue = propertyInfo.GetValue(kvp.Value, null);
                            destructuredObject[propertyInfo.Name] = GetRecordedValue(propertyValue);
                        }
                    }
                    properties[kvp.Key] = destructuredObject;
                }
                else
                {
                    properties[kvp.Key] = GetRecordedValue(kvp.Value);
                }
                // Grab value if 'Exception'
                if (kvp.Key == "Exception" && kvp.Value is Exception)
                {
                    exception = (Exception)kvp.Value;
                }
            }
            object messageTemplateProperty;
            if (structuredData.TryGetValue(StructuredData.MessageTemplateProperty, out messageTemplateProperty))
            {
                messageFormat = messageTemplateProperty as string;
            }
        }

        private List<object> BuildRecordedArgs(object[] messageArgs, ref Exception exception)
        {
            List<object> recordedArgs = new List<object>();
            foreach (var arg in messageArgs)
            {
                recordedArgs.Add(GetRecordedValue(arg));
                // Grab value if Exception (latest wins)
                if (arg is Exception)
                {
                    exception = (Exception)arg;
                }
            }
            return recordedArgs;
        }

        private List<object> BuildRecordedData(object[] data, ref string messageFormat)
        {
            List<object> recordedData = new List<object>();
            foreach (var dataItem in data)
            {
                recordedData.Add(GetRecordedValue(dataItem));
            }
            // If message format not set, display the data
            if (messageFormat == null)
            {
                messageFormat = "{Data}";
            }
            return recordedData;
        }

        private TraceData CreateTraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string messageFormat, object[] messageArgs, Guid? relatedActivityId, object[] data)
        {
            var traceTime = default(DateTimeOffset);
            if (eventCache != null)
            {
                traceTime = new DateTimeOffset(eventCache.DateTime);
            }
            else
            {
                traceTime = DateTimeOffset.UtcNow;
            }
            var activityId = Trace.CorrelationManager.ActivityId;

            // Properties
            var properties = new Dictionary<string, object>();
            AddAttributeProperties(properties, eventCache);
            AddLogicalStack(properties, eventCache);

            object[] recordedArgsArray = null;
            var exception = default(Exception);
            object[] recordedDataArray = null;
            if (messageFormat == null
                && (messageArgs == null || messageArgs.Length == 0)
                && data != null
                && data.Length == 1
                && (data[0] is IStructuredData ||
                    (data[0] is IDictionary<string, object> && ProcessDictionaryData)))
            {
                var structuredData = (IDictionary<string, object>)data[0];
                AddStructuredData(properties, structuredData, ref exception, ref messageFormat);
            }
            else
            {
                if (messageArgs != null)
                {
                    var recordedArgs = BuildRecordedArgs(messageArgs, ref exception);
                    recordedArgsArray = recordedArgs.ToArray();
                }
                if (data != null)
                {
                    var recordedData = BuildRecordedData(data, ref messageFormat);
                    recordedDataArray = recordedData.ToArray();
                }
            }

            var traceData = new TraceData(traceTime, source, activityId, eventType, id, messageFormat,
                recordedArgsArray, exception, relatedActivityId, recordedDataArray, properties);
            return traceData;
        }

        private void EnsureAttributesParsed()
        {
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
        }

        private object GetRecordedValue(object value)
        {
            if (IsFormatterLiteral(value))
            {
                return value;
            }
            // TODO: Should convert child dictionary/list into recorded values
            if (value is IDictionary<string, object>)
            {
                return value;
            }
            if (value is IList)
            {
                return value;
            }
            return value.ToString();
        }

        private bool IsFormatterLiteral(object value)
        {
            return SeqPayloadFormatter.IsLiteral(value);
        }
    }
}
