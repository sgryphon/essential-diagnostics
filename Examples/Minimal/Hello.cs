#define TRACE
using System.Diagnostics;

class Program {
  public static void Main() {
    var trace = new TraceSource("Hello");
    trace.TraceInformation("Hello World!");
  }
}
// Compile with: PS> & "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" Hello.cs