Essential.Diagnostics.SqlDatabaseTraceListener
==============================================

Copyright 2017 Sly Gryphon. This library distributed under the 
Microsoft Public License (Ms-PL).

http://essentialdiagnostics.codeplex.com/

Using and extending System.Diagnostics trace logging. 

SQL database trace listener extension for System.Diagnostics, 
along with required config sections. Trace listener that writes 
to the database table and connection specified (format can
be customised).

To create the required table, there is an included 
diagnostics_regsql.exe utility in the package tools directory. 
Use ".\diagnostics_regsql.exe -?" to display instructions

The utility runs the InstallTrace.sql script, which can also be run 
manually. It creates a table diagnostics_Trace, by default in
a database named diagnosticsdb, as well as a stored procedure
diagnostics_Trace_AddEntry for writing to the table and a 
role diagnostics_Trace_Writer with the required permissions.

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
