using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Security.Principal;

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
    /// ApplicationName.
    /// </para>
    /// <para>
    /// An example template that generates the same output as the ConsoleListener is:
    /// "{Source} {EventType}: {Id} : {Message}".
    /// </para>
    /// </remarks>
    public static class TraceFormatter
    {

        // TODO: AppDomainFriendlyName

        static string applicationName;
        static int processId;
        static string processName;

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
        public static string Format(string template, TraceEventCache eventCache, string source, 
            TraceEventType eventType, int id, string message, 
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
                        case "APPLICATIONNAME":
                            value = FormatApplicationName();
                            break;
                        case "PRINCIPALNAME":
                            value = FormatPrincipalName();
                            break;
                        case "WINDOWSIDENTITYNAME":
                            value = FormatWindowsIdentityName();
                            break;
                        default:
                            value = "{" + name + "}";
                            return true;
                    }
                    return true;
                });
            return result;
        }

        private static void EnsureApplicationName()
        {
            if (applicationName == null)
            {
                applicationName = Path.GetFileNameWithoutExtension(Application.ExecutablePath);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        private static void EnsureProcessInfo()
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

        internal static object FormatApplicationName()
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

        internal static object FormatProcessId(TraceEventCache eventCache)
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

        internal static object FormatProcessName()
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

        internal static object FormatUniversalTime(TraceEventCache eventCache)
        {
            object value;
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
