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


