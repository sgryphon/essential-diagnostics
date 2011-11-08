using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Essential.Web
{
    /// <summary>
    /// Adapter that wraps HttpContext.Current.
    /// </summary>
    public class HttpContextAdapter : IHttpTraceContext
    {
        static string AppData = "~/App_Data";
        HttpContext context;

        /// <summary>
        /// Constructor.
        /// </summary>
        public HttpContextAdapter(HttpContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Gets the physical file path that corresponds to the App_Data directory, if in the context of a web request.
        /// </summary>
        public string AppDataPath
        {
            get
            {
                if (context == null || context.Server == null) return null;
                //AppDomain.CurrentDomain.GetData("DataDirectory");
                return context.Server.MapPath(AppData);
            }
        }

        /// <summary>
        /// Gets the virtual path of the current request, if in the context of a web request. 
        /// </summary>
        public string RequestPath
        {
            get
            {
                if (context == null || context.Request == null) return null;
                return context.Request.Path;
            }
        }

        /// <summary>
        /// Gets information about the URL of the current request, if in the context of a web request. 
        /// </summary>
        public Uri RequestUrl
        {
            get
            {
                if (context == null || context.Request == null) return null;
                return context.Request.Url;
            }
        }

        /// <summary>
        /// Gets the IP host address of the remote client, if in the context of a web request. 
        /// </summary>
        public string UserHostAddress
        {
            get
            {
                if (context == null || context.Request == null) return null;
                return context.Request.UserHostAddress;
            }
        }
    }
}
