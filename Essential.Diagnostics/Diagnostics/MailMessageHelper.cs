using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Essential.Diagnostics
{
    internal static class MailMessageHelper
    {
        internal static string ExtractSubject(string message)
        {
            Regex regex = new Regex(@"((\d{1,4}[\:\-\s/]){2,3}){1,2}");//timestamp in trace
            Match match = regex.Match(message);
            if (match.Success)
            {
                message = message.Substring(match.Length);//so remove the timestamp
            }

            string[] ss = message.Split(new string[] { ";", ", ", ". " }, 2, StringSplitOptions.None);
            return ss[0];
        }


        internal static string SanitiseSubject(string subject)
        {
            const int subjectMaxLength = 254; //though .NET lib does not place any restriction, and the recent standard of Email seems to be 254, which sounds safe.
            if (subject.Length > 254)
                subject = subject.Substring(0, subjectMaxLength);

            try
            {
                for (int i = 0; i < subject.Length; i++)
                {
                    if (Char.IsControl(subject[i]))
                    {
                        return subject.Substring(0, i);
                    }
                }
                return subject;

            }
            catch (ArgumentException)
            {
                return "Invalid subject removed by TraceListener";
            }

        }

    }


}
