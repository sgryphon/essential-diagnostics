using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Essential.Diagnostics.Abstractions;

namespace AbstractionDependency
{
    public class ApplicationTest1
    {
        public void TestRun()
        {
            var mockTraceSource = new MockTraceSource<Application>();
            var applicationToTest = new Application(mockTraceSource);

            applicationToTest.Run();

            Console.WriteLine("Expected events: 1001, 8001");
            Console.WriteLine("Actual events: {0}, {1}", mockTraceSource.eventIds[0], mockTraceSource.eventIds[1]);
        }

        public class MockTraceSource<T> : ITraceSource<T>
        {
            public IList<int> eventIds = new List<int>();

            public void Flush()
            {
                //throw new NotImplementedException();
            }

            public void TraceEvent(System.Diagnostics.TraceEventType eventType, int id, string message)
            {
                eventIds.Add(id);
            }

            // Other properties and methods not used in this test

            public System.Collections.Specialized.StringDictionary Attributes
            {
                get { throw new NotImplementedException(); }
            }

            public void Close()
            {
                throw new NotImplementedException();
            }

            public System.Diagnostics.TraceListenerCollection Listeners
            {
                get { throw new NotImplementedException(); }
            }

            public string Name
            {
                get { throw new NotImplementedException(); }
            }

            public System.Diagnostics.SourceSwitch Switch
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            public void TraceData(System.Diagnostics.TraceEventType eventType, int id, params object[] data)
            {
                throw new NotImplementedException();
            }

            public void TraceData(System.Diagnostics.TraceEventType eventType, int id, object data)
            {
                throw new NotImplementedException();
            }

            public void TraceEvent(System.Diagnostics.TraceEventType eventType, int id, string format, params object[] args)
            {
                throw new NotImplementedException();
            }

            public void TraceEvent(System.Diagnostics.TraceEventType eventType, int id)
            {
                throw new NotImplementedException();
            }

            public void TraceInformation(string format, params object[] args)
            {
                throw new NotImplementedException();
            }

            public void TraceInformation(string message)
            {
                throw new NotImplementedException();
            }

            public void TraceTransfer(int id, string message, Guid relatedActivityId)
            {
                throw new NotImplementedException();
            }
        }
    }
}
