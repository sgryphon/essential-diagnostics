using System;
using System.Diagnostics;
using Essential.Diagnostics.Abstractions;

namespace Essential.Diagnostics
{
    /// <summary>
    /// Encompases a logical operation using the diagnostics correlation manager,
    /// starting the operation when the object is created and ending the
    /// operation when the object is disposed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The scope can optionally log start and stop trace events for
    /// the operation.
    /// </para>
    /// </remarks>
    public class LogicalOperationScope : IDisposable
    {
        object _operationId;
        ITraceSource _source;
        int _startId;
        string _startMessage;
        int _stopId;
        string _stopMessage;

        /// <summary>
        /// Constructor. 
        /// Encompases an unnamed logical operation.
        /// </summary>
        public LogicalOperationScope()
            : this((ITraceSource)null, null, 0, 0, null, null)
        {
        }

        /// <summary>
        /// Constructor. 
        /// Encompases a logical operation using the specified object.
        /// </summary>
        public LogicalOperationScope(object operationId) 
            : this((ITraceSource)null, operationId, 0, 0, null, null)
        {
        }

        /// <summary>
        /// Constructor. 
        /// Encompass a logical operation using the specified object, 
        /// and writing start and stop events to the specified source.
        /// </summary>
        public LogicalOperationScope(TraceSource source, object operationId) 
            : this(new TraceSourceWrapper(source), operationId, 0, 0, null, null)
        {
        }

        /// <summary>
        /// Constructor. 
        /// Encompases a logical operation using the specified object,
        /// and writing start and stop events to the specified source,
        /// with the specified event IDs.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public LogicalOperationScope(TraceSource source, object operationId, int startId, int stopId)
            : this(new TraceSourceWrapper(source), operationId, startId, stopId, null, null)
        {
        }

        /// <summary>
        /// Constructor. 
        /// Encompases a logical operation using the specified object,
        /// and writing start and stop events to the specified source,
        /// with the specified event IDs and messages.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public LogicalOperationScope(TraceSource source, object operationId, int startId, int stopId, string startMessage, string stopMessage)
            : this(new TraceSourceWrapper(source), operationId, startId, stopId, startMessage, stopMessage)
        {
        }

        /// <summary>
        /// Constructor. 
        /// Encompass a logical operation using the specified object, 
        /// and writing start and stop events to the specified source.
        /// </summary>
        public LogicalOperationScope(ITraceSource source, object operationId) 
            : this(source, operationId, 0, 0, null, null)
        {
        }

        /// <summary>
        /// Constructor. 
        /// Encompases a logical operation using the specified object,
        /// and writing start and stop events to the specified source,
        /// with the specified event IDs.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public LogicalOperationScope(ITraceSource source, object operationId, int startId, int stopId)
            : this(source, operationId, startId, stopId, null, null)
        {
        }

        /// <summary>
        /// Constructor. 
        /// Encompases a logical operation using the specified object,
        /// and writing start and stop events to the specified source,
        /// with the specified event IDs and messages.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public LogicalOperationScope(ITraceSource source, object operationId, int startId, int stopId, string startMessage, string stopMessage)
        {
            _source = source;
            _startId = startId;
            _stopId = stopId;
            _operationId = operationId;

            _startMessage = startMessage ?? Resource_Fluent.LogicalOperationScope_Start;
            _stopMessage = stopMessage ?? Resource_Fluent.LogicalOperationScope_Stop;

            // Start Logical Operation
            if (_operationId == null)
            {
                Trace.CorrelationManager.StartLogicalOperation();
            }
            else
            {
                Trace.CorrelationManager.StartLogicalOperation(_operationId);
            }

            // Log Start Message
            if (_source != null)
            {
                _source.TraceEvent(TraceEventType.Start, _startId, _startMessage);
            }
        }

        /// <summary>
        /// Dispose.
        /// Ends the logical operation.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose.
        /// Ends the logical operation.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Log Stop Message
                if (_source != null)
                {
                    _source.TraceEvent(TraceEventType.Stop, _stopId, _stopMessage);
                }

                // Stop Logical Operation
                Trace.CorrelationManager.StopLogicalOperation();
            }
        }

    }
}
