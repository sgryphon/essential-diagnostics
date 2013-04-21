Essential.Diagnostics - Event Schema Log Retention Example
==========================================================

An example showing the different retention options of EventSchemaTraceListener.

See http://msdn.microsoft.com/en-us/library/system.diagnostics.tracelogretentionoption.aspx

Instructions
------------

1. Build the application.
2. Run the application from the command line.
3. Examine the different output in the EventA - EventD log files.

As well as a different logRetentionOption, the listeners also have different 
traceOutputOptions, for example EventsC is the only file that actually includes
the date and time of the traces!  You will almost always want to have at least
traceOutputOptions="DateTime,ProcessId".

Listener Details
----------------

EventsA - SingleFileUnboundedSize: One file with no maximum file size restriction.

EventsB - SingleFileBoundedSize: One file with a maximum file size that is determined 
by the maximumFileSize attribute.

EventsC - UnlimitedSequentialFiles: An unlimited number of sequential files, each with 
a maximum file size that is determined by the maximumFileSize attribute. There is no 
limit to the number of files.

EventsD - LimitedCircularFiles: A finite number of sequential files, each with a 
maximum file size. When the maximumFileSize attribute value is reached, writing 
starts in a new file with an incremented integer suffix. When the maximumNumberOfFiles 
attribute value is reached, the first file is cleared and overwritten. Files are then 
incrementally overwritten in a circular manner.

There is also a LimitedSequentialFiles option.
