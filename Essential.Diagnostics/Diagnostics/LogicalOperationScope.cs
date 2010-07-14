using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Essential.Diagnostics
{
    public class LogicalOperationScope : IDisposable
    {
        TraceSource _source;
        int _startId;
        int _stopId;

        public LogicalOperationScope() : this(null, null, 0, 0)
        {
        }

        public LogicalOperationScope(object operationId) : this(null, operationId, 0, 0)
        {
        }

        public LogicalOperationScope(TraceSource source, object operationId) : this(source ,operationId, 0, 0)
        {
        }

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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

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
