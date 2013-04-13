using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Security.Permissions;
using System.ComponentModel;

namespace Essential.Diagnostics
{
    /// <summary>
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
    /// </summary>
    public abstract class EmailTraceListenerBase : TraceListenerBase
    {
        protected EmailTraceListenerBase()
        {
            smtpConfig = System.Configuration.ConfigurationManager.GetSection("system.net/mailSettings/smtp") as System.Net.Configuration.SmtpSection;
        }

        protected EmailTraceListenerBase(string toAddress)
            : this()
        {
            this.toAddress = toAddress;
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
        }

        void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            SendAllBeforeExit();
        }

        protected virtual void SendAllBeforeExit()
        {
            const int interval = 200;
            int totalWaitTime = 0;
            bool queueRunning = false;
            while ((SmtpEmailHelper.Busy) && (totalWaitTime < 2000))//The total execution time of all ProcessExit event handlers is limited, just as the total execution time of all finalizers is limited at process shutdown. The default is two seconds in .NET. 
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

        string toAddress;

        System.Net.Configuration.SmtpSection smtpConfig;

        /// <summary>
        /// Defined in attribute fromAddress or property From of system.net/mailSettings/smtp. 
        /// </summary>
        protected string FromAddress
        {
            get 
            {
               return smtpConfig.From;
            }
        }

        /// <summary>
        /// Normally taken from initializeData of config.
        /// 
        /// </summary>
        protected string ToAddress
        {
            get
            {
                return toAddress;
            }
        }

        /// <summary>
        /// Maximum concurrent connections in the SmtpClient pool, defined in custom attribute maxConnections. The default value is 2.
        /// </summary>
        protected int MaxConnections
        {
            get
            {//todo: test with config change.
                string s = Attributes["maxConnections"];
                int m;
                return Int32.TryParse(s, out m) ? m : 2;
            }
        }

        protected virtual string DefaultSubjectTemplate { get { return "{MESSAGE} -- Machine: {MACHINENAME}; User: {USER}; Process: {PROCESS}; AppDomain: {APPDOMAIN}"; } }

        protected virtual string DefaultBodyTemplate { get { return "Time: {LOCALDATETIME}\nMachine: {MACHINENAME}\nUser: {USER}\nProcess: {PROCESS}\nAppDomain: {APPDOMAIN}\n\n{MESSAGE}"; } }

        protected virtual string DefaultTraceTemplate { get { return "[{THREADID}] {EVENTTYPE}: {MESSAGE}"; } }

        /// <summary>
        /// For constructing an Email subject with basic process signature information for filtering Email messages.
        /// </summary>
        protected string SubjectTemplate
        {
            get 
            { 
                string s= Attributes["subjectTemplate"];
                if (String.IsNullOrEmpty(s))
                    return DefaultSubjectTemplate;

                return s;
            }
        }

        /// <summary>
        /// For constructing the whole Email body.
        /// </summary>
        protected string BodyTemplate
        {
            get
            {
                string s = Attributes["bodyTemplate"];
                if (String.IsNullOrEmpty(s))
                    return DefaultBodyTemplate;

                return s;
            }
        }

        /// <summary>
        /// For constructing a single trace message.
        /// </summary>
        protected string TraceTemplate
        {
            get
            {
                string s = Attributes["traceTemplate"];
                if (String.IsNullOrEmpty(s))
                    return DefaultTraceTemplate;

                return s;
            }
        }

        static string[] supportedAttributes = new string[] { "maxConnections", "subjectTemplate", "bodyTemplate", "traceTemplate" };

        protected override string[] GetSupportedAttributes()
        {
            return supportedAttributes;
        }

        Abstractions.ISmtpEmailHelper smtpEmailHelper;

        static object smtpEmailHelperLockForInit = new object();
   
        protected virtual Abstractions.ISmtpEmailHelper SmtpEmailHelper
        {
            get
            {
                lock (smtpEmailHelperLockForInit)//use Lazy<T> in .net 4.
                {
                    if (smtpEmailHelper == null)
                    {
                        smtpEmailHelper = new SmtpEmailWriterAsync(MaxConnections);//the listener is staying forever generally, no need to care about CA2000.
                    }
                }

                return smtpEmailHelper;
            }
        }



        protected void SendEmail(string subject, string body, string recipient)
        {
            SmtpEmailHelper.Send(subject, body, recipient, FromAddress);
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


    }
}
