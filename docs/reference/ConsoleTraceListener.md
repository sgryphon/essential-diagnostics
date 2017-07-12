# ConsoleTraceListener Class

Writes to the console output or error stream.

## Remarks

Each event includes the Source, EventType, Id and Message. Depending on 
traceOutputOptions, additional lines may be written

For an alternative that uses color to highlight event types and allows a
customised output format see [ColoredConsoleTraceListener](ColoredConsoleTraceListener.md).
	
## Config Attributes

| Attribute | Description |
| --------- | ----------- |
| initalizeData | If false (default) the listener writes to the console output stream; if true the listener writes to the console error stream instead. |
| traceOutputOptions | Are written on separate lines after each trace output. |

## Example Config

**Note:** You may need to change the version number of Visual Basic based on the .NET version you are using.

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <system.diagnostics>
    <sharedListeners>
      <add name="console"
         type="System.Diagnostics.ConsoleTraceListener" />
    </sharedListeners>
    <sources>
      <source name="ExampleSource" switchValue="All">
        <listeners>
          <clear />
          <add name="console" />
        </listeners>
      </source>
    </sources>
  </system.diagnostics>
</configuration>
```

## Example Output

From the [Getting Started](..\Getting-Started.md) example:

```powershell
PS C:\Essential.Diagnostics\Examples> .\Hello.exe
Hello Information: 0 : Hello World!
```

You can also download the Complete package with examples and see the Console subfolder in the HelloLogging example.

## Config Template

```xml
<add name="console"
  type="System.Diagnostics.ConsoleTraceListener"
  initalizeData="false|true"
  traceOutputOptions="ProcessId,LogicalOperationStack,ThreadId,DateTime,Timestamp,Callstack"
  />
```
