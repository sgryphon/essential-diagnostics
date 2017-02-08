using Essential.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Essential.Diagnostics
{
    class TraceData
    {
        //IList<object> _data;
        //IList<object> _messageArgs;
        //object[] _logicalOperationStack;
        //Dictionary<string, object> _properties;

        internal TraceData(DateTimeOffset traceTime, string source, Guid activityId, TraceEventType eventType, int id, 
            string messageFormat, object[] messageArgs, Exception ex, Guid? relatedActivityId, object[] data,
            IDictionary<string, object> properties)
        {
            DateTime = traceTime;
            Source = source;
            ActivityId = activityId;
            EventType = eventType;
            Id = id;
            MessageFormat = messageFormat;
            if (messageArgs != null)
            {
                MessageArgs = Array.AsReadOnly(messageArgs);
            }
            Exception = ex;
            RelatedActivityId = relatedActivityId;
            if (data != null)
            {
                Data = Array.AsReadOnly(data);
            }
            if (properties != null)
            {
                Properties = new ReadOnlyDictionary<string, object>(properties);
            }
        }

        public Guid ActivityId { get; private set; }

        public IList<object> Data { get; private set; }

        public DateTimeOffset DateTime { get; private set; }

        public TraceEventType EventType { get; private set; }

        public Exception Exception { get; private set; }

        public int Id { get; private set; }

        public IList<object> MessageArgs { get; private set; }

        public string MessageFormat { get; private set; }

        public Guid? RelatedActivityId { get; private set; }

        public string Source { get; private set; }

        public IDictionary<string, object> Properties { get; private set; }


    }
}
