using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace Essential.Diagnostics
{
    static class SeqPayloadFormatter
    {
        static readonly IDictionary<Type, Action<object, TextWriter>> LiteralWriters;

        static SeqPayloadFormatter()
        {
            LiteralWriters = new Dictionary<Type, Action<object, TextWriter>>
            {
                { typeof(bool), (v, w) => WriteBoolean((bool)v, w) },
                { typeof(char), (v, w) => WriteString(((char)v).ToString(CultureInfo.InvariantCulture), w) },
                { typeof(byte), WriteToString },
                { typeof(sbyte), WriteToString },
                { typeof(short), WriteToString },
                { typeof(ushort), WriteToString },
                { typeof(int), WriteToString },
                { typeof(uint), WriteToString },
                { typeof(long), WriteToString },
                { typeof(ulong), WriteToString },
                { typeof(float), WriteToString },
                { typeof(double), WriteToString },
                { typeof(decimal), WriteToString },
                { typeof(string), (v, w) => WriteString((string)v, w) },
                { typeof(DateTime), (v, w) => WriteDateTime((DateTime)v, w) },
                { typeof(DateTimeOffset), (v, w) => WriteOffset((DateTimeOffset)v, w) },
            };
        }

        static readonly IDictionary<TraceEventType, string> LevelMap = new Dictionary<TraceEventType, string>
        {
            { TraceEventType.Verbose, "Verbose" },
            // { "Debug", "Debug" },
            { TraceEventType.Information, "Information" },
            { TraceEventType.Warning, "Warning" },
            { TraceEventType.Error, "Error" },
            { TraceEventType.Critical, "Fatal" },
            // This is somewhat important
            { TraceEventType.Transfer, "Information" },
            // Somewhat more detailed
            { TraceEventType.Start, "Verbose" },
            { TraceEventType.Stop, "Verbose" },
            { TraceEventType.Resume, "Verbose" },
            { TraceEventType.Suspend, "Verbose" },
        };

        public static void ToJson(IEnumerable<SeqPayload> events, TextWriter payload)
        {
            var currentOffset = DateTimeOffset.Now.Offset;

            var delim = "";
            foreach (var loggingEvent in events)
            {
                payload.Write(delim);
                delim = ",";
                ToJson(loggingEvent, currentOffset, payload);
            }
        }

        static void ToJson(SeqPayload loggingEvent, TimeSpan currentOffset, TextWriter payload)
        {
            string level;
            if (!LevelMap.TryGetValue(loggingEvent.EventType, out level))
            {
                level = "Verbose";
            }

            payload.Write("{");

            var delim = "";
            DateTimeOffset offsetTimestamp = loggingEvent.EventTime;
            //if (loggingEvent.TimeStamp.Kind == DateTimeKind.Utc)
            //    offsetTimestamp = new DateTimeOffset(loggingEvent.TimeStamp, TimeSpan.Zero);
            //else
            //    offsetTimestamp = new DateTimeOffset(loggingEvent.TimeStamp, currentOffset);

            WriteJsonProperty("Timestamp", offsetTimestamp, ref delim, payload);
            WriteJsonProperty("Level", level, ref delim, payload);

            WriteJsonProperty("MessageTemplate", loggingEvent.MessageTemplate ?? string.Empty, ref delim, payload);

            //if (loggingEvent.Exception != null)
            //    WriteJsonProperty("Exception", loggingEvent.Exception, ref delim, payload);

            payload.Write(",\"Properties\":{");

            var pdelim = "";

            if (loggingEvent.Source != null)
            {
                WriteJsonProperty("Source", loggingEvent.Source, ref pdelim, payload);
                WriteJsonProperty("EventId", loggingEvent.EventId, ref pdelim, payload);
            }

            WriteJsonProperty("EventType", loggingEvent.EventType, ref pdelim, payload);

            WriteJsonProperty("ActivityId", loggingEvent.ActivityId, ref pdelim, payload);

            if (loggingEvent.RelatedActivityId.HasValue)
            {
                WriteJsonProperty("RelatedActivityId", loggingEvent.RelatedActivityId, ref pdelim, payload);
            }

            if (loggingEvent.Data != null && loggingEvent.Data.Length > 0)
            {
                WriteJsonProperty("Data", loggingEvent.Data, ref pdelim, payload);
            }

            //foreach (var property in properties)
            //{
            //    var stringValue = property.Value.Render(loggingEvent);
            //    if (property.As == "number")
            //    {
            //        decimal numberValue;
            //        if (decimal.TryParse(stringValue, out numberValue))
            //        {
            //            WriteJsonProperty(property.Name, numberValue, ref pdelim, payload);
            //            continue;
            //        }
            //    }

            //    WriteJsonProperty(property.Name, stringValue, ref pdelim, payload);
            //}

            //if (loggingEvent.Parameters != null)
            //{
            //    for (var i = 0; i < loggingEvent.Parameters.Length; ++i)
            //    {
            //        WriteJsonProperty(i.ToString(CultureInfo.InvariantCulture), loggingEvent.Parameters[i], ref pdelim, payload);
            //    }
            //}

            if (loggingEvent.Properties != null)
            {
                //var seenKeys = new HashSet<string>();
                var seenKeys = new List<string>();
                foreach (var property in loggingEvent.Properties)
                {
                    var sanitizedKey = SanitizeKey(property.Key.ToString());
                    if (seenKeys.Contains(sanitizedKey))
                        continue;

                    seenKeys.Add(sanitizedKey);
                    WriteJsonProperty(sanitizedKey, property.Value, ref pdelim, payload);
                }
            }

            payload.Write("}");
            payload.Write("}");
        }

        static string SanitizeKey(string key)
        {
            //return new string(key.Replace(":", "_").Where(c => c == '_' || char.IsLetterOrDigit(c)).ToArray());

            var builder = new StringBuilder();
            foreach (var c in key.ToCharArray())
            {
                if (c == ':')
                {
                    builder.Append('_');
                }
                else if (c == '_' || char.IsLetterOrDigit(c))
                {
                    builder.Append(c);
                }
            }
            return builder.ToString();
        }


        static void WriteJsonProperty(string name, object value, ref string precedingDelimiter, TextWriter output)
        {
            output.Write(precedingDelimiter);
            WritePropertyName(name, output);
            WriteLiteral(value, output);
            precedingDelimiter = ",";
        }

        static void WritePropertyName(string name, TextWriter output)
        {
            output.Write("\"");
            output.Write(name);
            output.Write("\":");
        }

        static void WriteArray(object[] array, TextWriter output)
        {
            output.Write("[");
            for (var index = 0; index < array.Length; index++)
            {
                if (index > 0)
                {
                    output.Write(",");
                }
                var value = array[index];
                WriteLiteral(value, output);
            }
            output.Write("]");
        }

        static void WriteLiteral(object value, TextWriter output)
        {
            if (value == null)
            {
                output.Write("null");
                return;
            }

            if (value is Array)
            {
                WriteArray((object[])value, output);
                return;
            }

            Action<object, TextWriter> writer;
            if (LiteralWriters.TryGetValue(value.GetType(), out writer))
            {
                writer(value, output);
                return;
            }

            WriteString(value.ToString(), output);
        }

        static void WriteToString(object number, TextWriter output)
        {
            output.Write(number.ToString());
        }

        static void WriteBoolean(bool value, TextWriter output)
        {
            output.Write(value ? "true" : "false");
        }

        static void WriteOffset(DateTimeOffset value, TextWriter output)
        {
            output.Write("\"");
            output.Write(value.ToString("o"));
            output.Write("\"");
        }

        static void WriteDateTime(DateTime value, TextWriter output)
        {
            output.Write("\"");
            output.Write(value.ToString("o"));
            output.Write("\"");
        }

        static void WriteString(string value, TextWriter output)
        {
            var content = Escape(value);
            output.Write("\"");
            output.Write(content);
            output.Write("\"");
        }

        static string Escape(string s)
        {
            if (s == null) return null;

            StringBuilder escapedResult = null;
            var cleanSegmentStart = 0;
            for (var i = 0; i < s.Length; ++i)
            {
                var c = s[i];
                if (c < (char)32 || c == '\\' || c == '"')
                {

                    if (escapedResult == null)
                        escapedResult = new StringBuilder();

                    escapedResult.Append(s.Substring(cleanSegmentStart, i - cleanSegmentStart));
                    cleanSegmentStart = i + 1;

                    switch (c)
                    {
                        case '"':
                            {
                                escapedResult.Append("\\\"");
                                break;
                            }
                        case '\\':
                            {
                                escapedResult.Append("\\\\");
                                break;
                            }
                        case '\n':
                            {
                                escapedResult.Append("\\n");
                                break;
                            }
                        case '\r':
                            {
                                escapedResult.Append("\\r");
                                break;
                            }
                        case '\f':
                            {
                                escapedResult.Append("\\f");
                                break;
                            }
                        case '\t':
                            {
                                escapedResult.Append("\\t");
                                break;
                            }
                        default:
                            {
                                escapedResult.Append("\\u");
                                escapedResult.Append(((int)c).ToString("X4"));
                                break;
                            }
                    }
                }
            }

            if (escapedResult != null)
            {
                if (cleanSegmentStart != s.Length)
                    escapedResult.Append(s.Substring(cleanSegmentStart));

                return escapedResult.ToString();
            }

            return s;
        }

        private delegate void Action<T1, T2>(T1 a, T2 b);
    }
}
