using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Principal;

namespace Essential.Diagnostics.Tests
{
    [TestClass]
    public class ExpressionFilterTests
    {
        [TestMethod]
        public void ShouldAllowValidTraceExpression()
        {
            var filter = new ExpressionFilter("Id < 2");

            var shouldTrace = filter.ShouldTrace(null, "Source", TraceEventType.Information, 1, "Message", null, null, null);

            Assert.IsTrue(shouldTrace);
        }

        [TestMethod]
        public void ShouldBlockInvalidTraceExpression()
        {
            var filter = new ExpressionFilter("Id < 2");

            var shouldTrace = filter.ShouldTrace(null, "Source", TraceEventType.Information, 3, "Message", null, null, null);

            Assert.IsFalse(shouldTrace);
        }

        [TestMethod]
        public void ShouldAllowValidTraceEnvironment()
        {
            var filter = new ExpressionFilter("System.Threading.Thread.CurrentPrincipal.Identity.Name == \"A\" ");
            bool shouldTrace;

            var originalPrincipal = System.Threading.Thread.CurrentPrincipal;
            try
            {
                System.Threading.Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("A"), new string[0]);
                shouldTrace = filter.ShouldTrace(null, "Source", TraceEventType.Information, 1, "Message", null, null, null);
            }
            finally
            {
                System.Threading.Thread.CurrentPrincipal = originalPrincipal;
            }

            Assert.IsTrue(shouldTrace);
        }

        [TestMethod]
        public void ShoulBlockInvalidTraceEnvironment()
        {
            var filter = new ExpressionFilter("System.Threading.Thread.CurrentPrincipal.Identity.Name == \"A\" ");
            bool shouldTrace;

            var originalPrincipal = System.Threading.Thread.CurrentPrincipal;
            try
            {
                System.Threading.Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("B"), new string[0]);
                shouldTrace = filter.ShouldTrace(null, "Source", TraceEventType.Information, 1, "Message", null, null, null);
            }
            finally
            {
                System.Threading.Thread.CurrentPrincipal = originalPrincipal;
            }

            Assert.IsFalse(shouldTrace);
        }

    }
}
