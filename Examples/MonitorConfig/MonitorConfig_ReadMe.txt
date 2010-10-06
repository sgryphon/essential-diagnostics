Essential.Diagnostics - Monitor Config Example
==============================================

Instructions
------------

1. Build the application.
2. Run the application from the command line.
3. While it is running, modify the MonitorConfig.exe.config file to see the effects.

Notes
-----

If you try to add a filter to an existing listener you will cause a NullReferenceException.
A work around is to also change the name of the listener (effectively creating a new one).
