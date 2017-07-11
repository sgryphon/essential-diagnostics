# Hello Color

The [ColoredConsoleTraceListener](ColoredConsoleTraceListener) is a replacement for the standard [ConsoleTraceListener](ConsoleTraceListener) that colorizes the output according to the [TraceEventType](TraceEventType). The listener also has allows trace details to be output according to a user-supplied template.

By default errors are output in red, warnings in yellow and information messages in white. The default template matches the format output by the standard ConsoleTraceListener.

Use the following config file, along with the [Hello Logging](Hello-Logging) sample program. You do not need to recompile the sample program.

|| This example requires you to [download](_releases) the Essential.Diagnostics extensions and add the DLL to the same directory as the example, however you do not need to recompile your program (just change the config file). For a real project however the simplest way to add Essential.Diagnostics is with [Nuget](http://nuget.org) via "nuget Essential.Diagnostics.Config". ||

**HelloLogging.exe.config**
{code:xml}
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <system.diagnostics>
    <sharedListeners>
      <add name="color"
         type="Essential.Diagnostics.ColoredConsoleTraceListener, Essential.Diagnostics"
         format="{DateTime} {EventType}: {Message}"
         activityTracingColor="DarkGreen"
         transferColor="Blue" />
    </sharedListeners>
    <sources>
      <source name="HelloProgram" switchValue="Information,ActivityTracing">
        <listeners>
          <clear />
          <add name="color" />
        </listeners>
      </source>
      <source name="HelloWorker" switchValue="All">
        <listeners>
          <clear />
          <add name="color" />
        </listeners>
      </source>
    </sources>
  </system.diagnostics>
</configuration>
{code:xml}

When you run the program, you should get output similar to the following.
 
![Colored Console Example](Hello Color_ColoredConsoleExample800.png)

Messages are colored and output according to the custom template.