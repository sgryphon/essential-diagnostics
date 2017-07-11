# EventLogTraceListener Class

Writes to the Windows Event Log.

## Remarks

Events are logged with the event Id (maximum 65535), and with a severity based on
the EventType (Critial and Error are logged as errors, Warning is logged as a 
warning, all others as information).

The message, or non-empty formatted message with arguments inserted is written as
the event entry data. If TraceData() is used, then the data items are concatenated 
with ", " and then written as the event entry data.

If, however, a null or empty format message is used then the format arguments
are used directly as the event entry data - this allows them to be used as 
multiple replacement strings for an event log entry ID specified in a registered
DLL. (See EventLog.WriteEvent() for details.)

Usually you will want to filter events sent to the Windows Event Log to only
include those above a certain level / event type.

Note: You must include an installer (that is run by an administrator)
to create the event log source (specified in initializeData), otherwise events 
cannot be written to it by normal process accounts. This limits the values that
can be used in initializeData to created event log sources.
	
## Config Attributes

|| Attribute || Description ||
| initalizeData | Name of the event log source. |
| traceOutputOptions | Ignored. |

## Example Config

{code:xml}
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <system.diagnostics>
    <sharedListeners>
      <add name="eventlog"
        type="System.Diagnostics.EventLogTraceListener"
        initializeData="Diagnostics.Sample">
        <filter type="System.Diagnostics.EventTypeFilter"
                initializeData="Warning" />
      </add>
    </sharedListeners>
    <sources>
      <source name="ExampleSource" switchValue="All">
        <listeners>
          <clear />
          <add name="eventlog" />
        </listeners>
      </source>
    </sources>
  </system.diagnostics>
</configuration>
{code:xml}

## Example Output

To see an example download the Complete package with examples and see the EventLog subfolder in the HelloLogging example.

## Config Template

{code:xml}
<add name="eventlog"
  type="System.Diagnostics.EventLogTraceListener"
  initalizeData=""
  />
{code:xml}
