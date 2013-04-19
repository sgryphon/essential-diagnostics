using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HelloMvc3.Controllers
{
    public class HomeController : Controller
    {
        TraceSource source = new TraceSource("HelloMvc3");

        public ActionResult Index()
        {
            throw new Exception("Foo");
            return View();
        }

        public ActionResult Log()
        {
            var messageCount = 0;
            source.TraceEvent(TraceEventType.Verbose, 0, "Message (verbose) {0}.", ++messageCount);
            source.TraceEvent(TraceEventType.Information, 1000, "Message (information) {0}.", ++messageCount);
            source.TraceEvent(TraceEventType.Warning, 4000, "Message (warning) {0}.", ++messageCount);
            source.TraceEvent(TraceEventType.Error, 5000, "Message (error) {0}.", ++messageCount);
            source.TraceEvent(TraceEventType.Critical, 9000, "Message (critical) {0}.", ++messageCount);
            return View();
        }

    }
}
