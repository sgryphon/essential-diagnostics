# Service Trace Viewer

A good tool for viewing log files is the Service Trace Viewer from the .NET SDK (this can normally be found in the Start > Programs menu under the Microsoft Windows SDK > Tools folder). 

The best format for this tool is the XmlWriterTraceListener, although it does understand the other XML formats to a limited degree.

Use the following config file, along with the [Hello Logging](Hello-Logging) sample program. You do not need to recompile the sample program, using the Service Trace Viewer only requires changes to the application config file.

**HelloLogging.exe.config**
{code:xml}
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <system.diagnostics>
    <sharedListeners>
      <add name="xml"
         type="System.Diagnostics.XmlWriterTraceListener"
         initializeData="HelloLogging.svclog" />
    </sharedListeners>
    <sources>
      <source name="HelloProgram" switchValue="Information,ActivityTracing">
        <listeners>
          <clear />
          <add name="xml" />
        </listeners>
      </source>
      <source name="HelloWorker" switchValue="All">
        <listeners>
          <clear />
          <add name="xml" />
        </listeners>
      </source>
    </sources>
  </system.diagnostics>
</configuration>
{code:xml}

You should get an XML file "HelloLogging.svclog" created in the example directory – if you don’t, check that the console logging is working (i.e. make sure you compiled with the TRACE flag).

To see your log, run the Service Trace Viewer tool (SvcTraceViewer.exe from the .NET SDK), and open up the log file, you should see log details similar to the following:

![Trace Viewer Example](Service Trace Viewer_TraceViewerExample800.png)

The trace viewer provides a graphical overview of how the activities relate to each other and allow you to easily narrow in on any problems in the code.

Note that the Service Trace Viewer can also correlate logs across multiple processes and services, providing an end-to-end view of your application. For an example, see the MSDN documentation [http://msdn.microsoft.com/en-us/library/aa751795.aspx](http://msdn.microsoft.com/en-us/library/aa751795.aspx).

>{**Next: [Windows Event Log](Windows-Event-Log)**}>