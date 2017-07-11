# FileLogTraceListener Class

Writes to a rolling text file.

## Remarks

A new file is used when the maxFileSize is reached, as well as a daily or weekly
basis as specified by logFileCreationSchedule. 

Each file has a name in the format "<directory>\<baseFileName>(-<yyyy-MM-dd>)(-<seq>).log",
with the local date included for daily and weekly rotation, and a sequence number
appended if the file already exists.

Directory is determined by the location enumeration, and customLocation (if using
a custom location).

Trace information is written to the file with Source, EventType, Id, and Message, 
separated by the specified delimiter.

If TraceData() is used, instead of TraceEvent(), then all Data items are written,
each separated by the delimiter, instead of Message (the number of delimiters will
vary based on the number of data items).

Depending on traceOutputOptions the Callstack, LogicalOperationStack, 
DateTime (UTC), ProcessId, ThreadId, and Timestamp are then written, separated
by the delimiter. If the includeHostName option is set, then a final delimiter and
the MachineName are also written.

For an alternative that allows a customised output format and alternative creation
frequencies, see [RollingFileTraceListener](RollingFileTraceListener).
	
## Config Attributes

|| Attribute || Description ||
| initalizeData | Ignored. |
| traceOutputOptions | Appended to output, separated by the specified delimiter. |
| baseFileName | Default is the application executable name. |
| customLocation | Applies if location is Custom; default is Application.UserAppDataPath. |
| delimiter | Default is tab delimited (in XML this is "&#x9;"). |
| encoding | Default is UTF8, but may use any named encoding supported by Encoding.GetEncoding(). |
| logFileCreationSchedule | Rotates logs on daily or weekly basis by appending the date. |

## Example Config

**Note:** You may need to change the version number of Visual Basic based on the .NET version you are using.

{code:xml}
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <system.diagnostics>
    <sharedListeners>
      <add name="filelog"
        type="Microsoft.VisualBasic.Logging.FileLogTraceListener, Microsoft.VisualBasic, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
        traceOutputOptions="DateTime,ProcessId,ThreadId"
        customLocation="Logs"
        location="Custom"
        logFileCreationSchedule="Daily" />
    </sharedListeners>
    <sources>
      <source name="ExampleSource" switchValue="All">
        <listeners>
          <clear />
          <add name="filelog" />
        </listeners>
      </source>
    </sources>
  </system.diagnostics>
</configuration>
{code:xml}

## Example Output

Download the Complete package with examples and see the FileLog subfolder in the HelloLogging example.

## Config Template

{code:xml}
<!-- Note: You may need to change the version number of Visual Basic based on the .NET version you are using. -->
<add name="filelog"
  type="Microsoft.VisualBasic.Logging.FileLogTraceListener, Microsoft.VisualBasic, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
  initalizeData=""
  traceOutputOptions="Callstack,LogicalOperationStack,DateTime,ProcessId,ThreadId,Timestamp,"
  append="true|false"
  autoFlush="false|true"
  baseFileName=""
  customLocation=""
  delimiter="&#x9;"
  diskSpaceExhaustedBehavior="DiscardMessages|ThrowException"
  encoding="UTF8|encoding name"
  includeHostName="false|true"
  location="LocalUserApplicationDirectory|TempDirectory|CommonApplicationDirectory|ExecutableDirectory|Custom"
  logFileCreationSchedule="None|Daily|Weekly"
  maxFileSize="5000000"
  reserveDiskSpace="10000000"
  />
{code:xml}
