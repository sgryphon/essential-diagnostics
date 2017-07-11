Essential.Diagnostics.RollingXmlTraceListener
=============================================

Copyright 2017 Sly Gryphon. This library distributed under the 
Microsoft Public License (Ms-PL).

http://essentialdiagnostics.codeplex.com/

Using and extending System.Diagnostics trace logging. 

Rolling file version of the XML trace listener extension for 
System.Diagnostics, along with required config sections. Writes 
E2ETraceEvent XML fragments to a text file, rolling to a new 
file based on a filename template (usually including the date). 
The E2ETraceEvent XML fragment format can be read by the Service 
Trace Viewer tool.

Other Essential.Diagnostics packages extend the .NET Framework 
System.Diagnostics trace logging with additional trace listeners. 
Included are colored console (that allows custom formats), 
SQL database (including a tool to create tables), rolling file 
(with custom formats), rolling XML, email (per trace or batched), 
and an in-memory trace listener.

The Essential.Diagnostics project also publishes a fluent client 
library that includes easy encapsulation of activity and logical 
operation scopes, as well as abstractions and templated classes 
for a simpler logging API and support for dependency injection 
frameworks.

See the project site on CodePlex for examples and documentation.
