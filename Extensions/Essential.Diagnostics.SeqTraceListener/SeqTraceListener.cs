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

        protected override void WriteTrace(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message, Guid? relatedActivityId, object[] data)
        {
            var payload = new SeqPayload();
            payload.Source = source;
            payload.EventType = eventType;
            payload.EventId = id;

            payload.MessageTemplate = message;

            payload.RelatedActivityId = relatedActivityId;
            payload.Data = data;

            // TraceOptions.DateTime
            // TraceOptions.Timestamp
            if (eventCache != null)
            {
                payload.EventTime = new DateTimeOffset(eventCache.DateTime);
            }
            else
            {
                payload.EventTime = DateTimeOffset.UtcNow;
            }

            // Callstack
            if ((TraceOutputOptions & TraceOptions.Callstack) == TraceOptions.Callstack)
            {
                if (eventCache != null)
                {
                    payload.Properties.Add("Callstack", eventCache.Callstack);
                }
            }

            // Convert stack to string for serialization
            if ((TraceOutputOptions & TraceOptions.LogicalOperationStack) == TraceOptions.LogicalOperationStack)
            {
                var stack = (eventCache?.LogicalOperationStack) ?? Trace.CorrelationManager.LogicalOperationStack;

                var logicalOperationStack = new List<object>();
                if (stack != null && stack.Count > 0)
                {
                    foreach (object o in stack)
                    {
                        logicalOperationStack.Add(o);
                    }
                    payload.Properties.Add("LogicalOperationStack", logicalOperationStack.ToArray());
                }
            }

            if ((TraceOutputOptions & TraceOptions.ProcessId) == TraceOptions.ProcessId)
            {
                var processId = eventCache != null ? eventCache.ProcessId : 0;
                payload.Properties.Add("ProcessId", processId);
            }

            if ((TraceOutputOptions & TraceOptions.ThreadId) == TraceOptions.ThreadId)
            {
                var threadId = eventCache != null ? eventCache.ThreadId : Thread.CurrentThread.ManagedThreadId.ToString();
                payload.Properties.Add("ThreadId", threadId);
            }

            payload.ActivityId = Trace.CorrelationManager.ActivityId;

            //var thread = Thread.CurrentThread.Name ?? threadId;


            //payload.Properties.Add("Thing", new Thing("Foo"));

            PostBatch(new[] { payload });
        }

        //class Thing
        //{
        //    string value;

        //    public Thing(string value)
        //    {
        //        this.value = value;
        //    }

        //    public override string ToString()
        //    {
        //        return this.value;
        //    }
        //}

        void PostBatch(IEnumerable<SeqPayload> events)
        {
            if (ServerUrl == null)
                return;

            var uri = ServerUrl;
            if (!uri.EndsWith("/"))
                uri += "/";
            uri += BulkUploadResource;

            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "POST";
            request.ContentType = "application/json; charset=utf-8";
            //if (!string.IsNullOrWhiteSpace(ApiKey))
            if (!string.IsNullOrEmpty(ApiKey))
            {
                request.Headers.Add(ApiKeyHeaderName, ApiKey);
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

            using (var response = (HttpWebResponse)request.GetResponse())
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
