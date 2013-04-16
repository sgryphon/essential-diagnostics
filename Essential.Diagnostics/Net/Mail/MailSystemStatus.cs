    using System;
using System.Collections.Generic;
using System.Text;

namespace Essential.Net.Mail
{
    internal enum MailSystemStatus
    {
        Ok,
        EmptyConnectionPool,
        TemporaryProblem, //then worth of retry
        Critical //then the Email message queue should stop accepting new messages.
    };
}
