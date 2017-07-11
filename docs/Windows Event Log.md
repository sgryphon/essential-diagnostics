# Windows Event Log

The Windows Event Log is an important tool for managing Windows based computers, particularly for Windows Services, web applications, and other server based applications that do not have a user interface.

When writing to the Windows Event Log you probably don't want to write every message, particularly not a large number of verbose messages. On the other hand, you probably want to always report every warning, error, or higher level message.

If you aren't writing directly to the Windows Event Log, then the EventLogTraceListener can be used to forward appropriate trace events to the Windows Event Log. For an example using the [Hello Logging](Hello-Logging) sample program, use the following configuration:

**HelloLogging.exe.config**
{code:xml}
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <system.diagnostics>
    <sharedListeners>
      <add name="console"
         type="System.Diagnostics.ConsoleTraceListener" />
      <add name="eventlog"
         type="System.Diagnostics.EventLogTraceListener">
        <filter type="System.Diagnostics.EventTypeFilter"
                initializeData="Warning" />
      </add>
    </sharedListeners>
    <sources>
      <source name="HelloProgram" switchValue="Information,ActivityTracing">
        <listeners>
          <clear />
          <add name="console" />
          <add name="eventlog" />
        </listeners>
      </source>
      <source name="HelloWorker" switchValue="All">
        <listeners>
          <clear />
          <add name="console" />
          <add name="eventlog" />
        </listeners>
      </source>
    </sources>
  </system.diagnostics>
</configuration>
{code:xml}

When the program is run, messages based on the switchValue settings for individual sources will be sent to both trace listeners. The console trace listener will output all messages, however the event log trace listener will only record warning and above messages.

>{**Next: [Hello Color](Hello-Color)**}>