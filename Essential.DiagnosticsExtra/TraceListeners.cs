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
    /// Email trace listeners output trace messages (warning and error only) to SMTP Email system as Email messages. Every message sent will go through a new connection in a new thread.
    /// 
    /// This base class defines basic functions of trace listeners with Email features. The applications need no knowledge of the 
    /// Email system, and it is up to the app.config to wire such output.
    /// 
    /// The subject line of the Email message will be the text before ':', ';', ',', '.', '-' in the trace message, along with the identity of the application.
    /// The body of the Email will be the trace message.
    /// It supports the following custom attributes used in config:
    /// * smtpServer
    /// * senderAddress
    /// * senderName
    /// * eventRecipient
    /// </summary>
    /// <remarks>The log message is sent in an asynchnous call. If the host process is terminated, the thread running the sending will be terminated as well, 
    /// therefore the last few error message traced might be lost. Because of the latency of Email, performance and the limitation of Email relay, this listener is not so appropriate in
    /// a service app that expect tens of thousands of concurrent requests. 
    /// In addition, firewall, anti-virus software and the mail server spam policy may also have impact on this listener, so system administrators have to be involved to ensure the operation of this listener.</remarks>
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

        protected string FromName { get { return Attributes["fromName"]; } }

        protected string ToAddress { get { return Attributes["toAddress"]; } }

        protected int MaxConnections
        {
            get
            {
                string s = Attributes["maxConnections"];
                int m;
                return Int32.TryParse(s, out m) ? m : 2;
            }
        }

        static string[] supportedAttributes = new string[] { "fromAddress", "fromName", "toAddress", "maxConnections" };

        protected override string[] GetSupportedAttributes()
        {
            return supportedAttributes;
        }

        // object clientLock = new object();

        protected void SendEmailAsync(string subject, string body, string recipient)
        {
            MessageQueue.AddAndSendAsync(CreateMailMessage(subject, body, recipient));
        }

        MailMessage CreateMailMessage(string subject, string body, string recipient)
        {
            MailMessage mailMessage = new MailMessage();

            mailMessage.IsBodyHtml = false;
            mailMessage.BodyEncoding = Encoding.UTF8;
            mailMessage.From = new MailAddress(FromAddress, FromName);
            mailMessage.To.Add(recipient);
            if (String.IsNullOrEmpty(subject))
                subject = "Subject empty";

            SanitiseEmailSubject(mailMessage, subject);
            mailMessage.Body = StartupInfo.GetParagraph(body);
            return mailMessage;
        }

        public void SendEmailAsync(string subject, string body)
        {
            SendEmailAsync(subject, body, ToAddress);
        }

        public void SendEmail(string subject, string body)
        {
            MessageQueue.AddAndSendAsync(CreateMailMessage(subject, body, ToAddress));
        }

        SmtpClient client;


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

            string subject = ExtractSubject(message);

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
            EventMessagesBuffer = new List<string>();
        }

        public ErrorBufferTraceListener(string name)
            : base(name)
        {
            EventMessagesBuffer = new List<string>();
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            if (eventType <= TraceEventType.Error)//Error or Critical
            {
                EventMessagesBuffer.Add(StartupInfo.GetISO8601Text(eventCache.DateTime) + " "
                    + "Error: " + message);
                EventMessagesBuffer.Add("  Call Stack: " + eventCache.Callstack);
            }
            else if (eventType == TraceEventType.Warning)
            {
                EventMessagesBuffer.Add(StartupInfo.GetISO8601Text(eventCache.DateTime) + " "
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
        public IList<string> EventMessagesBuffer { get; private set; }

        public void ClearEventMessagesBuffer()
        {
            EventMessagesBuffer.Clear();
        }

        public bool HasEventErrors { get { return EventMessagesBuffer.Count > 0; } }

        protected void SendEventMessages()
        {
            if (!HasEventErrors)
                return;

            StringBuilder builder = new StringBuilder();
            foreach (string s in EventMessagesBuffer)
            {
                builder.AppendLine(s);
            }
            string body = builder.ToString();
            string firstMessage = EventMessagesBuffer.Count == 0 ? String.Empty : EventMessagesBuffer[0];
            string subject = ExtractSubject(firstMessage);
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

            throw new InvalidOperationException("You want to use ErrorBufferTraceListener, but there's none in Trace.Listeners, probably not defined in the config file.");
        }

        /// <summary>
        /// This is commonly called at the end of a process.
        /// </summary>
        public static void SendMailOfEventMessages()
        {
            FindListener().SendEventMessages();
        }
    }

}
