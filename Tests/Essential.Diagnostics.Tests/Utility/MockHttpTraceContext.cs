using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Essential.Web;

namespace Essential.Diagnostics.Tests.Utility
{
    class MockHttpTraceContext : IHttpTraceContext
    {
        public Uri RequestUrl
        {
            get;
            set;
        }

        public string RequestPath
        {
            get;
            set;
        }

        public string UserHostAddress
        {
            get;
            set;
        }

        public string AppDataPath
        {
            get;
            set;
        }
    }
}
