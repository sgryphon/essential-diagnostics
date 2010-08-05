using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections;

namespace Essential.Diagnostics
{
    /// <summary>
    /// Records trace events in memory, within a circular buffer. 
    /// The buffer limit can be set by the initialization data.
    /// </summary>
    public class InMemoryTraceListener : TraceListenerBase
    {
        private const int DefaultSize = 20;

        private int _current;
        private TraceEvent[] _events;
        private object _eventsLock = new object();

        /// <summary>
        /// Constructor. Creates a listener with the default buffer size (20).
        /// </summary>
        public InMemoryTraceListener()
            : this(DefaultSize)
        {
        }

        /// <summary>
        /// Constructor. Creates a listener with the specified buffer size.
        /// </summary>
        public InMemoryTraceListener(int limit)
        {
            _events = new TraceEvent[limit];
        }

        /// <summary>
        /// Gets the current buffer size.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Note that the size of the buffer cannot be changed once the listener
        /// has been created. If the initialization data is changed, a new instance
        /// of the listener is created, i.e. existing data will be lost.
        /// </para>
        /// </remarks>
        public int Limit
        {
            get { return _events.Length; }
        }

        public override bool IsThreadSafe
        {
            get { return true; }
        }

        /// <summary>
        /// Clears the buffer.
        /// </summary>
        public void Clear()
        {
            lock (_eventsLock)
            {
                _events = new TraceEvent[_events.Length];
                _current = 0;
            }
        }

        /// <summary>
        /// Gets an array of the current events in the buffer.
        /// </summary>
        public TraceEvent[] GetEvents()
        {
            var events = new List<TraceEvent>();
            lock (_eventsLock)
            {
                for (var index = _current; index < _events.Length; index++)
                {
                    if (_events[index] != null)
                    {
                        events.Add(_events[index]);
                    }
                }
                for (var index = 0; index < _current; index++ )
                {
                    events.Add(_events[index]);
                }
            }
            return events.ToArray();
        }

        protected override void WriteTrace(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message, Guid? relatedActivityId, object[] data)
        {
            lock (_eventsLock)
            {
                var activityId = Trace.CorrelationManager.ActivityId;
                var traceTime = (eventCache == null) ? DateTimeOffset.UtcNow : eventCache.DateTime;
                var currentStack = (eventCache == null)
                                ? Trace.CorrelationManager.LogicalOperationStack
                                : eventCache.LogicalOperationStack;
                // Want to copy the stack as the original object will change.
                // Also, don't need stack behaviour (push, pop) for the copy -- just want a record of the contents at the time.
                var recordedStack = currentStack.ToArray();
                var trace = new TraceEvent(traceTime, source, activityId, eventType, id, message, relatedActivityId, recordedStack, data);

                _events[_current] = trace;
                _current++;
                if (_current >= _events.Length)
                {
                    _current = 0;
                }
            }
        }

        /// <summary>
        /// Details of a single trace event.
        /// </summary>
        public class TraceEvent
        {
            public TraceEvent(DateTimeOffset traceTime, string source, Guid activityId, TraceEventType eventType, int id, string message, Guid? relatedActivityId, object[] logicalOperationStack, object[] data)
            {
                DateTime = traceTime;
                Source = source;
                ActivityId = activityId;
                EventType = eventType;
                Id = id;
                Message = message;
                RelatedActivityId = relatedActivityId;
                LogicalOperationStack = logicalOperationStack;
                Data = data;
            }

            /// <summary>
            /// Gets the point in time the trace event was recorded.
            /// </summary>
            public DateTimeOffset DateTime { get; private set; }

            /// <summary>
            /// Gets the source used to write the trace event.
            /// </summary>
            public string Source { get; private set; }

            /// <summary>
            /// Gets the current activity the time of the event.
            /// </summary>
            public Guid ActivityId { get; private set; }

            /// <summary>
            /// Gets the type of the event.
            /// </summary>
            public TraceEventType EventType { get; private set; }

            /// <summary>
            /// Gets the numeric id of the event.
            /// </summary>
            public int Id { get; private set; }

            /// <summary>
            /// Gets the formatted message for the event.
            /// </summary>
            public string Message { get; private set; }

            /// <summary>
            /// Gets the related activity being transferred to (for transfer events).
            /// </summary>
            public Guid? RelatedActivityId { get; private set; }

            /// <summary>
            /// Gets the logical operation stack at the time of the event.
            /// </summary>
            /// <remarks>
            /// <para>
            /// Note that the most recent item (the top of the stack) is
            /// listed first.
            /// </para>
            /// </remarks>
            public object[] LogicalOperationStack { get; private set; }

            /// <summary>
            /// Gets additional data items for the event.
            /// </summary>
            public object[] Data { get; private set; }
        }
    }
}
