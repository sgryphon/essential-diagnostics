using Essential.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Essential.Diagnostics.Tests.Utility
{
    class MockHttpWebRequestFactory : IHttpWebRequestFactory
    {
        List<MockHttpWebRequest> _requestsCreated = new List<MockHttpWebRequest>();
        Queue<MockHttpWebResponse> _responseQueue = new Queue<MockHttpWebResponse>();

        public List<MockHttpWebRequest> RequestsCreated
        {
            get { return _requestsCreated; }
        }

        public Queue<MockHttpWebResponse> ResponseQueue
        {
            get { return _responseQueue; }
        }

        public IHttpWebRequest Create(string uri)
        {
            var response = _responseQueue.Dequeue();
            var request = new MockHttpWebRequest(uri, response);
            _requestsCreated.Add(request);
            return request;
        }
    }
}
