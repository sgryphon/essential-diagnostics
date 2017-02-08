using System;
using System.Collections.Generic;
using System.Text;

namespace Essential.Net
{
    internal interface IHttpWebRequestFactory
    {
        IHttpWebRequest Create(string uri);
    }
}
