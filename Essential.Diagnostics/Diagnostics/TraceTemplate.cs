using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Collections;
using System.Globalization;

namespace Essential.Diagnostics
{
    public static class TraceTemplate
    {
        static int processId;
        static string processName;

        public static string Format(string template, TraceEventCache eventCache, string source, 
            TraceEventType eventType, int id, string message, 
            Guid? relatedActivityId, object[] data)
        {
            var result = StringTemplate.Format(CultureInfo.CurrentCulture, template,
                delegate(string name, out object value)
                {
                    switch (name.ToUpperInvariant())
                    {
                        case "DATA":
                            StringBuilder builder = new StringBuilder();
                            if (data != null)
                            {
                                for (int i = 0; i < data.Length; i++)
                                {
                                    if (i != 0)
                                    {
                                        builder.Append(", ");
                                    }
                                    if (data[i] != null)
                                    {
                                        builder.Append(data[i]);
                                    }
                                }
                            }
                            value = builder.ToString();
                            break;
                        case "DATA0":
                            if (data != null && data.Length > 0)
                            {
                                value = data[0];
                            }
                            else
                            {
                                value = null;
                            }
                            break;
                        case "EVENTTYPE":
                            value = eventType;
                            break;
                        case "ID":
                            value = id;
                            break;
                        case "MESSAGE":
                            value = message;
                            break;
                        case "ACTIVITYID":
                            value = Trace.CorrelationManager.ActivityId;
                            break;
                        case "RELATEDACTIVITYID":
                            value = relatedActivityId;
                            break;
                        case "SOURCE":
                            value = source;
                            break;

                        case "CALLSTACK":
                            if (eventCache == null)
                            {
                                value = null;
                            }
                            else
                            {
                                value = eventCache.Callstack;
                            }
                            break;
                        case "DATETIME":
                            if (eventCache == null)
                            {
                                value = DateTimeOffset.UtcNow.UtcDateTime;
                            }
                            else
                            {
                                value = (DateTimeOffset)eventCache.DateTime;
                            }
                            break;
                        case "LOGICALOPERATIONSTACK":
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
                            break;
                        case "PROCESSID":
                            if (eventCache == null)
                            {
                                EnsureProcessInfo();
                                value = processId;
                            }
                            else
                            {
                                value = eventCache.ProcessId;
                            }
                            break;
                        case "THREADID":
                            if (eventCache == null)
                            {
                                value = Thread.CurrentThread.ManagedThreadId;
                            }
                            else
                            {
                                value = eventCache.ThreadId;
                            }
                            break;
                        case "TIMESTAMP":
                            if (eventCache == null)
                            {
                                value = null;
                            }
                            else
                            {
                                value = eventCache.Timestamp;
                            }
                            break;

                        case "MACHINENAME":
                            value = Environment.MachineName;
                            break;
                        case "PROCESSNAME":
                            EnsureProcessInfo();
                            value = processName;
                            break;
                        case "THREADNAME":
                            value = Thread.CurrentThread.Name;
                            break;

                        default:
                            value = string.Format("{{{0}}}", name);
                            return true;
                    }
                    return true;
                });
            return result;
        }

        /*
        public TraceEventCache eventCache;
        public string source;
        public TraceEventType eventType;
        public int id;
        public string message;
        static int processId;
        static string processName;
        public Guid? relatedActivityId;
        public object[] data;

        public bool GetArgument(string name, out object value)
        {
            switch (name)
            {
                case "Data":
                    StringBuilder builder = new StringBuilder();
                    if (data != null)
                    {
                        for (int i = 0; i < data.Length; i++)
                        {
                            if (i != 0)
                            {
                                builder.Append(", ");
                            }
                            if (data[i] != null)
                            {
                                builder.Append(data[i]);
                            }
                        }
                    }
                    value = builder.ToString();
                    break;
                case "Data0":
                    if (data != null && data.Length > 0)
                    {
                        value = data[0];
                    }
                    else
                    {
                        value = null;
                    }
                    break;
                case "EventType":
                    value = eventType;
                    break;
                case "Id":
                    value = id;
                    break;
                case "Message":
                    value = message;
                    break;
                case "ActivityId":
                    value = Trace.CorrelationManager.ActivityId;
                    break;
                case "RelatedActivityId":
                    value = relatedActivityId;
                    break;
                case "Source":
                    value = source;
                    break;

                case "Callstack":
                    if (eventCache == null)
                    {
                        value = null;
                    }
                    else
                    {
                        value = eventCache.Callstack;
                    }
                    break;
                case "DateTime":
                    if (eventCache == null)
                    {
                        value = DateTimeOffset.UtcNow.UtcDateTime;
                    }
                    else
                    {
                        value = eventCache.DateTime;
                    }
                    break;
                case "LogicalOperationStack":
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
                    break;
                case "ProcessId":
                    if (eventCache == null)
                    {
                        InitProcessInfo();
                        value = processId;
                    }
                    else
                    {
                        value = eventCache.ProcessId;
                    }
                    break;
                case "ThreadId":
                    if (eventCache == null)
                    {
                        value = Thread.CurrentThread.ManagedThreadId;
                    }
                    else
                    {
                        value = eventCache.ThreadId;
                    }
                    break;
                case "Timestamp":
                    if (eventCache == null)
                    {
                        value = null;
                    }
                    else
                    {
                        value = eventCache.Timestamp;
                    }
                    break;

                case "MachineName":
                    value = Environment.MachineName;
                    break;
                case "ProcessName":
                    InitProcessInfo();
                    value = processName;
                    break;
                case "ThreadName":
                    value = Thread.CurrentThread.Name;
                    break;

                default:
                    value = string.Format("{{{0}}}", name);
                    return true;
            }
            return true;
        }
         */

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

    }
}
