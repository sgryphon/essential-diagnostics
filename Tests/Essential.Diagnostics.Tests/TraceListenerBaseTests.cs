using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Essential.Diagnostics.Tests.Utility;

namespace Essential.Diagnostics.Tests
{
    [TestClass]
    public class TraceListenerBaseTests
    {
        [TestMethod]
        public void BaseHandlesTraceData()
        {
            object data = Guid.NewGuid();
            GivenTestSource()
                .WhenTraceAction(source => source.TraceData(TraceEventType.Warning, 2, data))
                .ThenVerifyTraceInfo("testSource", TraceEventType.Warning, 2, null, new object[] { data }, null);
        }

        [TestMethod]
        public void BaseHandlesTraceMultipleData()
        {
            object data0 = Guid.NewGuid();
            object data1 = Guid.NewGuid();
            GivenTestSource()
                .WhenTraceAction(source => source.TraceData(TraceEventType.Warning, 2, data0, data1))
                .ThenVerifyTraceInfo("testSource", TraceEventType.Warning, 2, null, new object[] { data0, data1 }, null);
        }


        [TestMethod]
        public void BaseHandlesTraceNullData()
        {
            object data0 = null;
            GivenTestSource()
                .WhenTraceAction(source => source.TraceData(TraceEventType.Warning, 2, data0))
                .ThenVerifyTraceInfo("testSource", TraceEventType.Warning, 2, null, new object[] { data0 }, null);
        }

        [TestMethod]
        public void BaseHandlesTraceNullDataArray()
        {
            object[] data = null;
            GivenTestSource()
                .WhenTraceAction(source => source.TraceData(TraceEventType.Warning, 2, data))
                .ThenVerifyTraceInfo("testSource", TraceEventType.Warning, 2, null, null, null);
        }

        [TestMethod]
        public void BaseHandlesTraceEventWithoutMessage()
        {
            GivenTestSource()
                .WhenTraceAction(source => source.TraceEvent(TraceEventType.Warning, 2))
                .ThenVerifyTraceInfo("testSource", TraceEventType.Warning, 2, "", null, null);
        }

        [TestMethod]
        public void BaseHandlesTraceEvent()
        {
            GivenTestSource()
                .WhenTraceAction(source => source.TraceEvent(TraceEventType.Warning, 2, "xyz"))
                .ThenVerifyTraceInfo("testSource", TraceEventType.Warning, 2, "xyz", null, null);
        }

        [TestMethod]
        public void BaseHandlesTraceEventWithBraces()
        {
            GivenTestSource()
                .WhenTraceAction(source => source.TraceEvent(TraceEventType.Warning, 2, "xyz{0}"))
                .ThenVerifyTraceInfo("testSource", TraceEventType.Warning, 2, "xyz{0}", null, null);
        }

        [TestMethod]
        public void BaseHandlesTraceEventWithNullMessage()
        {
            GivenTestSource()
                .WhenTraceAction(source => source.TraceEvent(TraceEventType.Warning, 2, null))
                .ThenVerifyTraceInfo("testSource", TraceEventType.Warning, 2, null, null, null);
        }

        [TestMethod]
        public void BaseHandlesTraceEventWithParams()
        {
            GivenTestSource()
                .WhenTraceAction(source => source.TraceEvent(TraceEventType.Warning, 2, "{0}-{1}", 3, "B"))
                .ThenVerifyTraceInfo("testSource", TraceEventType.Warning, 2, "3-B", null, null);
        }

        [TestMethod]
        public void BaseHandlesTraceEventWithNullParam()
        {
            object param = null;
            GivenTestSource()
                .WhenTraceAction(source => source.TraceEvent(TraceEventType.Warning, 2, "a{0}b", param))
                .ThenVerifyTraceInfo("testSource", TraceEventType.Warning, 2, "ab", null, null);
        }

