Essential.Diagnostics - Monitor Config Example
==============================================

An example using TraceConfigurationMonitor to monitor your application config file
for changes and refresh the diagnostics configuration as needed.

Instructions
------------

1. Build the application.
2. Run the application from the command line.
3. While it is running, modify the MonitorConfig.exe.config file to see the effects.

Notes
-----

If you try to add a filter to an existing listener (that doesn't already have a filter)
you will cause a NullReferenceException.
A work around is to also change the name of the listener (effectively creating a new one).
