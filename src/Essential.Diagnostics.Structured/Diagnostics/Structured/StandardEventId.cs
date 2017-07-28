using System;

namespace Essential.Diagnostics.Structured
{
    /// <summary>
    /// Enum of standard general event IDs to use with StructuredTrace.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Note: It is recommended that each applications
    /// use their own custom event ID enumeration.
    /// </para>
    /// <para>
    /// The structure of the event IDs are based on a structure similar to the theory of 
    /// response codes used for Internet systems such as SMTP and HTTP.
    /// </para>
    /// <para>
    /// In this case, the event IDs are based on a 4 digit system.
    /// </para>
    /// <para>
    /// The first digit is the class of the event: 
    /// 1 for startup code, 2 for events that have occurred (completed), 
    /// 3 for actions the program initiates, 4 for warnings, 5 for errors, 8 for shutdown or
    /// end events, and 9 for unknown or critical issues.
    /// </para>
    /// <para>
    /// The class is similar to the event level (e.g. events starting with 5 will usually
    /// be logged at the Error level).
    /// </para>
    /// <para>
    /// The second digit is the area of the program: 0 for configuration or syntax, 1 for
    /// system level, 2 for connnections, 3 for security and authentication, and 9 for 
    /// an unknown area, such as an unhandled general exception.
    /// </para>
    /// <para>
    /// Applications can use the second digit 4-8 for the major subsystems, which will
    /// vary between applications.
    /// </para>
    /// </remarks>
    public enum StandardEventId
    {
        ConfigurationStart = 1000,
        SystemStart = 1100,
        ConnectionStart = 1200,

        SystemEvent = 2100,
        ConnectionEvent = 2200,
        AuthenticationSuccess = 2300,

        ConfigurationAction = 3000,
        SystemAction = 3100,
        ConnectionAction = 3200,

        ConfigurationWarning = 4000,
        SystemWarning = 4100,
        ConnectionWarning = 4200,
        AuthenticationFailure = 4300,

        ConfigurationError = 5000,
        SystemError = 5100,
        ConnectionError = 5200,
        AuthenticationError = 5300,
        UnknownError = 5900,

        SystemStop = 8100,
        ConnectionStop = 8200,

        ConfigurationCriticalError = 9000,
        SystemCriticalError = 9100,
        ConnectionCriticalError = 9200,
        AuthenticationCriticalError = 9300,
        UnknownCriticalError = 9900,
    }
}
