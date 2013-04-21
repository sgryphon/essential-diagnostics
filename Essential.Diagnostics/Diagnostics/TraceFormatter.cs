using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Security.Principal;
using System.Reflection;
using System.Runtime.InteropServices;
using Essential.Web;
using System.Web;
using System.Text.RegularExpressions;

namespace Essential.Diagnostics
{
    /// <summary>
    /// Formats trace output using a template.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Uses the StringTemplate.Format function to format trace output using a supplied template
    /// and trace information. The formatted event can then be written to the console, a
    /// file, or other text-based output.
    /// </para>
    /// <para>
    /// The following parameters are available in the template string:
    /// Data, Data0, EventType, Id, Message, ActivityId, RelatedActivityId, Source, 
    /// Callstack, DateTime (or UtcDateTime), LocalDateTime, LogicalOperationStack, 
    /// ProcessId, ThreadId, Timestamp, MachineName, ProcessName, ThreadName,
    /// ApplicationName, MessagePrefix, AppDomain.
    /// </para>
    /// <para>
    /// An example template that generates the same output as the ConsoleListener is:
    /// "{Source} {EventType}: {Id} : {Message}".
    /// </para>
    /// </remarks>
    public class TraceFormatter
    {
        const int MaxPrefixLength = 40;
        const string PrefixContinuation = "...";

        static readonly Regex controlCharRegex = new Regex(@"\p{C}", RegexOptions.Compiled);

        string applicationName;
        private IHttpTraceContext httpTraceContext = new HttpContextCurrentAdapter();
        int processId;
        string processName;

        /// <summary>
        /// Gets or sets the context for ASP.NET web trace information.
        /// </summary>
        public IHttpTraceContext HttpTraceContext
        {
            get { return httpTraceContext; }
            set { httpTraceContext = value; }
        }

        /// <summary>
        /// Formats a trace event, inserted the provided values into the provided template.
        /// </summary>
        /// <returns>A string containing the values formatted using the provided template.</returns>
        /// <remarks>
        /// <para>
        /// Obsolete. Should use the overload that includes listener instead.
        /// </para>
        /// </remarks>
        [Obsolete("Should use the overload that includes listener instead.")]
        public string Format(string template, TraceEventCache eventCache, string source,
            TraceEventType eventType, int id, string message,
            Guid? relatedActivityId, object[] data)
        {
            return Format(template, null, eventCache, source, eventType, id, message, relatedActivityId, data);
        }

