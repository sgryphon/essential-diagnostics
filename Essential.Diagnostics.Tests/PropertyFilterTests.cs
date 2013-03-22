using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Essential.Diagnostics.Tests
{
   // [TestClass]no need to test obsolete members since we are not going to modify them.
    //public class PropertyFilterTests
    //{
    //    [TestMethod]
    //    public void ShouldAllowValidTrace()
    //    {
    //        var filter = new PropertyFilter("id == 1");

    //        var shouldTrace = filter.ShouldTrace(null, "Source", TraceEventType.Information, 1, "Message", null, null, null);

    //        Assert.IsTrue(shouldTrace);
    //    }

    //    [TestMethod]
    //    public void ShouldBlockInvalidTrace()
    //    {
    //        var filter = new PropertyFilter("id == 1");

    //        var shouldTrace = filter.ShouldTrace(null, "Source", TraceEventType.Information, 2, "Message", null, null, null);

    //        Assert.IsFalse(shouldTrace);
    //    }


    //    [TestMethod]
    //    public void FilterOnSingleDataItemShouldWork()
    //    {
    //        var filter = new PropertyFilter("data == 'A'");

    //        var shouldTrace1 = filter.ShouldTrace(null, "Source", TraceEventType.Information, 1, "Message", null, "A", null);
    //        var shouldTrace2 = filter.ShouldTrace(null, "Source", TraceEventType.Information, 1, "Message", null, "B", null);

    //        Assert.IsTrue(shouldTrace1);
    //        Assert.IsFalse(shouldTrace2);
    //    }
    //}
}
