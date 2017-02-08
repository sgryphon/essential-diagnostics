using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Essential.Net
{
    class HttpWebResponseWrapper : IHttpWebResponse
    {
        private HttpWebResponse _response;

        public HttpWebResponseWrapper(HttpWebResponse response)
        {
            _response = response;
        }

        public HttpStatusCode StatusCode
        {
            get { return _response.StatusCode; }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_response != null)
                {
                    ((IDisposable)_response).Dispose();
                    _response = null;
                }
            }
        }

        public Stream GetResponseStream()
        {
            return _response.GetResponseStream();
        }

    }
}
