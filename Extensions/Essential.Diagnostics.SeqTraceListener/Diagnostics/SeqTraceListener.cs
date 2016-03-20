using Essential.Net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace Essential.Diagnostics
{
    public class SeqTraceListener : TraceListenerBase
    {
        const string BulkUploadResource = "api/events/raw";
        const string ApiKeyHeaderName = "X-Seq-ApiKey";

        IHttpWebRequestFactory _httpWebRequestFactory = new WebRequestAdapter();
        string _serverUrl;

        private static string[] _supportedAttributes = new string[]
        {
            "apiKey", "ApiKey", "apikey",
        };

        /// <summary>
        /// Constructor with initializeData.
        /// </summary>
        /// <param name="serverUrl">URL of the server to write to</param>
        public SeqTraceListener(string serverUrl)
        {
            _serverUrl = serverUrl;
        }

        /// <summary>
        /// Gets or sets the HttpWebRequestFactory to use; this defaults to an adapter for System.Net.WebRequest.
        /// </summary>
        internal IHttpWebRequestFactory HttpWebRequestFactory
        {
            get
            {
                return _httpWebRequestFactory;
            }
            set
            {
                _httpWebRequestFactory = value;
            }
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
        /// A Seq <i>API key</i> that authenticates the client to the Seq server.
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

        /// <summary>
        /// Allowed attributes for this trace listener.
        /// </summary>
        protected override string[] GetSupportedAttributes()
        {
            return _supportedAttributes;
        }

        protected override void WriteTraceFormat(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            var traceData = CreateTraceData(eventCache, source, eventType, id, format, args, null, null);
            PostBatch(new[] { traceData });
        }

        protected override void WriteTrace(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message, Guid? relatedActivityId, object[] data)
        {
            var traceData = CreateTraceData(eventCache, source, eventType, id, message, null, relatedActivityId, data);
            PostBatch(new[] { traceData });
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
            }

            // Activity ID
            var activityId = Trace.CorrelationManager.ActivityId;


            // Optional properties (based on TraceOptions, etc)
            var properties = new Dictionary<string, object>();

            // TraceOptions.Timestamp

            // Callstack
            if ((TraceOutputOptions & TraceOptions.Callstack) == TraceOptions.Callstack)
            {
                if (eventCache != null)
                {
                    properties.Add("Callstack", eventCache.Callstack);
                }
            }

            // Convert stack to string for serialization
            if ((TraceOutputOptions & TraceOptions.LogicalOperationStack) == TraceOptions.LogicalOperationStack)
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

            if ((TraceOutputOptions & TraceOptions.ProcessId) == TraceOptions.ProcessId)
            {
                var processId = eventCache != null ? eventCache.ProcessId : 0;
                properties.Add("ProcessId", processId);
            }

            if ((TraceOutputOptions & TraceOptions.ThreadId) == TraceOptions.ThreadId)
            {
                var threadId = eventCache != null ? eventCache.ThreadId : Thread.CurrentThread.ManagedThreadId.ToString();
                properties.Add("ThreadId", threadId);
            }

            //var thread = Thread.CurrentThread.Name ?? threadId;

            //payload.Properties.Add("Thing", new Thing("Foo"));

            var traceData = new TraceData(traceTime, source, activityId, eventType, id, messageFormat, 
                recordedArgs?.ToArray(), exception, relatedActivityId, recordedData?.ToArray(), properties);
            return traceData;
        }

        private void PostBatch(IEnumerable<TraceData> events)
        {
            if (ServerUrl == null)
                return;

            var uri = ServerUrl;
            if (!uri.EndsWith("/"))
                uri += "/";
            uri += BulkUploadResource;

            //var request = (HttpWebRequest)WebRequest.Create(uri);
            var request = HttpWebRequestFactory.Create(uri);
            request.Method = "POST";
            request.ContentType = "application/json; charset=utf-8";
            //if (!string.IsNullOrWhiteSpace(ApiKey))
            if (!string.IsNullOrEmpty(ApiKey))
            {
                //request.Headers.Add(ApiKeyHeaderName, ApiKey);
                request.AddHeader(ApiKeyHeaderName, ApiKey);
            }

            //var test = new StringWriter();
            //test.Write("{\"events\":[");
            //SeqPayloadFormatter.ToJson(events, test);
            //test.Write("]}");
            //var output = test.ToString();

            using (var requestStream = request.GetRequestStream())
            using (var payload = new StreamWriter(requestStream))
            {
                payload.Write("{\"events\":[");
                SeqPayloadFormatter.ToJson(events, payload);
                payload.Write("]}");
            }

            //using (var response = (HttpWebResponse)request.GetResponse())
            using (var response = request.GetResponse())
            {
                var responseStream = response.GetResponseStream();
                if (responseStream == null)
                    throw new WebException("No response was received from the Seq server");

                using (var reader = new StreamReader(responseStream))
                {
                    var data = reader.ReadToEnd();
                    if ((int)response.StatusCode > 299)
                        throw new WebException(string.Format("Received failed response {0} from Seq server: {1}",
                            response.StatusCode,
                            data));
                }
            }
        }

    }
}