        /// <summary>
        /// Formats a trace event, inserted the provided values into the provided template.
        /// </summary>
        /// <returns>A string containing the values formatted using the provided template.</returns>
        /// <remarks>
        /// <para>
        /// Uses the StringTemplate.Format function to format trace output using a supplied template
        /// and trace information. The formatted event can then be written to the console, a
        /// file, or other text-based output.
        /// </para>
        /// <para>
        /// The following parameters are available in the template string:
        /// Data, Data0, EventType, Id, Message, ActivityId, RelatedActivityId, Source, 
        /// Callstack, DateTime (or UtcDateTime), LocalDateTime, LogicalOperationStack, 
        /// ProcessId, ThreadId, Timestamp, MachineName, ProcessName, ThreadName,
        /// ApplicationName, PrincipalName, WindowsIdentityName.
        /// </para>
        /// <para>
        /// An example template that generates the same output as the ConsoleListener is:
        /// "{Source} {EventType}: {Id} : {Message}".
        /// </para>
        /// </remarks>
        public string Format(string template, TraceListener listener, TraceEventCache eventCache, 
            string source, TraceEventType eventType, int id, string message, 
            Guid? relatedActivityId, object[] data)
        {
            var result = StringTemplate.Format(CultureInfo.CurrentCulture, template,
                delegate(string name, out object value)
                {
                    switch (name.ToUpperInvariant())
                    {
                        case "EVENTTYPE":
                            value = eventType;
                            break;
                        case "ID":
                            value = id;
                            break;
                        case "MESSAGE":
                            value = message;
                            break;
                        case "MESSAGEPREFIX":
                            value = FormatPrefix(message);
                            break;
                        case "SOURCE":
                            value = source;
                            break;
                        case "DATETIME":
                        case "UTCDATETIME":
                            value = FormatUniversalTime(eventCache);
                            break;
                        case "LOCALDATETIME":
                            value = FormatLocalTime(eventCache);
                            break;
                        case "THREADID":
                            value = FormatThreadId(eventCache);
                            break;
                        case "THREAD":
                            value = Thread.CurrentThread.Name ?? FormatThreadId(eventCache);
                            break;
                        case "THREADNAME":
                            value = Thread.CurrentThread.Name;
                            break;
                        case "ACTIVITYID":
                            value = Trace.CorrelationManager.ActivityId;
                            break;
                        case "RELATEDACTIVITYID":
                            value = relatedActivityId;
                            break;
                        case "DATA":
                            value = FormatData(data);
                            break;
                        case "DATA0":
                            value = FormatData(data, 0);
                            break;
                        case "CALLSTACK":
                            value = FormatCallstack(eventCache);
                            break;
                        case "LOGICALOPERATIONSTACK":
                            value = FormatLogicalOperationStack(eventCache);
                            break;
                        case "PROCESSID":
                            value = FormatProcessId(eventCache);
                            break;
                        case "TIMESTAMP":
                            value = FormatTimeStamp(eventCache);
                            break;
                        case "MACHINENAME":
                            value = Environment.MachineName;
                            break;
                        case "PROCESSNAME":
                            value = FormatProcessName();
                            break;
                        case "USER":
                            value = Environment.UserDomainName + "\\" + Environment.UserName;
                            break;
                        case "PROCESS":
                            value = Environment.CommandLine;
                            break;
                        case "APPLICATIONNAME":
                            value = FormatApplicationName();
                            break;
                        case "APPDOMAIN":
                            value = AppDomain.CurrentDomain.FriendlyName;
                            break;
                        case "PRINCIPALNAME":
                            value = FormatPrincipalName();
                            break;
                        case "WINDOWSIDENTITYNAME":
                            value = FormatWindowsIdentityName();
                            break;
                        case "REQUESTURL":
                            value = HttpTraceContext.RequestUrl;
                            break;
                        case "REQUESTPATH":
                            value = HttpTraceContext.RequestPath;
                            break;
                        case "USERHOSTADDRESS":
                            value = HttpTraceContext.UserHostAddress;
                            break;
                        case "APPDATA":
                            value = HttpTraceContext.AppDataPath;
                            break;
                        case "LISTENER":
                            value = (listener == null) ? "" : listener.Name;
                            break;
                        default:
                            value = "{" + name + "}";
                            return true;
                    }
                    return true;
                });
            return result;
        }

        private void EnsureApplicationName()
        {
            if (applicationName == null)
            {
                //applicationName = Path.GetFileNameWithoutExtension(Application.ExecutablePath);
                var entryAssembly = Assembly.GetEntryAssembly();
                if (entryAssembly == null)
                {
                    var moduleFileName = new StringBuilder(260);
                    int size = NativeMethods.GetModuleFileName(NativeMethods.NullHandleRef, moduleFileName, moduleFileName.Capacity);
                    if (size > 0)
                    {
                        applicationName = Path.GetFileNameWithoutExtension(moduleFileName.ToString());
                        return;
                    }
                    //I don't want to raise any error here since I have a graceful result at the end.
                }

                applicationName = Path.GetFileNameWithoutExtension(entryAssembly.EscapedCodeBase);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        private void EnsureProcessInfo()
        {
            if (processName == null)
            {
                using (Process process = Process.GetCurrentProcess())
                {
                    processId = process.Id;
                    processName = process.ProcessName;
                }
            }
        }

        internal object FormatApplicationName()
        {
            object value;
            EnsureApplicationName();
            value = applicationName;
            return value;
        }

        private static object FormatCallstack(TraceEventCache eventCache)
        {
            object value;
            if (eventCache == null)
            {
                value = null;
            }
            else
            {
                value = eventCache.Callstack;
            }
            return value;
        }

        private static object FormatData(object[] data)
        {
            object value;
            StringBuilder builder = new StringBuilder();
            if (data != null)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    if (i != 0)
                    {
                        builder.Append(",");
                    }
                    if (data[i] != null)
                    {
                        builder.Append(data[i]);
                    }
                }
            }
            value = builder.ToString();
            return value;
        }

