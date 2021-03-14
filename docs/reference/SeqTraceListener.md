# SeqTraceListener Class

Trace listener that writes to a Writes to a [Seq](https://getseq.net/) server.

## Installing

Install via NuGet:

* PM> **Install-Package [Essential.Diagnostics.SeqTraceListener](http://www.nuget.org/packages/Essential.Diagnostics.SeqTraceListener)**

## Remarks

This listener writes to a Seq server, including additional properties as specified in the config file.

For performance, the listener will queue messages and send to the Seq server over HTTP in batches of 100 (default) messages, with a timeout of 1,000 (default) milliseconds; if there is less than a full batch when the timeout is reached, the batch is sent anyway, so low volume messages will be sent every one second.

Both these settings are configurable.

In addition, the first message logged (or the first message after a completed timeout), is sent immediately in a batch (usually a single message). This first message lets the Seq server know the application is up an running without having to wait 1 second for the first batch.

When the HTTP post to send the batch fails, the component includes a retry algorithm. There is a back off delay, based on the batch timeout, that doubles for each retry. 

The default maximium number of retries is 10 (about 17 minutes based on the default settings and algorithm). This should be enough to cover small outages and network interruptions. 

Additional message that arrive during retry are queued, and then sent (in batches) once the connection is back up. The maximum queue size can also be specified (default 1,000), after which messages are instead dropped (to prevent filling up memory).

If the maximum number of retries is reached, then the failing batch is dropped; this helps overcome if the batch contains a poison message or is the cause of the HTTP failure (rather than it being a network issue).

## Config Attributes

| Attribute | Description |
| --------- | ----------- |
| initializeData | URL of the Seq server (e.g. local development is usually http://localhost:5341). |
| traceOutputOptions | If specified, sent as additional properties (can be specified either here, or in additionalProperties). |
| additionalProperties | Additional named properties to include in the Seq message. |
| apiKey | Your API key for the Seq server (for local development this can be empty). |
| batchSize | Number of messages per batch, default 100. A batch size of 0 will disable batching and each message is sent as a separate HTTP request, with no retries. |
| batchTimeout | Timeout after which incomplete batches are sent anyway, default 1,000 milliseconds. |
| individualSendIgnoreErrors | Gets or sets a flag whether to ignore errors when using individual send mode (batch size is 0). Use true to catch and ignore all exceptions when sending. Default is false. It has no effect if batch size is > 0. |
| maxQueueSize | Maximum number of messages to queue; once this limit is reached, messages are dropped. Default is 1,000. |
| maxRetries | Maximum number of retries where transmission to the Seq server fails, e.g. HTTP timeout. Retries have a backoff algorithm that doubles the wait time each attempt; the default is 10 retries, which works out at around 17 minutes. After the specified number of retries the batch is dropped (e.g. if it contains a poison message). |
| processDictionaryData | By default if the first data element is `IDictionary<string, object>` then it is treated as structured data and expanded into key-value pairs. Set to `false` to turn this behaviour off. |
| processDictionaryLogicalOperationStack | By default any members of the logical operation stack that are `IDictionary<string, object>` are treated as structured data and expanded into key-value pairs. Set to `false` to turn this behaviour off. |

## Example Config

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <system.diagnostics>
    <sharedListeners>
      <add name="seq" 
        type="Essential.Diagnostics.SeqTraceListener, Essential.Diagnostics.SeqTraceListener" 
        initializeData="http://localhost:5341" 
        additionalProperties="MachineName,ThreadId,ProcessId,LogicalOperationStack" 
        apiKey=" " />
    </sharedListeners>
    <sources>
      <source name="ExampleSource" switchValue="All">
        <listeners>
          <clear />
          <add name="seq" />
        </listeners>
      </source>
    </sources>
  </system.diagnostics>
</configuration>
```

## Example Output

Events are sent to the specified Seq server, where they can be queried, filtered, etc.

![SeqTraceListener example Seq output](../images/SeqTraceListener_SeqTraceListener800.png)

## Config Template

```xml
<add name="seq"
  type="Essential.Diagnostics.SeqTraceListener, Essential.Diagnostics.SeqTraceListener" 
  initializeData="Seq server URL"
  traceOutputOptions="CallStack,LogicalOperationStack,ProcessId,ThreadId,User"
  additionalProperties="CallStack,LogicalOperationStack,MachineName,PrincipalName,ProcessId,ThreadId,User"
  apiKey="your API key, or blank for localhost"
  batchSize="100|use 0 to disable"
  batchTimeout="00:00:01.00"
  individualSendIgnoreErrors="false"
  maxQueueSize="1000"
  maxRetries="10" 
  processDictionaryData="true"
  processDictionaryLogicalOperationStack="true"
/>
```
