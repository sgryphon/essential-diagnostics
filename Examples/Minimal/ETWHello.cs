using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;

class Program {
  public static void Main() {
    using (var eventListener = new ConsoleEventListener())
    {
      var eventSource = new HelloEventSource();
      eventListener.EnableEvents(eventSource, EventLevel.Verbose);

      eventSource.Hello("Hello World!");
    }
  }
}

class HelloEventSource : EventSource
{
    [Event(1, Message = "{0}")]
    public void Hello(string message)
    {
        WriteEvent(1, message);
    }
}

class ConsoleEventListener : EventListener
{
  protected override void OnEventWritten(EventWrittenEventArgs eventData)
  {
    Console.WriteLine("{0}, {1}, {2}, {3}/{4}/{5}/{6}, {7}",
      eventData.EventId,
      eventData.EventSource,
      eventData.Level,
      eventData.Keywords,
      eventData.Opcode,
      eventData.Task,
      eventData.Version,
      string.Format(eventData.Message, eventData.Payload.ToArray()));
  }
}
// Compile with: PS> & "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" ETWHello.cs