        [TestMethod]
        //[ExpectedException(typeof(ArgumentNullException))]
        public void BaseWorkAroundHandlesTraceEventWithNullParamArray()
        {
            // Note: This should really thrown ArgumentNullException (as the args array can't be null),
            // however traceSource.TraceInformation(message) calls TraceEvent(..., format, null) instead
            // of TraceEvent(..., message), so we don't call string.Format if args is null.
            object[] paramArray = null;
            GivenTestSource()
                .WhenTraceAction(source => source.TraceEvent(TraceEventType.Warning, 2, "a{0}b", paramArray))
                .ThenVerifyTraceInfo("testSource", TraceEventType.Warning, 2, "a{0}b", null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BaseThrowExceptionsWhenTraceEventWithParamsAndNullFormat()
        {
            GivenTestSource()
                .WhenTraceAction(source => source.TraceEvent(TraceEventType.Warning, 2, null, 3, "B"));
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void BaseThrowExceptionsWhenTraceEventWithZeroParams()
        {
            object[] paramArray = new object[0];
            GivenTestSource()
                .WhenTraceAction(source => source.TraceEvent(TraceEventType.Warning, 2, "a{0}b", paramArray));
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void BaseThrowExceptionsWhenTraceEventWithNotEnoughParams()
        {
            GivenTestSource()
                .WhenTraceAction(source => source.TraceEvent(TraceEventType.Warning, 2, "a{0}b{1}c", "X"));
        }

        [TestMethod]
        public void BaseHandlesTraceInformation()
        {
            GivenTestSource()
                .WhenTraceAction(source => source.TraceInformation("xyz"))
                .ThenVerifyTraceInfo("testSource", TraceEventType.Information, 0, "xyz", null, null);
        }

        [TestMethod]
        public void BaseHandlesTraceInformationWithBraces()
        {
            GivenTestSource()
                .WhenTraceAction(source => source.TraceInformation("xyz{0}"))
                .ThenVerifyTraceInfo("testSource", TraceEventType.Information, 0, "xyz{0}", null, null);
        }

        [TestMethod]
        public void BaseHandlesTraceInformationWithNullMessage()
        {
            GivenTestSource()
                .WhenTraceAction(source => source.TraceInformation(null))
                .ThenVerifyTraceInfo("testSource", TraceEventType.Information, 0, null, null, null);
        }

        [TestMethod]
        public void BaseHandlesTraceInformationWithParams()
        {
            GivenTestSource()
                .WhenTraceAction(source => source.TraceInformation("{0}-{1}", 3, "B"))
                .ThenVerifyTraceInfo("testSource", TraceEventType.Information, 0, "3-B", null, null);
        }

        [TestMethod]
        public void BaseHandlesTraceInformationWithNullParam()
        {
            object param = null;
            GivenTestSource()
                .WhenTraceAction(source => source.TraceInformation("a{0}b", param))
                .ThenVerifyTraceInfo("testSource", TraceEventType.Information, 0, "ab", null, null);
        }

        [TestMethod]
        public void BaseHandlesTraceTransfer()
        {
            var activityId = Guid.NewGuid();
            var expectedMessage = string.Format("xyz, relatedActivityId={0}", activityId);
            GivenTestSource()
                .WhenTraceAction(source => source.TraceTransfer(2, "xyz", activityId))
                .ThenVerifyTraceInfo("testSource", TraceEventType.Transfer, 2,expectedMessage, null, activityId);
        }

        [TestMethod]
        public void BaseHandlesTraceTransferWithBraces()
        {
            var activityId = Guid.NewGuid();
            var expectedMessage = string.Format("xyz{{0}}, relatedActivityId={0}", activityId);
            GivenTestSource()
                .WhenTraceAction(source => source.TraceTransfer(2, "xyz{0}", activityId))
                .ThenVerifyTraceInfo("testSource", TraceEventType.Transfer, 2, expectedMessage, null, activityId);
        }

        [TestMethod]
        public void BaseHandlesWriteNumber()
        {
            GivenTestSource()
                .WhenTraceAction(source => Trace.Write(1))
                .ThenVerifyTraceInfo(null, TraceEventType.Verbose, 0, null, new object[] { 1 }, null);
        }

        [TestMethod]
        public void BaseHandlesWriteString()
        {
            GivenTrace()
                .WhenTraceAction(source => Trace.Write("ab"))
                .ThenVerifyTraceInfo(null, TraceEventType.Verbose, 0, "ab", null, null);
        }

        [TestMethod]
        public void BaseHandlesWriteNullObject()
        {
            object param = null;
            GivenTestSource()
                .WhenTraceAction(source => Trace.Write(param))
                .ThenVerifyTraceInfo(null, TraceEventType.Verbose, 0, null, null, null);
        }

        [TestMethod]
        public void BaseHandlesWriteNullString()
        {
            string s = null;
            GivenTrace()
                .WhenTraceAction(source => Trace.Write(s))
                .ThenVerifyTraceInfo(null, TraceEventType.Verbose, 0, null, null, null);
        }

        [TestMethod]
        public void BaseHandlesWriteCategoryString()
        {
            GivenTrace()
                .WhenTraceAction(source => Trace.Write("ab", "c1"))
                .ThenVerifyTraceInfo(null, TraceEventType.Verbose, 0, "c1: ab", null, null);
        }

        [TestMethod]
        public void BaseHandlesWriteCategoryNumber()
        {
            GivenTrace()
                .WhenTraceAction(source => Trace.Write(1, "c1"))
                .ThenVerifyTraceInfo(null, TraceEventType.Verbose, 0, "c1", new object[] { 1 }, null);
        }

        [TestMethod]
        public void BaseHandlesWriteCategoryNullString()
        {
            string s = null;
            GivenTrace()
                .WhenTraceAction(source => Trace.Write(s, "c1"))
                .ThenVerifyTraceInfo(null, TraceEventType.Verbose, 0, "c1: ", null, null);
        }

        [TestMethod]
        public void BaseHandlesWriteCategoryEmptyString()
        {
            GivenTrace()
                .WhenTraceAction(source => Trace.Write("", "c1"))
                .ThenVerifyTraceInfo(null, TraceEventType.Verbose, 0, "c1: ", null, null);
        }

        [TestMethod]
        public void BaseHandlesWriteNullCategoryString()
        {
            GivenTrace()
                .WhenTraceAction(source => Trace.Write("ab", null))
                .ThenVerifyTraceInfo(null, TraceEventType.Verbose, 0, "ab", null, null);
        }

        [TestMethod]
        public void BaseHandlesWriteEmptyCategoryString()
        {
            GivenTrace()
                .WhenTraceAction(source => Trace.Write("ab", ""))
                .ThenVerifyTraceInfo(null, TraceEventType.Verbose, 0, ": ab", null, null);
        }

        [TestMethod]
        public void BaseHandlesTraceTraceInformation()
        {
            var sourceName = "PROGRAM";
            GivenTrace()
                .WhenTraceAction(source => Trace.TraceInformation("a{0}b", 1))
                .ThenVerifyTraceInfo(sourceName, TraceEventType.Information, 0, "a1b", null, null);
        }

        private TestSourceContext GivenTestSource()
        {
            return new TestSourceContext("testSource");
        }

        private TestSourceContext GivenTrace()
        {
            return new TestSourceContext(null);
        }

        class TestSourceContext
        {
            TraceSource source;
            TestTraceListener listener;

            public TestSourceContext(string sourceName)
            {
                if (!string.IsNullOrEmpty(sourceName))
                {
                    source = new TraceSource(sourceName);
                    listener = source.Listeners.OfType<TestTraceListener>().First();
                    listener.MethodCallInformation.Clear();
                }
                else
                {
                    listener = Trace.Listeners.OfType<TestTraceListener>().First();
                    listener.MethodCallInformation.Clear();
                }
            }

            public TestSourceContext WhenTraceAction(Action<TraceSource> action)
            {
                action(source);
                return this;
            }

            public void ThenVerifyTraceInfo(string source, TraceEventType eventType, int id, string message, object[] data, Guid? activityId)
            {
                var info = listener.MethodCallInformation[0];
                Assert.AreEqual(source, info.Source);
                Assert.AreEqual(eventType, info.EventType);
                Assert.AreEqual(id, info.Id);
                Assert.AreEqual(message, info.Message);
                Assert.AreEqual(activityId, info.RelatedActivityId);

                CollectionAssert.AreEqual(data, info.Data);
            }

        }
    }
}
