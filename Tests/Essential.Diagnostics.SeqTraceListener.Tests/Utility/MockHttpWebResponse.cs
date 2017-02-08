using Essential.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;

namespace Essential.Diagnostics.Tests.Utility
{
    class MockHttpWebResponse : IHttpWebResponse
    {        
        public MockHttpWebResponse(HttpStatusCode statusCode, string responseBody)
        {
            StatusCode = statusCode;
            var bodyBytes = new byte[0];
            if (!String.IsNullOrEmpty(responseBody))
            {
                bodyBytes = Encoding.UTF8.GetBytes(responseBody);
            }
            ResponseStream = new MemoryStream(bodyBytes);
        }

        public MemoryStream ResponseStream
        {
            get; set;
        }

        public HttpStatusCode StatusCode
        {
            get; set;
        }

        public void Dispose()
        {
            // Do nothing
        }

        public Stream GetResponseStream()
        {
            return ResponseStream;
        }
    }
}
