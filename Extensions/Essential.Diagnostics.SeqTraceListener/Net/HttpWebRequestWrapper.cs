using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Essential.Net
{
    class HttpWebRequestWrapper : IHttpWebRequest
    {
        private readonly HttpWebRequest _request;

        public HttpWebRequestWrapper(HttpWebRequest request)
        {
            _request = request;
        }

        public string ContentType
        {
            get { return _request.ContentType; }
            set { _request.ContentType = value; }
        }

        public string Method
        {
            get { return _request.Method; }
            set { _request.Method = value; }
        }

        public void AddHeader(string name, string value)
        {
            _request.Headers.Add(name, value);
        }

        public Stream GetRequestStream()
        {
            return _request.GetRequestStream();
        }

        public IHttpWebResponse GetResponse()
        {
            return new HttpWebResponseWrapper((HttpWebResponse)_request.GetResponse());
        }
    }
}
