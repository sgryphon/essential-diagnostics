Essential.Diagnostics - Performance
===================================

Performance comparison of logging frameworks -- System.Diagnostics, log4net, NLog

Instructions
------------

1. Build the application.
2. Run the application from the command line.
3. Examine the output.

Remarks
-------

These tests are intended to measure the overhead of the different frameworks, i.e. the overhead of adding
trace messages and then ignoring or filtering them out. All the frameworks allow you to selectively
turn on sections of messages to control the volume captured.

Before running any tests, all frameworks are warmed up by logging 10 messages each.

The program then first runs a test with no tracing framework, to get a baseline time for the overhead of the
test program itself. Different scenarios are then tested for each framework, with an increasing number of
messages captured (per million trace statements).

The scenarios tested for each framework are:

* All messages ignored, i.e. tracing turned off
* One source turned on at Warning level, capturing 16 messages per million trace statements.
* All sources turned on at Warning level, capturing 245 messages per million trace statements.
* One source turned to full (all messages), with others at Warning level, capturing 3,907 messages per 
  million trace statements.

The different frameworks compared:

* System.Diagnostics, using built-in FileLogTraceListener.
* System.Diagnostics, using built-in FileLogTraceListener + DefaultTraceListener (i.e. if you don't clear the listeners)
* System.Diagnostics, using Essential.Diagnostics RollingFileTraceListener.
* System.Diagnostics, using built-in EventSchemaTraceListener.
* log4net
* NLog
* Enterprise Library Logging Application Block

Note
----

With System.Diagnostics to ignore all messages for a source set switchValue="Off" or simply leave out the source 
altogether. Having a source with no listeners has more overhead, but isn't too bad so long as you actually clear
the list of listeners.

If you don't clear the list, you will get all messages written to the default listener, which will severely impact
the performance of your application. i.e. the worst thing you can do is:

      <!-- Bad example: will severely impact performance -->
      <source name="MySource" switchValue="All">
      </source>

