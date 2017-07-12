[Home](../ReadMe.md) | [Index](Index.md) | [Examples](Examples.md) | [Guidance](Guidance.md) | [FAQ](FAQ.md) | [Listeners](Listeners.md) | [Filters](Filters.md) | [Extensions](Extensions.md)

# Integration

System.Diagnostics is already integrated with many parts of the .NET Framework, so you can include framework tracing within your application tracing.

## System Defined Sources Summary

See below for details

| Source | Description |
| ------ | ----------- |
| CardSpace | Tracing for CardSpace. |
| System.IdentityModel | Good for debugging Windows Identity Foundation. Old source was Microsoft.IdentityModel. |
| System.IO.Log | Logging for the .NET Framework interface to the Common Log File System (CLFS). |
| System.Net | Network operations, good for debugging connection problems, e.g. email sending. |
| System.Net.Sockets | More detailed network operations. |
| System.Runtime.Serialization | Logs when objects are read or written. |
| System.ServiceModel | Logs all stages of WCF processin. Important to include the ActivityTracing for cross-tier correlation and set propagateActivity. |
| System.ServiceModel.Activation | WCF tracing. |
| System.ServiceModel.MessageLogging | WCF detailed message dumps; needs to also be turned on in WCF settings (see example). |

## Example Config

Example config sections, listing the sources only (but not configured with any listeners), plus the additional settings needed for WCF tracing.

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <system.diagnostics>
    <!-- Also need to define listeners and link to the sources; can use the same listeners as your own code -->
    <sources>
      <source name="System.ServiceModel"
              switchValue="Information, ActivityTracing"
              propagateActivity="true">
        <listeners>
          <clear />
        </listeners>
      </source>
      <source name="System.ServiceModel.MessageLogging">
        <listeners>
          <clear />
        </listeners>
      </source>
      <source name="Microsoft.IdentityModel" switchValue="Off" />
      <source name="CardSpace" switchValue="Off" />
      <source name="System.IO.Log" switchValue="Off" />
      <source name="System.Net" switchValue="Off" />
      <source name="System.Net.Sockets" switchValue="Off" />
      <source name="System.Runtime.Serialization" switchValue="Off" />
    </sources>
  </system.diagnostics>
  <system.serviceModel>
    <diagnostics>
      <messageLogging
           logEntireMessage="true"
           logMalformedMessages="false"
           logMessagesAtServiceLevel="true"
           logMessagesAtTransportLevel="false"
           maxMessagesToLog="3000"
           maxSizeOfMessageToLog="2000">
      </messageLogging>
    </diagnostics>
  </system.serviceModel>
</configuration>
```

## Detail And References

### Windows Communication Foundation (WCF)

WCF supports tracing, including correlated tracing across multiple tiers using the Service Trace Viewer.

See:

* [http://msdn.microsoft.com/en-us/library/ms730064.aspx](http://msdn.microsoft.com/en-us/library/ms730064.aspx)

### Windows Identity Foundation (WIF)

Tracing can be turned on to troubleshoot WIF federated sign on issues.

See:

* [http://msdn.microsoft.com/en-us/library/ee517282.aspx](http://msdn.microsoft.com/en-us/library/ee517282.aspx)
* [http://blogs.msdn.com/b/jimmiet/archive/2010/09/02/10057532.aspx](http://blogs.msdn.com/b/jimmiet/archive/2010/09/02/10057532.aspx)

### System.Net.Mail

For an example of the switches to turn on the trace System.Net.Mail SMTP messages.

See:

* [http://systemnetmail.com/faq/4.10.aspx](http://systemnetmail.com/faq/4.10.aspx)

### Other

There are also other sources defined for areas such as System.Runtime.Serialization.

* [http://msdn.microsoft.com/en-us/library/ms733025.aspx](http://msdn.microsoft.com/en-us/library/ms733025.aspx)
