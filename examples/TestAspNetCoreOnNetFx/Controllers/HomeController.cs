using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace TestAspNetCoreOnNetFx.Controllers
{
    public class HomeController : Controller
    {
        TraceSource traceSource = new TraceSource("TraceSource.TestAspNetCoreOnNetFx");

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            traceSource.TraceEvent(TraceEventType.Information, 1001, "TraceSource from About controller: {0}", Guid.NewGuid());

            var loggerFactory = new LoggerFactory();
            loggerFactory.AddTraceSource(new SourceSwitch("sourceSwitch") { Level = SourceLevels.All });
            var logger = loggerFactory.CreateLogger("Logger.TestAspNetCoreOnNetFx");
            logger.LogInformation(1002, "Logger from About controller: {0}", Guid.NewGuid());

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
