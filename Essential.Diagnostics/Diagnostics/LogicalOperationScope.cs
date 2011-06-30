using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

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
        TraceSource _source;
        int _startId;
        int _stopId;

        /// <summary>
        /// Constructor. 
        /// Encompases an unnamed logical operation.
        /// </summary>
        public LogicalOperationScope() : this(null, null, 0, 0)
        {
        }

        /// <summary>
        /// Constructor. 
        /// Encompases a logical operation using the specified object.
        /// </summary>
        public LogicalOperationScope(object operationId) : this(null, operationId, 0, 0)
        {
        }

        /// <summary>
        /// Constructor. 
        /// Encompass a logical operation using the specified object, 
        /// and writing start and stop events to the specified source.
        /// </summary>
        public LogicalOperationScope(TraceSource source, object operationId) : this(source ,operationId, 0, 0)
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
        {
            _source = source;
            _startId = startId;
            _stopId = stopId;

            // Start Logical Operation
            if (operationId == null)
            {
                Trace.CorrelationManager.StartLogicalOperation();
            }
            else
            {
                Trace.CorrelationManager.StartLogicalOperation(operationId);
            }

            // Log Start Message
            if (_source != null)
            {
                _source.TraceEvent(TraceEventType.Start, _startId);
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
                    _source.TraceEvent(TraceEventType.Stop, _stopId);
                }

                // Stop Logical Operation
                Trace.CorrelationManager.StopLogicalOperation();
            }
        }

    }
}
