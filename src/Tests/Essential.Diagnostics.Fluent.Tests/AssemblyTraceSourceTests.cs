using Microsoft.VisualStudio.TestTools.UnitTesting;
using Essential.Diagnostics.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Essential.Diagnostics.Tests.Utility;
using System.Diagnostics;

namespace Essential.Diagnostics.Abstractions.Tests
{
    [TestClass()]
    public class AssemblyTraceSourceTests
    {
        [TestMethod()]
        public void AssemblyTraceSourceTest()
        {
            var source = new AssemblyTraceSource<AssemblyTraceSourceTests>();
            var listener = source.Listeners.OfType<TestTraceListener>().First();
            listener.MethodCallInformation.Clear();

            source.TraceEvent(TraceEventType.Information, 1, "a1");

            Assert.AreEqual("Essential.Diagnostics.Fluent.Tests", source.Name);
            var events = listener.MethodCallInformation;
            Assert.AreEqual(1, events[0].Id);
            Assert.AreEqual("a1", events[0].Message);
            Assert.AreEqual(TraceEventType.Information, events[0].EventType);
            Assert.AreEqual("Essential.Diagnostics.Fluent.Tests", events[0].Source);
        }

        //[TestMethod()]
        //public void AssemblyTraceSourceTest1()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void CloseTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void FlushTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void TraceDataTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void TraceDataTest1()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void TraceEventTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void TraceEventTest1()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void TraceEventTest2()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void TraceInformationTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void TraceInformationTest1()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void TraceTransferTest()
        //{
        //    Assert.Fail();
        //}
    }
}