using System;
using System.Diagnostics;
using Essential.Diagnostics.Abstractions;
using System.Xml;
using System.Security;

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

        const string TraceRecordXmlTemplate = @"<TraceRecord Severity='Start' xmlns='http://schemas.microsoft.com/2004/10/E2ETraceEvent/TraceRecord'>
    <TraceIdentifier></TraceIdentifier>
    <Description>{0}</Description>
    <AppDomain>{1}</AppDomain>
    <ExtendedData xmlns='http://schemas.microsoft.com/2006/08/ServiceModel/DictionaryTraceRecord'>
        <ActivityName>{2}</ActivityName>
        <ActivityType>Construct</ActivityType>
    </ExtendedData>
</TraceRecord>";

        string _activityName;
        Guid _previousActivityId;
        ITraceSource _source;
        int _startId;
        string _startMessage;
        int _stopId;
        string _stopMessage;
        int _transferInId;
        string _transferInMessage;
        int _transferOutId;
        string _transferOutMessage;

        /// <summary>
        /// Constructor. Sets the ActivityId for the life of the object without logging any events.
        /// </summary>
        public ActivityScope()
            : this((ITraceSource)null, 0, 0, 0, 0, null, null, null, null, null)
        {
        }

        /// <summary>
        /// Constructor. Sets the ActivityId for the life of the object, logging events but without specific event IDs.
        /// </summary>
        public ActivityScope(TraceSource source)
            : this(new TraceSourceWrapper(source), 0, 0, 0, 0, null, null, null, null, null)
        {
        }

        /// <summary>
        /// Constructor. Sets the ActivityId for the life of the object, logging events with the specified event IDs.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public ActivityScope(TraceSource source, int transferInId, int startId, int transferOutId, int stopId)
            : this(new TraceSourceWrapper(source), transferInId, startId, transferOutId, stopId,
                  null, null, null, null, null)
        {
        }

        /// <summary>
        /// Constructor. Sets the ActivityId for the life of the object, logging events with the specified event IDs and messages.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Custom messages may be set for transfer-in, start, transfer-out and stop. Empty strings will be honored; 
        /// if null values, the default messages will be used instead.
        /// </para>
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public ActivityScope(TraceSource source, int transferInId, int startId, int transferOutId, int stopId,
            string transferInMessage, string startMessage, string transferOutMessage, string stopMessage)
            : this(new TraceSourceWrapper(source), transferInId, startId, transferOutId, stopId,
                  transferInMessage, startMessage, transferOutMessage, stopMessage, null)
        {
        }

        /// <summary>
        /// Constructor. Sets the ActivityId for the life of the object, logging events with the specified event IDs, messages, and activity name (when using XML listeners). 
        /// </summary>
        /// <remarks>
        /// <para>
        /// Note that when activityName is specified, the Start event is logged as a Data event, with specially formatted
        /// XML containing the activity name and start message. This is used by XmlWriterTraceListener and the XML Trace Viewer
        /// application, but may not work in other listeners.
        /// </para>
        /// <para>
        /// Custom messages may be set for transfer-in, start, transfer-out and stop. Empty strings will be honored; 
        /// if null values, the default messages will be used instead.
        /// </para>
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public ActivityScope(TraceSource source, int transferInId, int startId, int transferOutId, int stopId,
            string transferInMessage, string startMessage, string transferOutMessage, string stopMessage, string activityName)
            : this(new TraceSourceWrapper(source), transferInId, startId, transferOutId, stopId, 
                  transferInMessage, startMessage, transferOutMessage, stopMessage, activityName)
        {
        }

        /// <summary>
        /// Constructor. Sets the ActivityId for the life of the object, logging events but without specific event IDs.
        /// </summary>
        public ActivityScope(ITraceSource source)
            : this(source, 0, 0, 0, 0, null, null, null, null, null)
        {
        }

        /// <summary>
        /// Constructor. Sets the ActivityId for the life of the object, logging events with the specified event IDs.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public ActivityScope(ITraceSource source, int transferInId, int startId, int transferOutId, int stopId)
            : this(source, transferInId, startId, transferOutId, stopId,
                  null, null, null, null, null)
        {
        }

        /// <summary>
        /// Constructor. Sets the ActivityId for the life of the object, logging events with the specified event IDs and messages.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Custom messages may be set for transfer-in, start, transfer-out and stop. Empty strings will be honored; 
        /// if null values, the default messages will be used instead.
        /// </para>
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public ActivityScope(ITraceSource source, int transferInId, int startId, int transferOutId, int stopId,
            string transferInMessage, string startMessage, string transferOutMessage, string stopMessage)
            : this(source, transferInId, startId, transferOutId, stopId,
                  transferInMessage, startMessage, transferOutMessage, stopMessage, null)
        {
        }

        /// <summary>
        /// Constructor. Sets the ActivityId for the life of the object, logging events with the specified event IDs, messages, and activity name (when using XML listeners). 
        /// </summary>
        /// <remarks>
        /// <para>
        /// Note that when activityName is specified, the Start event is logged as a Data event, with specially formatted
        /// XML containing the activity name and start message. This is used by XmlWriterTraceListener and the XML Trace Viewer
        /// application, but may not work in other listeners.
        /// </para>
        /// <para>
        /// Custom messages may be set for transfer-in, start, transfer-out and stop. Empty strings will be honored; 
        /// if null values, the default messages will be used instead.
        /// </para>
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public ActivityScope(ITraceSource source, int transferInId, int startId, int transferOutId, int stopId,
            string transferInMessage, string startMessage, string transferOutMessage, string stopMessage, string activityName)
        {
            _source = source;
            _startId = startId;
            _stopId = stopId;
            _transferInId = transferInId;
            _transferOutId = transferOutId;

            _previousActivityId = Trace.CorrelationManager.ActivityId;

            _transferInMessage = transferInMessage ?? Resource_Fluent.ActivityScope_Transfer;
            _startMessage = startMessage ?? Resource_Fluent.ActivityScope_Start;
            _transferOutMessage = transferOutMessage ?? Resource_Fluent.ActivityScope_Transfer;
            _stopMessage = stopMessage ?? Resource_Fluent.ActivityScope_Stop;

            _activityName = activityName;

            StartScope();
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
                StopScope();
            }
        }

        private void StartScope()
        {
            // Log Transfer In (on original activity)
            Guid newActivity = Guid.NewGuid();
            if (_source != null)
            {
                _source.TraceTransfer(_transferInId, _transferInMessage, newActivity);
            }

            // Change to scope ActivityId
            Trace.CorrelationManager.ActivityId = newActivity;

            // Log Start Message (first message on scope activity)
            if (_source != null)
            {
                if (_activityName == null)
                {
                    _source.TraceEvent(TraceEventType.Start, _startId, _startMessage);
                }
                else
                {
                    //var xml = string.Format(TraceRecordXmlTemplate,
                    //    SecurityElement.Escape(_startMessage),
                    //    SecurityElement.Escape(AppDomain.CurrentDomain.FriendlyName),
                    //    SecurityElement.Escape(_activityName)
                    //);
                    var xml = TraceRecordXmlTemplate;
                    var doc = new XmlDocument();
                    doc.LoadXml(xml);
                    doc.ChildNodes[0].ChildNodes[1].InnerText = _startMessage;
                    doc.ChildNodes[0].ChildNodes[2].InnerText = AppDomain.CurrentDomain.FriendlyName;
                    doc.ChildNodes[0].ChildNodes[3].ChildNodes[0].InnerText = _activityName;
                    _source.TraceData(TraceEventType.Start, _startId, doc.CreateNavigator());
                }
            }
        }

        private void StopScope()
        {
            // Log Transfer Out (on scope activity, back to original)
            if (_source != null)
            {
                _source.TraceTransfer(_transferOutId, _transferOutMessage, _previousActivityId);
            }

            // Log Stop Message (this is the last message on the scope activity)
            if (_source != null)
            {
                _source.TraceEvent(TraceEventType.Stop, _stopId, _stopMessage);
            }

            // Change back to original ActivityId
            Trace.CorrelationManager.ActivityId = _previousActivityId;
        }

    }
}
