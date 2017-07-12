[Home](../ReadMe.md) | [Index](Index.md) | [Examples](Examples.md) | [Guidance](Guidance.md) | [FAQ](FAQ.md) | [Listeners](Listeners.md) | [Filters](Filters.md) | [Extensions](Extensions.md)

# Getting Started - The Basics

The simplest possible example using the ubiquitous "Hello World":

**Hello.cs**
```c#
#define TRACE
using System.Diagnostics;

class Program {
  static TraceSource _trace = new TraceSource("Hello");

  public static void Main() {
    _trace.TraceInformation("Hello World!");
  }
}
```

**Hello.exe.config**
```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <system.diagnostics>
    <sources>
      <source name="Hello" switchValue="All">
        <listeners>
          <clear />
          <add name="console" 
            type="System.Diagnostics.ConsoleTraceListener" />
        </listeners>
      </source>
    </sources>
  </system.diagnostics>
</configuration>
```

Compile with `csc Hello.cs` (in .NET 4.0 you may need `csc Hello.cs /r:System.dll`), and run, you should get the following output:

```powershell
PS C:\Essential.Diagnostics\Examples> csc.exe Hello.cs
PS C:\Essential.Diagnostics\Examples> .\Hello.exe
Hello Information: 0 : Hello World!
```

Note that instead of `#define TRACE` you would usually compile as `csc Hello.cs /d:TRACE`, which is turned on by default in Visual Studio.

A simple "Hello World" isn’t however very useful for showing the different capabilities of logging, so the next page will walk you through a [logging primer](Logging-Primer.md) to introduce the range of features available in [System.Diagnostics](http://msdn.microsoft.com/en-us/library/system.diagnostics.aspx) and the Essential.Diagnostics extensions.

**Next: [Logging Primer](Logging-Primer.md)**
