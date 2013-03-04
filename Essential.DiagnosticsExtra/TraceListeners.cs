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
        internal static IAsyncResult SendEmailAsync(string smtpServer, MailMessage mailMessage)
        {
            SendEmailHandler d = (ss, mm) =>
            {
                SendEmail(ss, mm);
            };

            return d.BeginInvoke(smtpServer, mailMessage, EmailSentCallback, null);
        }

         static void EmailSentCallback(IAsyncResult asyncResult)
        {
            AsyncResult resultObj = (AsyncResult)asyncResult;
            SendEmailHandler d = (SendEmailHandler)resultObj.AsyncDelegate;
            try
            {
                d.EndInvoke(asyncResult);
            }
            catch (Exception e)
            {
                Trace.TraceWarning(e.ToString());
                throw;
            }
        }

        internal static bool SendEmail(string smtpServer, MailMessage mailMessage)
        {
            SmtpClient client = new SmtpClient(smtpServer);

            try
            {
                client.Send(mailMessage);
                Debug.WriteLine("Email sent");
                return true;
            }
            catch (SmtpException e)
            {
                Trace.WriteLine(e.ToString());
            }
            catch (IOException e)
            {
                Trace.WriteLine(e.ToString());
            }

            return false;
        }


    }

    internal delegate void SendEmailHandler(string smtpServer, MailMessage mailMessage);

    /// <summary>
    /// Email trace listeners output trace messages (warning and error only) to SMTP Email system as Email messages.
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
    /// therefore the last few error message traced might be lost.</remarks>

    public abstract class EmailTraceListenerBase : TraceListener
    {
        protected EmailTraceListenerBase()
        {
        }

        protected EmailTraceListenerBase(string name)
            : base(name)
        {
        }

        protected string SmtpServer { get { return Attributes["smtpServer"]; } }

        protected string SenderAddress { get { return Attributes["senderAddress"]; } }

        protected string SenderName { get { return Attributes["senderName"]; } }

        protected string EventRecipient { get { return Attributes["eventRecipient"]; } }

        static string[] supportedAttributes = new string[] { "smtpServer", "senderAddress", "senderName", "eventRecipient" };

        protected override string[] GetSupportedAttributes()
        {
            return supportedAttributes;
        }

        // object clientLock = new object();

        protected void SendEmail(string subject, string body, string recipient)
        {
            using (MailMessage mailMessage = new MailMessage())
            {
                mailMessage.IsBodyHtml = false;
                mailMessage.BodyEncoding = Encoding.UTF8;
                mailMessage.From = new MailAddress(SenderAddress, SenderName);
                mailMessage.To.Add(recipient);
                if (String.IsNullOrEmpty(subject))
                    subject = "Subject empty";

                MailUtil.AssignEmailSubject(mailMessage, subject);
                mailMessage.Body = StartupInfo.GetParagraph(body);
                SendEmailSafely(mailMessage);
            }
        }

        protected void SendEmailSafely(MailMessage mailMessage)
        {
            MailUtil.SendEmailAsync(SmtpServer, mailMessage);
        }

        public void SendEmail(string subject, string body)
        {
            SendEmail(subject, body, EventRecipient);
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

        internal static string ExtractSubject(string message, string category)
        {
            return category + ": " + ExtractSubject(message);
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
                SendEmail(subject, messageformated);
            }
            else if (eventType == TraceEventType.Warning)
            {
                messageformated = "Warning: " + message;
                SendEmail(subject, messageformated);
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
    /// Besides sending Email against TraceWarning and TraceError, provide multiple recipients for Trace.WriteLine(message, category), with a mapping between a category and multiple recipients.
    /// The mapping must be assigined in client codes during execution.
    /// 
    /// This is designed for applications requireing recipients to be obtained from a database. However, messages in TraceEvent() will still be
    /// sent to only a recipient defined in config, since there may be many error messages long before recipients defined in db could possibly be read.
    /// </summary>
    public class EmailExTraceListener : EmailTraceListener
    {
        EmailTraceDestination addressbook;

        public EmailExTraceListener()
        {

        }

        public EmailExTraceListener(string name)
            : base(name)
        {

        }

        /// <summary>
        /// This is to be called inside client codes to give the listener an addressbook, while the listener is wired through config as usual.
        /// </summary>
        /// <param name="addressbook"></param>
        public static void AssignAddressbook(EmailTraceDestination addressbook)
        {
            EmailExTraceListener myListener = null;
            foreach (TraceListener t in Trace.Listeners)
            {
                myListener = t as EmailExTraceListener;
                if (myListener != null)
                    break;
            }

            if (myListener == null)
                throw new ArgumentException("You want to use EmailExTraceListener, but there's none in Trace.Listeners, probably not defined in the config file.");

            myListener.addressbook = addressbook;
        }

        public void SendEmail(string subject, string body, IEnumerable<string> addresses)
        {
            if (addresses == null)
                throw new ArgumentNullException("addresses");

            using (MailMessage mailMessage = new MailMessage())
            {
                mailMessage.IsBodyHtml = false;
                mailMessage.BodyEncoding = Encoding.UTF8;
                mailMessage.From = new MailAddress(SenderAddress, SenderName);
                foreach (string address in addresses)
                {
                    mailMessage.To.Add(address);
                }

                MailUtil.AssignEmailSubject(mailMessage, subject);
                mailMessage.Body = StartupInfo.GetParagraph(body);
                SendEmailSafely(mailMessage);
            }

        }

        public override void WriteLine(string message, string category)
        {
            if (addressbook == null)
                throw new ArgumentException("addressbook of recipients is not assigned. This must be fixed by calling AssignAddressbook().");

            try
            {
                IEnumerable<string> addresses = addressbook[category];
                SendEmail(ExtractSubject(message, category), message, addresses);
            }
            catch (KeyNotFoundException)
            {
                string m = String.Format("Can not find addresses for category {0}, and please make sure the category has Email addressed associated. Error message is in body.", category);
                SendEmail(m, message);
                return;
            }
        }

        public override void Write(string message, string category)
        {
            WriteLine(message, category);
        }

    }

    /// <summary>
    /// Mapping between category and a string array. The key category is case insensitive.
    /// </summary>
    /// 
    [Serializable]
    public class EmailTraceDestination : Dictionary<string, IEnumerable<string>>
    {
        public EmailTraceDestination()
            : base(StringComparer.CurrentCultureIgnoreCase)
        {

        }

        public EmailTraceDestination(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public IEnumerable<string> GetEmailAddresses(string category)
        {
            return this[category];
        }
    }

}
