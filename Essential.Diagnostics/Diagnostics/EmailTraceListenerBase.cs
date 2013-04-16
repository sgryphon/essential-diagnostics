using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Security.Permissions;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Essential.Diagnostics
{
    /// <remarks>
    /// Output trace messages of warning and error only as Email messages via SMTP. Every messages sent will go through a MailMessage queue which will send 
    /// messages through multiple SmtpClient connections in a connection pool.
    /// 
    /// The subject line of the Email message will be the text before ':', ';', ',', '.', '-' in the trace message, along with the identity of the application.
    /// The body of the Email will be the trace message.
    /// 
    /// The SMTP host settings are defined in MailSettings of app.config, as documented at http://msdn.microsoft.com/en-us/library/w355a94k.aspx. 
    /// 
    /// It supports the following custom attributes used in config:
    /// * maxConnections: Maximum SmtpClient connections in pool. The default is 2.
    /// 
    /// Because the Email messages will be sent in multiple threads, the send time of Email messages may not be in the exact order and the exact time of the creation of the message, tehrefore,
    /// it is recommended that the Email subject or body should log the datetime of the trace message.
    /// </remarks>
    public abstract class EmailTraceListenerBase : TraceListenerBase
    {
        const int defaultMaxConnections = 2;
        string toAddress;
        static string[] supportedAttributes = new string[] { 
            "maxConnections", "MaxConnections", "maxconnections",
            "subjectTemplate", "SubjectTemplate", "subjecttemplate",
            "bodyTemplate", "BodyTemplate", "bodytemplate",
            "traceTemplate", "TraceTemplate", "tracetemplate" };
        SmtpEmailWriterAsync emailWriter;
        static object smtpEmailHelperLockForInit = new object();

        protected EmailTraceListenerBase(string toAddress)
        {
            this.toAddress = toAddress;
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
        }

        /// <summary>
        /// Gets the email address the trace messages will be sent to.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value is part of initializeData; if the value changes the
        /// listener is recreated. See the constructor parameter for details
        /// of the supported formats.
        /// </para>
        /// </remarks>
        public string ToAddress
        {
            get
            {
                return toAddress;
            }
        }

        /// <summary>
        /// Gets or sets the maximum concurrent connections in the SmtpClient pool.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The is deffined by the custom attribute maxConnections. The default value is 2.
        /// </para>
        /// </remarks>
        public int MaxConnections
        {
            get
            {//todo: test with config change.
                string s = Attributes["maxConnections"];
                int value;
                return Int32.TryParse(s, out value) ? value : defaultMaxConnections;
            }
            set
            {
                Attributes["maxConnections"] = value.ToString(CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Gets or sets the template used to construct the email subject.
        /// </summary>
        public string SubjectTemplate
        {
            get 
            { 
                string s = Attributes["subjectTemplate"];
                if (String.IsNullOrEmpty(s))
                {
                    return DefaultSubjectTemplate;
                }
                return s;
            }
            set
            {
                Attributes["subjectTemplate"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the template used to construct the email body.
        /// </summary>
        public string BodyTemplate
        {
            get
            {
                string s = Attributes["bodyTemplate"];
                if (String.IsNullOrEmpty(s)) 
                {
                    return DefaultBodyTemplate;
                }
                return s;
            }
            set
            {
                Attributes["bodyTemplate"] = value;
            }
        }


        protected virtual string DefaultSubjectTemplate { get { return "{MESSAGE} -- Machine: {MACHINENAME}; User: {USER}; Process: {PROCESS}; AppDomain: {APPDOMAIN}"; } }

        protected virtual string DefaultBodyTemplate { get { return "Time: {LOCALDATETIME}\nMachine: {MACHINENAME}\nUser: {USER}\nProcess: {PROCESS}\nAppDomain: {APPDOMAIN}\n\n{MESSAGE}"; } }


        protected override string[] GetSupportedAttributes()
        {
            return supportedAttributes;
        }

        internal SmtpEmailWriterAsync EmailWriter
        {
            get
            {
                lock (smtpEmailHelperLockForInit)//use Lazy<T> in .net 4.
                {
                    if (emailWriter == null)
                    {
                        emailWriter = new SmtpEmailWriterAsync(MaxConnections);//the listener is staying forever generally, no need to care about CA2000.
                    }
                }

                return emailWriter;
            }
        }

        protected virtual void SendAllBeforeExit()
        {
            const int interval = 200;
            int totalWaitTime = 0;
            bool queueRunning = false;
            while ((EmailWriter.Busy) && (totalWaitTime < 2000))//The total execution time of all ProcessExit event handlers is limited, just as the total execution time of all finalizers is limited at process shutdown. The default is two seconds in .NET. 
            {
                System.Threading.Thread.Sleep(interval);
                totalWaitTime += interval;
                queueRunning = true;
            }

            if (queueRunning)//Because of the latancy of file system or the communication stack, sometimes event if MessageQueue.Idle becomes true, the files might not yet been saved before the process exits.
            {
                System.Threading.Thread.Sleep(1000);//so need to wait around 1 second more.
            }
        }

        protected void SendEmail(string subject, string body, string recipient)
        {
            EmailWriter.Send(subject, body, recipient);
        }

        /// <summary>
        /// Send Email via a SmtpClient in pool.
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        protected void SendEmail(string subject, string body)
        {
            SendEmail(subject, body, ToAddress);
        }

        void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            SendAllBeforeExit();
        }

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
