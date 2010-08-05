using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Essential.Diagnostics.Tests
{
    [TestClass]
    public class ExpressionFilterTests
    {
        [TestMethod]
        public void ShouldAllowValidTrace()
        {
            var filter = new ExpressionFilter("Id == 1");

            var shouldTrace = filter.ShouldTrace(null, "Source", TraceEventType.Information, 1, "Message", null, null, null);

            Assert.IsTrue(shouldTrace);
        }

        [TestMethod]
        public void ShouldBlockInvalidTrace()
        {
            var filter = new ExpressionFilter("Id == 1");

            var shouldTrace = filter.ShouldTrace(null, "Source", TraceEventType.Information, 2, "Message", null, null, null);

            Assert.IsFalse(shouldTrace);
        }
    }
}
