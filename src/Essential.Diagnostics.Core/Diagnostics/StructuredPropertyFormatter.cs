using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace Essential
{
    internal class StructuredPropertyFormatter
    {
        static readonly IDictionary<Type, Action<object, TextWriter>> LiteralWriters;

        static StructuredPropertyFormatter()
        {
            LiteralWriters = new Dictionary<Type, Action<object, TextWriter>>
            {
                { typeof(bool), (v, w) => WriteBoolean((bool)v, w) },
                { typeof(char), (v, w) => WriteString(((char)v).ToString(CultureInfo.InvariantCulture), w) },
                { typeof(byte), (v, w) => WriteByte((byte)v, w) },
                { typeof(byte[]), (v, w) => WriteByteArray((byte[])v, w) },
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
                { typeof(Guid), WriteToString },
                { typeof(TimeSpan), WriteToString },
                { typeof(string), (v, w) => WriteString((string)v, w) },
                { typeof(DateTime), (v, w) => WriteDateTime((DateTime)v, w) },
                { typeof(DateTimeOffset), (v, w) => WriteDateTimeOffset((DateTimeOffset)v, w) },
            };
        }

        public static string DestructureObject(object obj)
        {
            var writer = new StringWriter();
            DestructurePropertyValue(obj, writer, 0, 0);
            return writer.ToString();
        }

        public static void FormatProperties(IEnumerable<KeyValuePair<string, object>> properties, IList<string> excludeKeys, TextWriter output, ref string delimiter)
        {
            foreach (var kvp in properties)
            {
                if (!excludeKeys.Contains(kvp.Key))
                {
                    WriteProperty(kvp.Key, kvp.Value, output, 0, 0, ref delimiter);
                }
            }
        }

        static void DestructurePropertyValue(object obj, TextWriter output, int arrayCount, int destructureCount)
        {
            if (obj == null)
            {
                output.Write("null");
                return;
            }

            var type = obj.GetType();
            var publicProperties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
            output.Write("(");
            var delimiter = "";
            destructureCount = destructureCount + 1;
            foreach (var propertyInfo in publicProperties)
            {
                var propertyValue = propertyInfo.GetValue(obj, null);
                WriteProperty(propertyInfo.Name, propertyValue, output, arrayCount, destructureCount, ref delimiter);
            }
            output.Write(")");
        }

        static string Escape(string s)
        {
            if (s == null) return null;

            StringBuilder escapedResult = null;
            var cleanSegmentStart = 0;
            for (var i = 0; i < s.Length; ++i)
            {
                var c = s[i];
                if (c == '\\' || c == '\'')
                //    if (c < (char)32 || c == '\\' || c == '\'')
                {
                    if (escapedResult == null)
                        escapedResult = new StringBuilder();

                    escapedResult.Append(s.Substring(cleanSegmentStart, i - cleanSegmentStart));
                    cleanSegmentStart = i + 1;

                    switch (c)
                    {
                        case '\'':
                            {
                                escapedResult.Append("\\'");
                                break;
                            }
                        case '\\':
                            {
                                escapedResult.Append("\\\\");
                                break;
                            }
                        //case '\n':
                        //    {
                        //        escapedResult.Append("\\n");
                        //        break;
                        //    }
                        //case '\r':
                        //    {
                        //        escapedResult.Append("\\r");
                        //        break;
                        //    }
                        //case '\f':
                        //    {
                        //        escapedResult.Append("\\f");
                        //        break;
                        //    }
                        //case '\t':
                        //    {
                        //        escapedResult.Append("\\t");
                        //        break;
                        //    }
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

        static void WriteArray(IList array, TextWriter output, int arrayCount, int destructureCount)
        {
            output.Write("[");
            arrayCount = arrayCount + 1;
            for (var index = 0; index < array.Count; index++)
            {
                if (index > 0)
                {
                    output.Write(",");
                }
                var value = array[index];
                WritePropertyValue(value, output, arrayCount, destructureCount);
            }
            output.Write("]");
        }

        static void WriteDictionary(IDictionary<string, object> dictionary, TextWriter output, int arrayCount, int destructureCount)
        {
            output.Write("(");
            destructureCount = destructureCount + 1;
            var delimiter = "";
            foreach (var kvp in dictionary)
            {
                WriteProperty(kvp.Key, kvp.Value, output, arrayCount, destructureCount, ref delimiter);
            }
            output.Write(")");
        }

        static void WriteBoolean(bool value, TextWriter output)
        {
            output.Write(value ? "true" : "false");
        }

        static void WriteByte(byte value, TextWriter output)
        {
            output.Write("0x");
            output.Write(value.ToString("X2"));
        }

        static void WriteByteArray(byte[] value, TextWriter output)
        {
            foreach (var b in value)
            {
                output.Write(b.ToString("X2"));
            }
        }

        static void WriteDateTime(DateTime value, TextWriter output)
        {
            output.Write(value.ToString("s"));
        }

        static void WriteDateTimeOffset(DateTimeOffset value, TextWriter output)
        {
            output.Write(value.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszzz"));
        }

        static void WriteProperty(string name, object value, TextWriter output, int arrayCount, int destructureCount, ref string delimiter)
        {
            output.Write(delimiter);
            var destructure = name.StartsWith("@");
            WritePropertyName(name, output);
            // TODO: Support IDictionary<string, object> as well ... but really need to start being careful of circular references
            if (name.StartsWith("@"))
            {
                DestructurePropertyValue(value, output, arrayCount, destructureCount);
            }
            else
            {
                WritePropertyValue(value, output, arrayCount, destructureCount);
            }
            delimiter = " ";
        }

        static void WritePropertyName(string name, TextWriter output)
        {
            foreach (var c in name)
            {
                if (c == ' ')
                {
                    output.Write('_');
                }
                else if (char.IsLetterOrDigit(c))
                {
                    output.Write(c);
                }
            }
            output.Write("=");
        }

        static void WritePropertyValue(object value, TextWriter output, int arrayCount, int destructureCount)
        {
            if (value == null)
            {
                output.Write("null");
                return;
            }
            Action<object, TextWriter> writer;
            if (LiteralWriters.TryGetValue(value.GetType(), out writer))
            {
                writer(value, output);
                return;
            }
            if (arrayCount < 1)
            {
                if (value is IList)
                {
                    WriteArray((IList)value, output, arrayCount, destructureCount);
                    return;
                }
            }
            if (destructureCount < 1 && arrayCount < 1)
            {
                if (value is IDictionary<string, object>)
                {
                    WriteDictionary((IDictionary<string, object>)value, output, arrayCount, destructureCount);
                    return;
                }
            }
            WriteString(value.ToString(), output);
        }

        static void WriteString(string value, TextWriter output)
        {
            var content = Escape(value);
            output.Write("'");
            output.Write(content);
            output.Write("'");
        }

        static void WriteToString(object number, TextWriter output)
        {
            output.Write(number.ToString());
        }

        private delegate void Action<T1, T2>(T1 a, T2 b);
    }
}
