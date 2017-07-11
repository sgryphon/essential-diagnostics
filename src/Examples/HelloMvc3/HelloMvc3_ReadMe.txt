Essential.Diagnostics - Hello Mvc3
==================================

-------------------------------------------------------------------------------
NOTE:

If you build and get the error "Package restore is disabled by default" it is
because the HelloMvc3 project has additional dependencies. 

To allow HelloMvc3 to build, you can go to Tools > Options, then in Package 
Manager enable "Allow NuGet to download missing packages during build".
-------------------------------------------------------------------------------

Shows how System.Diagnostics tracing can be integrated with ASP.NET, by forwarding events to page tracing.

Also shows how to use the built in FileLogTraceListener with web applications by setting a custom location, 
plus use of the {AppData} token with the extended RollingFileTraceListener.

Note that the project is configured to use IIS Express, included with Visual Studio Express 2012 for Web.

For the Framework FileLogTraceListener to work you need to ensure a directory "C:\Temp\Logs" exists,
or change the configuration.

Instructions
------------

1. Examine the configuration for WebPageTraceListener in the App.config file.
2. Build and run the web application; the default home page index will load.
3. Click step 2, to navigate to the Log page (this will write log messages).
4. Click step 3, to open the trace.axd page.
5. Click View Details next to the last request for /Home/Log.
6. Examine the messages output in the Trace Information section.

The trace source name is used as the category, and the event ID is prefixed to the message.

The RollingFileTraceListener, which writes logs into the App_Data folder, also shows usage
of the ASP.NET format tokens for AppData, RequestPath, RequestUrl and UserHostAddress.

