# TraceConfigurationMonitor Class 

Monitors the application configuration file and refreshes the tracing configuration when the file is changed. 


## Installing

Install via NuGet (this package is automatically installed when you install one of the trace listeners):

* PM> **Install-Package [Essential.Diagnostics.Core](http://www.nuget.org/packages/Essential.Diagnostics.Core)**

## Remarks

The TraceConfigurationMonitor sets up a FileSystemWatcher that is watching the application config file. When it detects a change it calls Trace.Refresh() to reload the diagnostics configuration. Note that you don't need this class for web applications, as they restart when web.config is editted.

**Note:** If you try to add a filter to an existing listener (that doesn't already have a filter) you will cause a NullReferenceException. A work around is to also change the name of the listener (effectively creating a new one), and update the references in any trace sources.

## Example

In your application, create and start the configuration monitor. For example in a Windows Service you could use the following:

{code:c#}
public partial class ExampleService : ServiceBase
{
    TraceConfigurationMonitor configMonitor;

    protected override void OnStart(string[]() args)
    {
        configMonitor = new TraceConfigurationMonitor();
        configMonitor.Start();

        // ... run the service ...
    }
	
    // ...
}
{code:c#}

Now if you change the application config file (while it is running), the diagnostics configuration is reloaded.

When your application ends you can clean up by stopping and disposing the configuration monitor:

{code:c#}
    protected override void OnStop()
    {
        configMonitor.Stop();
        configMonitor.Dispose();
    }
{code:c#}
