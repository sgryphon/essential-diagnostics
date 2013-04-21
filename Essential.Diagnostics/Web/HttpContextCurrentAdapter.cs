using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Essential.Web
{
    /// <summary>
    /// Adapter that wraps HttpContext.Current.
    /// </summary>
    public class HttpContextCurrentAdapter : IHttpTraceContext
    {
        static string AppData = "~/App_Data";

        /// <summary>
        /// Gets the physical file path that corresponds to the App_Data directory, if in the context of a web request.
        /// </summary>
        public string AppDataPath
        {
            get
            {
                var context = HttpContext.Current;
                if (context == null) { return null; }

                string path = null;
                if (context.Server != null)
                {
                    //AppDomain.CurrentDomain.GetData("DataDirectory");
                    //return context.Server.MapPath(AppData);
                    //HttpRuntime.AppDomainAppVirtualPath
                    path = context.Server.MapPath(AppData);
                }
                return path;
            }
        }

        /// <summary>
        /// Gets the virtual path of the current request, if in the context of a web request. 
        /// </summary>
        public string RequestPath
        {
            get
            {
                var context = HttpContext.Current;
                if (context == null) return null;
                try
                {
                    if (context.Request == null) return null;
                    return context.Request.Path;
                }
                catch (HttpException)
                {
                    // See Issue #23 on CodePlex (report marnixvv branch)
                    // Under IIS7 Integrated mode if called from Application_Start, then
                    // HttpContext.Request throsw "Request is not available in this context"
                    // (rather than, say, return null).
                    // Inside tracing we don't know where the statement has been
                    // called from.
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets information about the URL of the current request, if in the context of a web request. 
        /// </summary>
        public Uri RequestUrl
        {
            get
            {
                var context = HttpContext.Current;
                if (context == null) return null;
                try
                {
                    if (context.Request == null) return null;
                    return context.Request.Url;
                }
                catch (HttpException)
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the IP host address of the remote client, if in the context of a web request. 
        /// </summary>
        public string UserHostAddress
        {
            get
            {
                var context = HttpContext.Current;
                if (context == null) return null;
                try
                {
                    if (context.Request == null) return null;
                    return context.Request.UserHostAddress;
                }
                catch (HttpException)
                {
                    return null;
                }
            }
        }
    }
}
