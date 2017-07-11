# Logging Primer

A simple ["Hello" example](Getting-Started) isn’t however very useful for showing the different capabilities of logging, so we will use a slightly more complicated example. First of all, here is the program we will use both without and then with simple logging.

* [Hello World with no logging](#HelloNoLogging)
* [Hello Logging](Hello-Logging)

Once the program has had logging statements added, any of the trace listeners and filters can be configured to send the trace output to a wide variety of destinations.

* [Service Trace Viewer](Service-Trace-Viewer)
* [Windows Event Log](Windows-Event-Log)

## Extending System.Diagnostics

The best thing is – all of the logging examples above can be done right now – they are already part of the .NET Framework that you are using – and they are designed to be extended.

So, as well as teaching you about what you already have, this project provides some extensions to the features already available in .NET – for example a trace listener that logs to a database or writes to the console in color. These extensions are clearly marked with the extension symbol where they appear in the documentation.

For example, here are examples of some of the extended listeners provided.

* [Hello Color](Hello-Color) ![EX](Logging Primer_http://www.codeplex.com/download?ProjectName=essentialdiagnostics&DownloadId=150104)
* [Hello Database](Hello-Database) ![EX](Logging Primer_http://www.codeplex.com/download?ProjectName=essentialdiagnostics&DownloadId=150104)

{anchor:HelloNoLogging}
# Hello World (no logging)

This version of “Hello World” involves a bunch of Worker classes that Poke() each other to say “Hello World”. Sometimes they get sick of being poked.

**HelloWorld.cs**
{code:c#}
using System;
using System.Collections.ObjectModel;
using System.Threading;

// Program class
class Program {
  public static Random Random = new Random();
  public static Collection<Worker> Workers = new Collection<Worker>();

  public static void Main(string[]() args) {
    int numberOfWorkers = Program.Random.Next(2, 4);
    for(int i = 1; i <= numberOfWorkers; i++ ) {
      Worker worker = new Worker() { Id = string.Format("Worker {0}", i) };
      Workers.Add(worker);
    }
    StartWorkers();
    foreach(Worker worker in Workers) {
      worker.FinishedEvent.WaitOne();
    }
  }

  static void StartWorkers() {
    foreach(Worker worker in Workers) {
      ThreadPool.QueueUserWorkItem(worker.Work);
    }
  }
}

// Worker class
class Worker {
  int _count;
  public AutoResetEvent FinishedEvent = new AutoResetEvent(false);
  public string Id;

  public void Poke() {
    Thread.Sleep(Program.Random.Next(500));
    _count++;
    if( _count < 4 )
      Console.WriteLine("Hello World {1}", Id, _count);
    else if( _count < 6 ) {
      Console.WriteLine("Hi", Id);
    }
  }
  
  public void Work(object state) {
    int numberOfPokes = Program.Random.Next(3, 7);
    for(int i = 1; i < numberOfPokes; i++) {
      Thread.Sleep(Program.Random.Next(500));
      int index = Program.Random.Next(Program.Workers.Count);
      Program.Workers[index](index).Poke();
    }
    FinishedEvent.Set();
  }
}
{code:c#}

Compiling and running this program may produce the following:

{code:powershell}
PS C:\Microsoft.Diagnostics\Examples> .\HelloWorld.exe
Hello World 1
Hello World 2
Hello World 1
Hello World 3
Hi
Hello World 2
Hello World 1
Hi
Hello World 3
Hello World 2
{code:powershell}

Lots of “Hello World”, but a bit difficult to tell which bit of code did what.

>{**Next: [Hello Logging](Hello-Logging)**}>
