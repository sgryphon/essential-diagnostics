using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Essential.Net
{
    internal interface IHttpWebResponse : IDisposable
    {
        HttpStatusCode StatusCode { get; }

        Stream GetResponseStream();
    }
}
