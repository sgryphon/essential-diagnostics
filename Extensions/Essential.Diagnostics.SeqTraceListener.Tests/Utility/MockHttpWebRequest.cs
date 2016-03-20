using Essential.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Essential.Diagnostics.Tests.Utility
{
    class MockHttpWebRequest : IHttpWebRequest
    {
        public MockHttpWebRequest(string uri, MockHttpWebResponse response)
        {
            Uri = uri;
            Response = response;
            RequestStream = new MemoryStream();
            Headers = new Dictionary<string, string>();
        }

        public string ContentType
        {
            get;
            set;
        }

        public IDictionary<string, string> Headers
        {
            get;
            set;
        }

        public string Method
        {
            get;
            set;
        }

        public MockHttpWebResponse Response
        {
            get;
            set;
        }

        public string RequestBody
        {
            get
            {
                var bytes = RequestStream.ToArray();
                var body = Encoding.UTF8.GetString(bytes);
                return body;
            }
        }

        public MemoryStream RequestStream
        {
            get;
            set;
        }

        public string Uri
        {
            get;
            set;
        }

        public void AddHeader(string name, string value)
        {
            Headers.Add(name, value);
        }

        public Stream GetRequestStream()
        {
            return RequestStream;
        }

        public IHttpWebResponse GetResponse()
        {
            return Response;
        }
    }
}
