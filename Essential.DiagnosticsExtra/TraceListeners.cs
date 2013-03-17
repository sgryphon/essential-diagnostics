using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Security.Permissions;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Xml.Xsl;
using System.Xml;
using System.ComponentModel;
using System.Net.Mail;

namespace Essential.Diagnostics
{
    /// <summary>
    /// Common info to be logged during the startup of a process or AppDomain. And the date format prefers ISO8601.
    /// </summary>
    public static class StartupInfo
    {
        /// <summary>
        /// To be used to write the first trace when a process starts, with info of the process signature.
        /// </summary>
        /// <param name="basicMessage"></param>
        public static void WriteLine(string basicMessage)
        {
            Trace.TraceInformation(GetMessageWithProcessSignature(basicMessage));
        }

        /// <summary>
        /// Message suffixed with process info: machine name, user name, process name along with arguments, app domain name and local time.
        /// </summary>
        /// <param name="basicMessage"></param>
        /// <returns></returns>
        /// <remarks>Though a trace option or format might give a time stamp prefix to a message, StartupInfo enforces the info about local time in ISO8601 format.</remarks>
        public static string GetMessageWithProcessSignature(string basicMessage)
        {
            return String.Format("{0} -- Machine: {1}; User: {2}/{3}; Process: {4}; AppDomain: {5} Local Time: {6}.",
                basicMessage,
                Environment.MachineName,
                Environment.UserDomainName,
                Environment.UserName,
                Environment.CommandLine,
                AppDomain.CurrentDomain.ToString(),
                GetISO8601Text(DateTime.Now));
        }

        /// <summary>
        /// StartupParagraph plus basic message.
        /// </summary>
        /// <param name="basicMessage"></param>
        /// <returns></returns>
        public static string GetParagraph(string basicMessage)
        {
            return String.Format("{0}+Message:\n{1}", Paragraph, basicMessage);
        }

        /// <summary>
        /// Text lines presenting time, machine name, user name, process name and app domain.
        /// </summary>
        public static string Paragraph
        {
            get
            {
                return String.Format("Time        : {0}\n" +
                                     "Machine     : {1}\n" +
                                     "User        : {2}\\{3}\n" +
                                     "Process     : {4}\n" +
                                     "AppDomain   : {5}\n",
                    NowText,
                    Environment.MachineName,
                    Environment.UserDomainName,
                    Environment.UserName,
                    Environment.CommandLine,
                    AppDomain.CurrentDomain.ToString()
                    );
            }
        }

