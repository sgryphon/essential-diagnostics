Essential.Diagnostics - Hello Web
=================================

Shows how System.Diagnostics tracing can be integrated with ASP.NET, by forwarding events to page tracing.

Note that the project is configured to use IIS Express, which requires Visual Studio 2010 SP1 and IIS Express
to be installed, however it works equally well with the Visual Studio Development Server (set in the Web tab 
of the project properties).

Instructions
------------

1. Examing the configuration for WebPageTraceListener in the App.config file.
2. Build and run the web application; the default home page index will load.
3. Click step 2, to navigate to the Log page (this will write log messages).
4. Click step 3, to open the trace.axd page.
5. Click View Details next to the last request for /Home/Log.
6. Examine the messages output in the Trace Information section.

The trace source name is used as the category, and the event ID is prefixed to the message.
