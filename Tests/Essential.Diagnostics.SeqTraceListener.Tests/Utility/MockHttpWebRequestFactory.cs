using Essential.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
            MockHttpWebResponse response;
            if (_responseQueue.Count > 0)
            {
                response = _responseQueue.Dequeue();
            }
            else
            {
                response = new MockHttpWebResponse(HttpStatusCode.InternalServerError, null);
            }
            var request = new MockHttpWebRequest(uri, response);
            _requestsCreated.Add(request);
            return request;
        }
    }
}
