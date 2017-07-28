Essential.Diagnostics.Core
==========================

Copyright 2017 Sly Gryphon. This library distributed under the 
Microsoft Reciprocal License (Ms-RL).

https://github.com/sgryphon/essential-diagnostics/

Using and extending System.Diagnostics trace logging. 

Core library for Essential.Diagnostics, with a common base class for 
trace listeners. It also includes a generic formatter, a generic 
expression-based trace filter, a file watcher for configuration 
changes, and support for structured data.

Other Essential.Diagnostics packages extend the .NET Framework 
System.Diagnostics trace logging with additional trace listeners. 
Included are colored console (that allows custom formats), 
SQL database (including a tool to create tables), rolling file 
(with custom formats), rolling XML, email (per trace or batched), 
and an in-memory trace listener.

The Essential.Diagnostics project also publishes structured data
(semantic) tracing extensions and a fluent client library that 
includes easy encapsulation of activity and logical operation scopes, 
as well as abstractions and templated classes for a simpler 
logging API and support for dependency injection frameworks.

See the project site on GitHub for examples and documentation.