        public static string GetISO8601Text(DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static string NowShotText
        {
            get
            {
                return DateTime.Now.ToString("yyyyMMdd HHmmss");
            }
        }

        /// <summary>
        /// Now in ISO8601 
        /// </summary>
        public static string NowText
        {
            get
            {
                return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

    }


    /// <summary>
    /// Output trace messages of warning and error only as Email messages via SMTP. Every messages sent will go through a MailMessage queue which will send 
    /// messages through multiple SmtpClient connections in a connection pool.
    /// 
    /// The subject line of the Email message will be the text before ':', ';', ',', '.', '-' in the trace message, along with the identity of the application.
    /// The body of the Email will be the trace message.
    /// 
    /// The SMTP host settings are defined in MailSettings of app.config, as documented at http://msdn.microsoft.com/en-us/library/w355a94k.aspx. However, the from address in MailSettings is not used in this trace listener,
    /// since it might be used by the application. The trace listener should have its own from address in order to be filtered.
    /// 
    /// It supports the following custom attributes used in config:
    /// * fromAddress
    /// * toAddress
    /// * maxConnections: Maximum SmtpClient connections in pool. The default is 2.
    /// 
    /// Because the Email messages will be sent in multiple threads, the send time of Email messages may not be in the exact order and the exact time of the creation of the message, tehrefore,
    /// it is recommended that the Email subject or body should log the datetime of the trace message.
    /// </summary>
    /// <remarks>
    /// Each message is sent in an asynchnous call. If the host process is terminated, the thread running the sending will be terminated as well, 
    /// therefore the last few error message traced might be lost. Because of the latency of Email, performance and the limitation of Email relay, this listener is not so appropriate in
    /// a service app that expect tens of thousands of concurrent requests per minutes. Ohterwise, a critical error in the service app with trigger tens of thousands of warnings piled in the MailMessage queue.
    /// Alternatively, to avoid memory blow because of a larrge message queue, you may consider to make SmtpClient deliver messages to a directory through defining SmtpDeliveryMethod=SpecifiedPickupDirectory or PickupDirectoryFromIis,
    /// so the otehr application may pickup and send.
    /// 
    /// In addition, firewall, anti-virus software and the mail server spam policy may also have impact on this listener, so system administrators have to be involved to ensure the operation of this listener.
    /// 
    /// If you define SmtpDeliveryMethod to make SmtpClient write messages to a pickup directory, please make sure the total number of client connections is no more than 2, since 
    /// concurrent access to a file system is almost certain to slow down the overall performance unless you are RAID. For the best performance, you really need to run some integration test
    /// to get the optimized number for a RAID system.
    /// 
    /// Please note, defining a large number of SMTP connections in pool may not necessarily give you the best performance. The overall performance with the optimized number of concurrent connections depends on the following factors:
    /// 1. The number of processors
    /// 2. The implementation/config of the SMTP server
    /// 3. The average size of Email messages
    /// </remarks>
    public abstract class EmailTraceListenerBase : TraceListener
    {
        protected EmailTraceListenerBase()
        {
        }

        protected EmailTraceListenerBase(string name)
            : base(name)
        {
        }

        protected string FromAddress { get { return Attributes["fromAddress"]; } }

        protected string ToAddress { get { return Attributes["toAddress"]; } }

        /// <summary>
        /// Maximum concurrent connections in the SmtpClient pool, defined in custom attribute maxConnections. The default value is 2.
        /// </summary>
        protected int MaxConnections
        {
            get
            {
                string s = Attributes["maxConnections"];
                int m;
                return Int32.TryParse(s, out m) ? m : 2;
            }
        }

        static string[] supportedAttributes = new string[] { "fromAddress",  "toAddress", "maxConnections" };

        protected override string[] GetSupportedAttributes()
        {
            return supportedAttributes;
        }

        // object clientLock = new object();

        MailMessage CreateMailMessage(string subject, string body, string recipient)
        {
            MailMessage mailMessage = new MailMessage();

            mailMessage.IsBodyHtml = false;
            mailMessage.BodyEncoding = Encoding.UTF8;
            mailMessage.From = new MailAddress(FromAddress);
            mailMessage.To.Add(recipient);
            if (String.IsNullOrEmpty(subject))
                subject = "Subject empty";

            EmailUtil.SanitiseEmailSubject(mailMessage, subject);
            mailMessage.Body = StartupInfo.GetParagraph(body);
            return mailMessage;
        }

        protected void SendEmailAsync(string subject, string body, string recipient)
        {
            MessageQueue.AddAndSendAsync(CreateMailMessage(subject, body, recipient));
        }

        /// <summary>
        /// Send Email via a SmtpClient in pool.
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        protected void SendEmailAsync(string subject, string body)
        {
            SendEmailAsync(subject, body, ToAddress);
        }

        /// <summary>
        /// Send Email via a new SmtpClient.
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        protected void SendEmail(string subject, string body)
        { 
            SmtpClient client=null;
            try
            {
                client = new SmtpClient();
                client.Send(FromAddress, ToAddress, subject, body);

            }
            finally
            {
                if  ((client!=null)&&(typeof(SmtpClient).GetInterface("IDisposable") != null))
                {
                    (client as IDisposable).Dispose();
                }
            }
        }

        

        MailMessageQueue messageQueue;

        static object objectLock = new object();

        MailMessageQueue MessageQueue
        {
            get
            {
                lock (objectLock)
                {
                    if (messageQueue == null)
                    {
                        messageQueue = new MailMessageQueue(MaxConnections);
                        Debug.WriteLine("MessageQueue is created with some connections: " + MaxConnections);
                    }
                }

                return messageQueue;
            }
        }


    }

    internal static class EmailUtil
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
            return StartupInfo.GetMessageWithProcessSignature(ss[0]);
        }

        /// <summary>
        /// Email subject generated by other part of the systems might contain invalid characters, or be too long. This function 
        /// will clean up.
        /// </summary>
        /// <param name="mailMessage"></param>
        /// <param name="subject"></param>
        internal static void SanitiseEmailSubject(MailMessage mailMessage, string subject)
        {
            if (String.IsNullOrEmpty(subject))
                return;

            if (mailMessage == null)
                throw new ArgumentNullException("mailMessage");

            const int subjectMaxLength = 254; //though .NET lib does not place any restriction, and the recent standard of Email seems to be 254, which sounds safe.
            if (subject.Length > 254)
                subject = subject.Substring(0, subjectMaxLength);

            try
            {
                for (int i = 0; i < subject.Length; i++)
                {
                    if (Char.IsControl(subject[i]))
                    {
                        mailMessage.Subject = subject.Substring(0, i);
                        return;
                    }
                }
                mailMessage.Subject = subject;

            }
            catch (ArgumentException)
            {
                mailMessage.Subject = "Invalid subject removed by TraceListener";
            }
        }


    }

    /// <summary>
    /// Send Email against TraceWarning and TraceError.
    /// </summary>
    public class EmailTraceListener : EmailTraceListenerBase
    {
        public EmailTraceListener()
            : base()
        {
        }

