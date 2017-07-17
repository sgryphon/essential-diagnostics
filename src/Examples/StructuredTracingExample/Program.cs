using Essential.Diagnostics;
using Essential.Diagnostics.Structured;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace StructuredTracing.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            var trace = new TraceSource("StructuredTracing.Example");
            trace.TraceData(TraceEventType.Verbose, 0, 
                new StructuredData(new Dictionary<string, object>
                {
                    { "EmailAddress", "alice@example.org" },
                    { "Delay", 123.45 }
                }, "Email sent"));
            trace.TraceData(TraceEventType.Information, 1001, 
                new StructuredData("City {Place} has coordinates {@Location}", 
                "Brisbane", new Location() { Latitude = -27.5, Longitude = 153.0 }, Guid.NewGuid()));

            trace.TraceStructuredData(TraceEventType.Warning, 3001, new Dictionary<string, object>
                {
                    { "CurrentValue", 49 },
                    { "Threshold", 50 }
                }, "Reached {CurrentValue}");

            trace.TraceStructuredData(TraceEventType.Error, 5001, new ApplicationException(), 
                "There was an error processing {OrderId}", 12345);

            var structuredTrace = new AssemblyStructuredTrace<StandardEventId, Program>();

            structuredTrace.Information(StandardEventId.ConfigurationStart, 
                "Configuration started: {Address}", new IPAddress(new byte[] { 192, 168, 1, 1 }));

            structuredTrace.Critical(StandardEventId.SystemCriticalError, 
                new ApplicationException(), "System error");

            structuredTrace.Verbose("Item is between {Low} and {High}", 22, 24);

            Console.WriteLine("Done");
            Console.ReadKey();
        }
    }

    public class Location
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
