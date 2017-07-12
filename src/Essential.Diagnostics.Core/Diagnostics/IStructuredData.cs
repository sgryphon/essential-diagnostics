using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace Essential.Diagnostics
{
    public interface IStructuredData
    {
        IDictionary<string, object> Properties { get; }
        string MessageTemplate { get; }
    }
}
