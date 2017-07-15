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
                { typeof(DateTimeOffset), (v, w) => WriteOffset((DateTimeOffset)v, w) },
            };
        }

        public static void FormatProperties(IEnumerable<KeyValuePair<string, object>> properties, IList<string> excludeKeys, TextWriter output, ref string delimiter)
        {
            foreach (var kvp in properties)
            {
                if (!excludeKeys.Contains(kvp.Key))
                {
                    WriteProperty(kvp.Key, kvp.Value, output, ref delimiter);
                }
            }
        }

        static void DestructurePropertyValue(object obj, TextWriter output)
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
            foreach (var propertyInfo in publicProperties)
            {
                var propertyValue = propertyInfo.GetValue(obj, null);
                WriteProperty(propertyInfo.Name, propertyValue, output, ref delimiter);
            }
            output.Write(")");
        }



        static void WriteProperty(string name, object value, TextWriter output, ref string delimiter)
        {
            output.Write(delimiter);
            var destructure = name.StartsWith("@");
            WritePropertyName(name, output);
            if (name.StartsWith("@"))
            {
                DestructurePropertyValue(value, output);
            }
            else
            {
                WritePropertyValue(value, output);
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

        static void WriteArray(IList array, TextWriter output)
        {
            output.Write("[");
            for (var index = 0; index < array.Count; index++)
            {
                if (index > 0)
                {
                    output.Write(",");
                }
                var value = array[index];
                WritePropertyValue(value, output);
            }
            output.Write("]");
        }

        static void WritePropertyValue(object value, TextWriter output)
        {
            if (value == null)
            {
                output.Write("null");
                return;
            }

            if (value is IList)
            {
                WriteArray((IList)value, output);
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

        static void WriteByte(byte value, TextWriter output)
        {
            output.Write("0x");
            output.Write(value.ToString("X"));
        }


        static void WriteOffset(DateTimeOffset value, TextWriter output)
        {
            output.Write(value.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszzz"));
        }

        static void WriteDateTime(DateTime value, TextWriter output)
        {
            output.Write(value.ToString("s"));
        }

        static void WriteString(string value, TextWriter output)
        {
            var content = Escape(value);
            output.Write("'");
            output.Write(content);
            output.Write("'");
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

        private delegate void Action<T1, T2>(T1 a, T2 b);
    }
}
