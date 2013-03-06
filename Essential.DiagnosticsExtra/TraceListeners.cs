using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Security.Permissions;
using System.Net.Mail;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Xml.Xsl;
using System.Xml;
using System.ComponentModel;
using System.Runtime.Remoting.Messaging;

namespace Essential.Diagnostics
{
    /// <summary>
    /// Common info to be logged during the startup of a process. And the date format prefers ISO8601.
    /// </summary>
    public static class StartupInfo
    {
        /// <summary>
        /// To be used to write the first trace when a process start. So the message will appear in both the log file and the event viewer.
        /// </summary>
        /// <param name="basicMessage"></param>
        public static void WriteLine(string basicMessage)
        {
            Trace.TraceInformation(GetLine(basicMessage));
        }

        public static string GetLine(string basicMessage)
        {
            return String.Format("{0} -- Machine: {1}; User: {2}/{3}; Process: {4}; AppDomain: {5} Time: {6}.",
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
            return String.Format("Time        : {4}\n" +
                                 "Machine     : {0}\n" +
                                 "User        : {1}\\{2}\n" +
                                 "Process     : {3}\n" +
                                 "Notification: {5}\n\n",
                Environment.MachineName,//0
                Environment.UserDomainName,//1
                Environment.UserName,//2
                Environment.CommandLine,//3
                GetISO8601Text(DateTime.Now),//4
               basicMessage);//5
        }

        /// <summary>
        /// Text lines presenting time, machine name, user name and process name.
        /// </summary>
        public static string Paragraph
        {
            get
            {
                return String.Format("Time        : {4}\n" +
                                     "Machine     : {0}\n" +
                                     "User        : {1}\\{2}\n" +
                                     "Process     : {3}\n",
                    Environment.MachineName,//0
                    Environment.UserDomainName,//1
                    Environment.UserName,//2
                    Environment.CommandLine,//3
                    GetISO8601Text(DateTime.Now));
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

        public static string NowText
        {
            get
            {
                return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

    }

    internal class MailInfo
    {
        internal string From { get; set; }
        internal string Recipient { get; set; }
        internal DateTime TimeSent { get; set; }

        internal Exception Exception { get; set; }
    }

    /// <summary>
    /// A wrapper of Smtpclient. Though Smtpclient provides SendAsync, this function apparently only do the transmitting in a new thread, while the initial hand shaking is still in the caller thread.
    /// So this class will do the whole thing in a new thread.
    /// </summary>
    internal class SmtpClientAsync
    {
        SmtpClient client;

        internal SmtpClientAsync(string smtpServerName)
        {
            client = new SmtpClient(smtpServerName);
        }

        internal event SendCompletedEventHandler SendCompleted;

        /// <summary>
        /// Send mail via a spin off thread. And mailMessage will be disposed in the thread.
        /// </summary>
        /// <param name="smtpServer"></param>
        /// <param name="mailMessage"></param>
        /// <returns></returns>
        /// <remarks>If the host process is terminated, the thread running the sending will be terminated as well, therefore the last few error message traced might be lost.</remarks>
        internal IAsyncResult SendAsync(MailMessage mailMessage)
        {
            MailInfo state = new MailInfo()
            {
                From = mailMessage.From.Address,
                Recipient = mailMessage.To[0].Address,
                TimeSent = DateTime.Now,
            };

            SendEmailHandler d = (mm) =>
            {
                if (!Send(mm, state))
                {
                    Debug.WriteLine("Hey, send fails, please check trace following.");
                }
            };

            return d.BeginInvoke(mailMessage, EmailSentCallback, state);
        }

        void EmailSentCallback(IAsyncResult asyncResult)
        {
            AsyncResult resultObj = (AsyncResult)asyncResult;
            SendEmailHandler d = (SendEmailHandler)resultObj.AsyncDelegate;
            MailInfo state = null;
            try
            {
                d.EndInvoke(asyncResult);
                Debug.WriteLine("IsCompleted: " + asyncResult.IsCompleted);
                state = (MailInfo)asyncResult.AsyncState;
                if (SendCompleted != null)
                {
                    SendCompleted(this, new AsyncCompletedEventArgs(state.Exception, false, state));
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("The thread is terminated, and general exception is caught. " + e.ToString());
                SendCompleted(this, new AsyncCompletedEventArgs(e, false, null));
                throw;
            }
        }

        /// <summary>
        /// Send and dispose mailMessage.
        /// </summary>
        /// <param name="mailMessage"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        bool Send(MailMessage mailMessage, MailInfo state)
        {
            try
            {
#if DEBUG
                DateTime dt = DateTime.Now;
#endif
                client.Send(mailMessage);
                Debug.WriteLine("Email sent in " + (DateTime.Now - dt).TotalSeconds.ToString());
                return true;
            }
            catch (SmtpException e)
            {
                Debug.WriteLine("Caught in Send. " + e.ToString());
                state.Exception = e;
            }
            catch (IOException e)
            {
                Debug.WriteLine("Caught in Send. " + e.ToString());
                state.Exception = e;
            }
            finally
            {
                mailMessage.Dispose();
            }

            return false;

        }
    }

    internal static class MailUtil
    {
        internal static void AssignEmailSubject(MailMessage mailMessage, string subject)
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

        /// <summary>
        /// Send mail via a spin off thread.
        /// </summary>
        /// <param name="smtpServer"></param>
        /// <param name="mailMessage"></param>
        /// <returns></returns>
        /// <remarks>If the host process is terminated, the thread running the sending will be terminated as well, therefore the last few error message traced might be lost.</remarks>
        internal static void SendEmailAsync(string smtpServer, MailMessage mailMessage)
        {
            SmtpClientAsync client = new SmtpClientAsync(smtpServer);
            client.SendCompleted += new SendCompletedEventHandler(client_SendCompleted);
            client.SendAsync(mailMessage);
        }

        static void client_SendCompleted(object sender, AsyncCompletedEventArgs e)
        {
#if DEBUG
            MailInfo info = (MailInfo)e.UserState;
            Debug.WriteLine(String.Format("Email sent to {0} at {1}", info.Recipient, info.TimeSent));
#endif
        }

    }

    internal delegate void SendEmailHandler(MailMessage mailMessage);

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

        protected string SmtpServer
        {
            get
            {
                string s = Attributes["smtpServer"];
                if (String.IsNullOrEmpty(s))
                {
                    throw new InvalidOperationException("Hey, smtpServer needs to be defined in config.");
                }
                return s;
            }
        }

        protected string SenderAddress { get { return Attributes["senderAddress"]; } }

        protected string SenderName { get { return Attributes["senderName"]; } }

        protected string EventRecipient { get { return Attributes["eventRecipient"]; } }

        static string[] supportedAttributes = new string[] { "smtpServer", "senderAddress", "senderName", "eventRecipient" };

        protected override string[] GetSupportedAttributes()
        {
            return supportedAttributes;
        }

        // object clientLock = new object();

        protected void SendEmailAsync(string subject, string body, string recipient)
        {
            MailUtil.SendEmailAsync(SmtpServer, CreateMailMessage(subject, body, recipient));//mailMessage will be disposed in SendEmailAsync.
        }

        MailMessage CreateMailMessage(string subject, string body, string recipient)
        {
            MailMessage mailMessage = new MailMessage();

            mailMessage.IsBodyHtml = false;
            mailMessage.BodyEncoding = Encoding.UTF8;
            mailMessage.From = new MailAddress(SenderAddress, SenderName);
            mailMessage.To.Add(recipient);
            if (String.IsNullOrEmpty(subject))
                subject = "Subject empty";

            MailUtil.AssignEmailSubject(mailMessage, subject);
            mailMessage.Body = StartupInfo.GetParagraph(body);
            return mailMessage;
        }

        public void SendEmailAsync(string subject, string body)
        {
            SendEmailAsync(subject, body, EventRecipient);
        }

        public void SendEmail(string subject, string body)
        {
            using (MailMessage message = CreateMailMessage(subject, body, EventRecipient))
            {
                Client.Send(message);
            }
        }

        SmtpClient client;

        SmtpClient Client
        {
            get
            {
                if (client == null)
                {
                    client = new SmtpClient(SmtpServer);
                }
                return client;
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
            return StartupInfo.GetLine(ss[0]);
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
