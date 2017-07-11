# Hello Logging

Let’s introduce some logging into our [application](Logging-Primer), and we can see what it can do.

> This examples shows manually configuring the app.config file, however the easiest way to add this, right now, to your existing project is using [NuGet](http://nuget.org) via `Install-Package System.Diagnostics.Config`.

Note that this code is intended to demonstrate logging and does not necessarily follow best practices in order to keep things simple (e.g. normally you would use properties instead of public fields). Also there is a lot of logging code because even though the example is short it tries to include many of the logging features.

**HelloLogging.cs**
```c#
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;

// Program class
class Program {
  static TraceSource _trace = new TraceSource("HelloProgram");
  public static Random Random = new Random();
  public static Collection<Worker> Workers = new Collection<Worker>();

  public static void Main(string[]() args) {
    // Trace start
    Trace.CorrelationManager.ActivityId = Guid.NewGuid();
    Trace.CorrelationManager.StartLogicalOperation("Main");
    _trace.TraceEvent(TraceEventType.Start, 1000, "Program start.");
    // Run program
    int numberOfWorkers = Program.Random.Next(2, 4);
    _trace.TraceEvent(TraceEventType.Information, 2000, "Creating {0} workers", numberOfWorkers);
    for(int i = 1; i <= numberOfWorkers; i++ ) {
      Worker worker = new Worker() { Id = string.Format("Worker {0}", i) };
      Workers.Add(worker);
    }
    StartWorkers();
    foreach(Worker worker in Workers) {
      worker.FinishedEvent.WaitOne();
    }
    // Trace stop
    _trace.TraceEvent(TraceEventType.Stop, 8000, "Program stop.");
    Trace.CorrelationManager.StopLogicalOperation();
    _trace.Flush();
  }

  static void StartWorkers() {
    // Trace transfer in
    Guid newActivity = Guid.NewGuid();
    _trace.TraceTransfer(6011, "Transferred to Start", newActivity);
    Guid oldActivity = Trace.CorrelationManager.ActivityId;
    Trace.CorrelationManager.ActivityId = newActivity;
    _trace.TraceEvent(TraceEventType.Start, 1010, "Starting workers.");
    // Do work
    foreach(Worker worker in Workers) {
      ThreadPool.QueueUserWorkItem(worker.Work);
    }
    // Trace transfer back
    _trace.TraceTransfer(6012, "Transferred back", oldActivity);
    _trace.TraceEvent(TraceEventType.Stop, 8010, "Finished starting.");
    Trace.CorrelationManager.ActivityId = oldActivity;
  }
}

// Worker class
class Worker {
  int _count;
  static TraceSource _trace = new TraceSource("HelloWorker");
  public AutoResetEvent FinishedEvent = new AutoResetEvent(false);
  public string Id;

  public void Poke() {
    // Trace - mark with logical operation
    Trace.CorrelationManager.StartLogicalOperation(string.Format("Poked:{0}", Id));
    _trace.TraceEvent(TraceEventType.Verbose, 0, "Worker {0} was poked", Id);
    // Work
    Thread.Sleep(Program.Random.Next(500));
    _count++;
    if( _count < 4 )
        Console.WriteLine("Hello World {1}", Id, _count);
    else if( _count < 6 ) {
        Console.WriteLine("Hi", Id);
        _trace.TraceEvent(TraceEventType.Warning, 4500, "Worker {0} getting annoyed", Id);
    } else {
        _trace.TraceEvent(TraceEventType.Error, 5500, "Worker {0} - too many pokes", Id);
    }
    // Trace - end logical operation
    Trace.CorrelationManager.StopLogicalOperation();
  }
  
  public void Work(object state) {
    // Trace transfer to thread
    Guid newActivity = Guid.NewGuid();
    _trace.TraceTransfer(6501, "Transfered to worker", newActivity);
    Trace.CorrelationManager.ActivityId = newActivity;
    Trace.CorrelationManager.StartLogicalOperation(string.Format("Worker:{0}", Id));
    _trace.TraceEvent(TraceEventType.Start, 1500, "Worker {0} start.", Id);
    // Do work
    int numberOfPokes = Program.Random.Next(3, 6);
    _trace.TraceEvent(TraceEventType.Information, 2500, "Worker {0} will poke {1} times", Id, numberOfPokes);
    for(int i = 1; i <= numberOfPokes; i++) {
      Thread.Sleep(Program.Random.Next(500));
      int index = Program.Random.Next(Program.Workers.Count);
      _trace.TraceEvent(TraceEventType.Verbose, 0, "Worker {0} poking {1}", Id, Program.Workers[index](index).Id);
      Program.Workers[index](index).Poke();
    }
    FinishedEvent.Set();
    // Trace stop (no transfer)
    _trace.TraceEvent(TraceEventType.Stop, 8500, "Worker stop.");
    Trace.CorrelationManager.StopLogicalOperation();
  }
}
```

For logging you also need a configuration file:

**HelloLogging.exe.config**
```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <system.diagnostics>
    <sharedListeners>
      <add name="console"
         type="System.Diagnostics.ConsoleTraceListener" />
    </sharedListeners>
    <sources>
      <source name="HelloProgram" switchValue="Information,ActivityTracing">
        <listeners>
          <clear />
          <add name="console" />
        </listeners>
      </source>
      <source name="HelloWorker" switchValue="All">
        <listeners>
          <clear />
          <add name="console" />
        </listeners>
      </source>
    </sources>
  </system.diagnostics>
</configuration>
```

Now, compile with `csc HelloLogging.cs /d:TRACE` – don’t forget the TRACE flag, or you won’t get any logging – and run.

With the basic console logger the information may not add much clarity, however the level of detail can be controlled by simply altering the configuration file without having to recompile.

**Next: [Service Trace Viewer](Service-Trace-Viewer)**
