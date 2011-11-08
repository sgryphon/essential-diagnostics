using System;
using System.Collections.Generic;
using System.Text;

namespace Essential.Web
{
    /// <summary>
    /// Interface representing information available to tracing from HttpContext.Current.
    /// </summary>
    public interface IHttpTraceContext
    {
        /// <summary>
        /// Gets the physical file path that corresponds to the App_Data directory, if in the context of a web request.
        /// </summary>
        string AppDataPath { get; }

        /// <summary>
        /// Gets the virtual path of the current request, if in the context of a web request. 
        /// </summary>
        string RequestPath { get; }

        /// <summary>
        /// Gets information about the URL of the current request, if in the context of a web request. 
        /// </summary>
        Uri RequestUrl { get; }

        /// <summary>
        /// Gets the IP host address of the remote client, if in the context of a web request. 
        /// </summary>
        string UserHostAddress { get; }
    }
}
