using Essential.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace Essential.Diagnostics
{
    class SeqBatchSender
    {
        const string BulkUploadResource = "api/events/raw";
        const string ApiKeyHeaderName = "X-Seq-ApiKey";

        SeqTraceListener _associatedTraceListener;
        IHttpWebRequestFactory _httpWebRequestFactory;
        
        bool isProcessing = false;
        Queue<TraceData> queue = new Queue<TraceData>();
        AutoResetEvent sendTrigger = new AutoResetEvent(false);
        object stateLock = new object();

        public SeqBatchSender(SeqTraceListener associatedTraceListener, IHttpWebRequestFactory httpWebRequestFactory)
        {
            _associatedTraceListener = associatedTraceListener;
            _httpWebRequestFactory = httpWebRequestFactory;
        }

        public void Enqueue(TraceData traceData)
        {
            // Batch size 0 sends immediately (synchronous); batch size 1 sends one at a time, but async.
            if (_associatedTraceListener.BatchSize > 0)
            {
                EnqueueTraceData(traceData);
            }
            else
            {
                PostBatch(new[] { traceData });
            }
        }

        internal IHttpWebRequestFactory HttpWebRequestFactory
        {
            get { return _httpWebRequestFactory; }
            set { _httpWebRequestFactory = value; }
        }

        // State: process running or not 
        //    vs has items in queue or not

        private void EnqueueTraceData(TraceData traceData)
        {
            lock (stateLock)
            {
                queue.Enqueue(traceData);
                if (isProcessing)
                {
                    if (queue.Count >= _associatedTraceListener.BatchSize)
                    {
                        sendTrigger.Set();
                    }
                }
                else
                {
                    isProcessing = true;
                    // Trigger to send immediately
                    sendTrigger.Set();
                    var queued = ThreadPool.QueueUserWorkItem(delegate
                    {
                        Process();
                    });
                }
            }
        }

        private void Process()
        {
            bool finished = false;
            while (!finished)
            {
                // Wait for next check (unless triggered early)
                var triggered = sendTrigger.WaitOne(_associatedTraceListener.BatchTimeout);

                IEnumerable<TraceData> eventsToSend = null;
                lock (stateLock)
                {
                    if (queue.Count > 0)
                    {
                        // Copy and then clear queue
                        eventsToSend = queue.ToArray();
                        queue.Clear();
                    }
                    else
                    {
                        // Nothing to process, so finish
                        isProcessing = false;
                        finished = true;
                    }
                }

                // Actually send the batch outside the lock
                if (eventsToSend != null)
                {
                    // TODO: Logic to handle when batch fails
                    PostBatch(eventsToSend);
                }
            }
        }

        private void PostBatch(IEnumerable<TraceData> events)
        {
            if (_associatedTraceListener.ServerUrl == null)
                return;

            var uri = _associatedTraceListener.ServerUrl;
            if (!uri.EndsWith("/"))
                uri += "/";
            uri += BulkUploadResource;

            //var request = (HttpWebRequest)WebRequest.Create(uri);
            var request = _httpWebRequestFactory.Create(uri);
            request.Method = "POST";
            request.ContentType = "application/json; charset=utf-8";
            //if (!string.IsNullOrWhiteSpace(ApiKey))
            if (!string.IsNullOrEmpty(_associatedTraceListener.ApiKey))
            {
                //request.Headers.Add(ApiKeyHeaderName, ApiKey);
                request.AddHeader(ApiKeyHeaderName, _associatedTraceListener.ApiKey);
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
