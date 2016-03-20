using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Essential.Net
{
    class WebRequestAdapter : IHttpWebRequestFactory
    {
        public IHttpWebRequest Create(string uri)
        {
            return new HttpWebRequestWrapper((HttpWebRequest)WebRequest.Create(uri));
        }
    }
}