        public EmailTraceListener(string name)
            : base(name)
        {
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            if (String.IsNullOrEmpty(message))
                return;

            if (eventCache == null)
                throw new ArgumentNullException("eventCache");

            string subject = EmailUtil.ExtractSubject(message);

            string messageformated;

            if (eventType <= TraceEventType.Error)//Error or Critical
            {
                messageformated = "Error: "
                    + message + Environment.NewLine
                    + "  Call Stack: " + eventCache.Callstack;
                SendEmailAsync(subject, messageformated);
            }
            else if (eventType == TraceEventType.Warning)
            {
                messageformated = "Warning: " + message;
                SendEmailAsync(subject, messageformated);
            }
            //ignore the other types
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            TraceEvent(eventCache, source, eventType, id, String.Format(format, args));
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="eventCache"></param>
        /// <param name="source"></param>
        /// <param name="eventType"></param>
        /// <param name="id"></param>
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            // do nothing  base.TraceEvent(eventCache, source, eventType, id);
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="message"></param>
        public override void Write(string message)
        {
            //do nothing
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="message"></param>
        public override void WriteLine(string message)
        {
            //do nothing
        }

    }

    /// <summary>
    /// Intended to be used in console apps which will send all warning/error traces via Email at the end of the process.
    /// 
    /// Put error and warning trace messages into a buffer, taking care of Trace.Trace*() while ignoring Trace.Write*(). The client codes may want to get
    /// all messages at the end of a business operation in order to send out the messages in one batch.
    /// For error message, call stack and local datetime will be accompanied.
    /// 
    /// 
    /// </summary>
    /// <example>            
    ///     ErrorBufferTraceListener bufferListener = new ErrorBufferTraceListener("ErrorBufferOfAdWordsAgent");
    ///     Trace.Listeners.Add(bufferListener);
    ///     ...
    ///        if (bufferListener.HasErrors)
    ///        {
    ///            SendEmail("When uploading to AdWords, there are errors", bufferListener.Messages);
    ///        }
    ///
    ///</example>
    public class ErrorBufferTraceListener : EmailTraceListenerBase
    {
        public ErrorBufferTraceListener()
        {
            EventMessagesBuffer = new StringBuilder();
        }

        public ErrorBufferTraceListener(string name)
            : base(name)
        {
            EventMessagesBuffer = new StringBuilder();
        }

        void EventMessagesBufferAdd(string s)
        {
            EventMessagesBuffer.AppendLine(s);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            if (eventType <= TraceEventType.Error)//Error or Critical
            {
                EventMessagesBufferAdd(StartupInfo.GetISO8601Text(eventCache.DateTime) + " "
                    + "Error: " + message);
                EventMessagesBufferAdd("  Call Stack: " + eventCache.Callstack);
            }
            else if (eventType == TraceEventType.Warning)
            {
                EventMessagesBufferAdd(StartupInfo.GetISO8601Text(eventCache.DateTime) + " "
                    + "Warning: " + message);
            }
            //Ignore other event type
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            TraceEvent(eventCache, source, eventType, id, String.Format(format, args));
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            // do nothing  base.TraceEvent(eventCache, source, eventType, id);
        }

        public override void Write(string message)
        {
            //do nothing
        }

        public override void WriteLine(string message)
        {
            //do nothing
        }

        /// <summary>
        /// Message buffer
        /// </summary>
        public StringBuilder EventMessagesBuffer { get; private set; }

        public void ClearEventMessagesBuffer()
        {
            EventMessagesBuffer = new StringBuilder();
        }

        public bool HasEventErrors { get { return EventMessagesBuffer.Length > 0; } }

        protected void SendEventMessages()
        {
            if (!HasEventErrors)
                return;

            string body = EventMessagesBuffer.ToString(); 
            string firstMessage = body.Substring(0, body.IndexOf("\n"));// EventMessagesBuffer.Count == 0 ? String.Empty : EventMessagesBuffer[0];
            Debug.WriteLine("firstMessage: " + firstMessage);
            string subject = EmailUtil.ExtractSubject(firstMessage);
            SendEmail(subject, body);
        }

        static ErrorBufferTraceListener FindListener()
        {
            ErrorBufferTraceListener myListener = null;
            foreach (TraceListener t in Trace.Listeners)
            {
                myListener = t as ErrorBufferTraceListener;
                if (myListener != null)
                    return myListener;
            }

            Trace.TraceError("You want to use ErrorBufferTraceListener, but there's none in Trace.Listeners, probably not defined in the config file.");
            return null;
        }

        /// <summary>
        /// This is commonly called at the end of a process.
        /// </summary>
        public static void SendMailOfEventMessages()
        {
            var listener = FindListener();
            if (listener != null)
            {
                listener.SendEventMessages();
            }
        }
    }

}