        private static object FormatData(object[] data, int index)
        {
            object value;
            if (data != null && data.Length > index)
            {
                value = data[index];
            }
            else
            {
                value = null;
            }
            return value;
        }

        private static object FormatLogicalOperationStack(TraceEventCache eventCache)
        {
            object value;
            Stack stack;
            if (eventCache == null)
            {
                stack = Trace.CorrelationManager.LogicalOperationStack;
            }
            else
            {
                stack = eventCache.LogicalOperationStack;
            }
            if (stack != null && stack.Count > 0)
            {
                StringBuilder stackBuilder = new StringBuilder();
                foreach (object o in stack)
                {
                    if (stackBuilder.Length > 0) stackBuilder.Append(", ");
                    stackBuilder.Append(o);
                }
                value = stackBuilder.ToString();
            }
            else
            {
                value = null;
            }
            return value;
        }

        private static string FormatPrefix(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                // Prefix is the part of the message before the first <;,.:>
                string[] split = message.Split(new char[] { '.', '!', '?', ':', ';', ',', '\r', '\n' }, 2, StringSplitOptions.None);
                string prefix;
                if (split[0].Length <= MaxPrefixLength)
                {
                    prefix = split[0];
                }
                else
                {
                    prefix = split[0].Substring(0, MaxPrefixLength - PrefixContinuation.Length) + PrefixContinuation;
                }
                if (controlCharRegex.IsMatch(prefix))
                {
                    prefix = controlCharRegex.Replace(prefix, "");
                }
                return prefix;
            }
            else
            {
                return message;
            }
        }

        internal static object FormatPrincipalName()
        {
            var principal = Thread.CurrentPrincipal;
            object value = "";
            if (principal != null && principal.Identity != null)
            {
                value = principal.Identity.Name;
            }
            return value;
        }

        internal static object FormatWindowsIdentityName()
        {
            var identity = WindowsIdentity.GetCurrent();
            object value = "";
            if (identity != null)
            {
                value = identity.Name;
            }
            return value;
        }

        internal object FormatProcessId(TraceEventCache eventCache)
        {
            object value;
            if (eventCache == null)
            {
                EnsureProcessInfo();
                value = processId;
            }
            else
            {
                value = eventCache.ProcessId;
            }
            return value;
        }

        internal object FormatProcessName()
        {
            object value;
            EnsureProcessInfo();
            value = processName;
            return value;
        }

        internal static object FormatThreadId(TraceEventCache eventCache)
        {
            object value;
            if (eventCache == null)
            {
                value = Thread.CurrentThread.ManagedThreadId;
            }
            else
            {
                value = eventCache.ThreadId;
            }
            return value;
        }

        private static object FormatTimeStamp(TraceEventCache eventCache)
        {
            object value;
            if (eventCache == null)
            {
                value = null;
            }
            else
            {
                value = eventCache.Timestamp;
            }
            return value;
        }

        internal static DateTimeOffset FormatUniversalTime(TraceEventCache eventCache)
        {
            DateTimeOffset value;
            if (eventCache == null)
            {
                value = DateTimeOffset.UtcNow;
            }
            else
            {
                // Cast to DateTimeOffset first, to get consistent behaviour of ToUniversal/ToLocal
                // (i.e. Unspecified Kind is treated as Local)
                value = ((DateTimeOffset)eventCache.DateTime).ToUniversalTime();
            }
            return value;
        }

        internal static object FormatLocalTime(TraceEventCache eventCache)
        {
            object value;
            if (eventCache == null)
            {
                value = DateTimeOffset.Now;
            }
            else
            {
                // Cast to DateTimeOffset first, to get consistent behaviour of ToUniversal/ToLocal
                // (i.e. Unspecified Kind is treated as Local)
                value = ((DateTimeOffset)eventCache.DateTime).ToLocalTime();
            }
            return value;
        }

    }
}
