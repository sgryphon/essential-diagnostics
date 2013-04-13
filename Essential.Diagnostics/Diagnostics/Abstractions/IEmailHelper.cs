using System;
using System.Collections.Generic;
using System.Text;

namespace Essential.Diagnostics.Abstractions
{
    public interface ISmtpEmailHelper
    {
        /// <summary>
        /// Send Email message. And the implementation could be sending asynchronously.
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="recipient"></param>
        /// <param name="from"></param>
        void Send(string subject, string body, string recipient, string from);

        /// <summary>
        /// The helper is busy sending Email message.
        /// </summary>
        bool Busy { get; }
    }
}
