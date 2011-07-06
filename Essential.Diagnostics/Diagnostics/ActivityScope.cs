using System;
using System.Diagnostics;

namespace Essential.Diagnostics
{
    /// <summary>
    /// Sets the correlation manager ActivityId for the life of the object,
    /// resetting it when disposed, and optionally logging activity messages.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The sequence of events follows the convention used in WCF logging.
    /// </para>
    /// <para>
    /// When created, the object logs a Transfer event, changes the ActivityId,
    /// and then logs a Start event.
    /// </para>
    /// <para>
    /// When disposed, the object logs a Transfer event (back to the original),
    /// a Stop event, and then changes the ActivityId (back to the original).
    /// </para>
    /// </remarks>
    public class ActivityScope : IDisposable
    {
        // NOTE: Seems overly gratuitous logging, i.e. little distinction 
        // between transfer/start, and transfer/stop)

        // Principles:
        // 1. Need ActivityId to correlate at all, so outer wrapper must be new activity.
        // 2. ActivityId transfer also allows asynchronous actions to also be correlated
        //    i.e. where there is not necessarily a transfer back, such as separate threads.
        // 3. For simple stack operations, i.e. method calls, the LogicalOperationStack may be simpler.

        // Think about activity, with logical op stack inside, and then a transfer...?
        // Activities are kind of like [logical] threads vs logical op stack like the call stack.

        // WCF sets an ActivityId for you, but with other contexts it may not be set.

        Guid _previousActivityId;
        TraceSource _source;
        int _startId;
        int _stopId;
        int _transferInId;
        int _transferOutId;

        /// <summary>
        /// Constructor. Sets the ActivityId for the life of the object without logging any events.
        /// </summary>
        public ActivityScope() : this(null, 0, 0, 0, 0)
        {
        }

        /// <summary>
        /// Constructor. Sets the ActivityId for the life of the object, logging events but without specific event ID's.
        /// </summary>
        public ActivityScope(TraceSource source) : this(source, 0, 0, 0, 0)
        {
        }

        /// <summary>
        /// Constructor. Sets the ActivityId for the life of the object, logging events with the specified event ID's.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public ActivityScope(TraceSource source, int transferInId, int startId, int transferOutId, int stopId)
        {
            _source = source;
            _startId = startId;
            _stopId = stopId;
            _transferInId = transferInId;
            _transferOutId = transferOutId;

            _previousActivityId = Trace.CorrelationManager.ActivityId;

            // Log Transfer In
            Guid newActivity = Guid.NewGuid();
            if( _source != null ) 
            {
                _source.TraceTransfer(_transferInId, Resource.ActivityScope_Transfer, newActivity);
            }
            Trace.CorrelationManager.ActivityId = newActivity;

            // Log Start Message
            if( _source != null ) 
            {
                _source.TraceEvent(TraceEventType.Start, _startId, Resource.ActivityScope_Start);
            }
        }

        /// <summary>
        /// Disposes of the object, resetting the ActivityId.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of the object, resetting the ActivityId.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Log Transfer Out
                if( _source != null )
                {
                    _source.TraceTransfer(_transferOutId, Resource.ActivityScope_Transfer, _previousActivityId);
                }

                // Log Stop Message
                if( _source != null )
                {
                    _source.TraceEvent(TraceEventType.Stop, _stopId, Resource.ActivityScope_Stop);
                }
                Trace.CorrelationManager.ActivityId = _previousActivityId;
            }
        }
    }
}
