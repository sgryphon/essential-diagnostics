using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Essential.Net
{
    internal interface IHttpWebRequest
    {
        string ContentType { get; set; }

        string Method { get; set; }

        void AddHeader(string name, string value);

        Stream GetRequestStream();

        IHttpWebResponse GetResponse();
    }
}
