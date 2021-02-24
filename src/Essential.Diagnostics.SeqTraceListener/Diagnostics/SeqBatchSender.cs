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
            // Batch size 0 sends immediately (synchronous), with no error handling. 
            // Batch size 1 sends one at a time, but async, including retry, etc.
            if (_associatedTraceListener.BatchSize > 0)
            {
                EnqueueTraceData(traceData);
            }
            else
            {
                try
                {
                    PostBatch(new[] { traceData });
                }
                catch (Exception ex)
                {
                    if (!_associatedTraceListener.IndividualSendIgnoreErrors)
                    {
                        throw;
                    }
                    else
                    {
                        if (Console.Error != null)
                            Console.Error.WriteLine($"SeqBatchSender exception sending batch, exception supressed: {ex.Message}");
                    }
                }
            }
        }


        // Get a batch
        // Try and send it
        //    If not, wait, and retry
        // Once sent, see if there is another batch, is so, repeat above
        // If not, wait timeout (unless it fills up before!)
        // If any queue, treat as a batch, repeat above
        // If none queued, end.

        internal IHttpWebRequestFactory HttpWebRequestFactory
        {
            get { return _httpWebRequestFactory; }
            set { _httpWebRequestFactory = value; }
        }

        // State: process running or not 
        //    vs has items in queue or not

        private void EnqueueTraceData(TraceData traceData)
        {
            //Console.WriteLine(string.Format("Enqueue {0}", traceData.Id));

            lock (stateLock)
            {
                if (queue.Count < _associatedTraceListener.MaxQueueSize)
                {
                    queue.Enqueue(traceData);
                }
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
                    // Trigger to send immediately for first message,
                    // (or the first message after a wait) 
                    // so the server knows we are up and running.
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
            //Console.WriteLine("Process started");

            bool finished = false;
            List<TraceData> currentBatch = new List<TraceData>();
            int retryCount = 0;
            TimeSpan retryTimeout = TimeSpan.Zero;

            while (!finished)
            {
                if (currentBatch.Count > 0)
                {
                    //Console.WriteLine("Wait retry timeout");
                    // Wait retry timeout.
                    Thread.Sleep(retryTimeout);
                }
                else
                {
                    //Console.WriteLine("Wait next check or trigger");
                    // Wait for next check (unless triggered early)
                    var triggered = sendTrigger.WaitOne(_associatedTraceListener.BatchTimeout);
                }

                // If we don't already have a batch, try and get one
                if (currentBatch.Count == 0)
                {
                    //Console.WriteLine("Getting batch from queue");
                    lock (stateLock)
                    {
                        if (queue.Count > 0)
                        {
                            // Dequeue items and add to batch
                            var count = _associatedTraceListener.BatchSize;
                            if (queue.Count < count)
                            {
                                count = queue.Count;
                            }
                            for (var i = 0; i < count; i++)
                            {
                                var item = queue.Dequeue();
                                currentBatch.Add(item);
                            }
                        }
                        else
                        {
                            //Console.WriteLine("Finish");
                            // Tried to get batch, but nothing there:
                            // So finish (local variable to this thread)
                            finished = true;
                            // Also indicate that queue can start new thread
                            isProcessing = false;
                        }
                    }
                }

                // Actually send the batch outside the lock
                if (!finished)
                {
                    var success = false;
                    try
                    {
                        success = PostBatch(currentBatch, false);
                    }
                    catch (Exception ex)
                    {
                        if (Console.Error != null)
                            Console.Error.WriteLine(string.Format("SeqBatchSender exception sending batch: {0}", ex.Message));
                    }
                    // Retry when batch fails
                    if (success)
                    {
                        //Console.WriteLine("Post batch success");
                        currentBatch.Clear();
                        retryCount = 0;
                        retryTimeout = TimeSpan.Zero;
                    }
                    else
                    {
                        if (retryCount >= _associatedTraceListener.MaxRetries)
                        {
                            if (Console.Error != null)
                                Console.Error.WriteLine("SeqBatchSender exceeded retry count; abandoning batch.");
                            currentBatch.Clear();
                            retryCount = 0;
                            retryTimeout = TimeSpan.Zero;
                            // Want to try next batch immediately
                            sendTrigger.Set();
                        }
                        else
                        {
                            if (retryTimeout == TimeSpan.Zero)
                            {
                                retryCount = 1;
                                retryTimeout = _associatedTraceListener.BatchTimeout;
                            }
                            else
                            {
                                retryCount++;
                                retryTimeout = retryTimeout + retryTimeout;
                            }
                            // Can't really trace this anywhere else
                            if (Console.Error != null)
                                Console.Error.WriteLine(string.Format("SeqBatchSender retry {0} with timeout {1}", retryCount, retryTimeout));
                        }
                    }
                }
            }
        }

        private bool PostBatch(IEnumerable<TraceData> events, bool shouldThrow = true)
        {
            if (_associatedTraceListener.ServerUrl == null)
                return true;

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
                    if ((int)response.StatusCode >= 300)
                    {
                        if (shouldThrow)
                        {
                            throw new WebException(string.Format("Received failed response {0} from Seq server: {1}",
                                response.StatusCode,
                                data));
                        }
                        return false;
                    }
                    return true;
                }
            }
        }
    }
}
