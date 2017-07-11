# PropertyFilter Class

Trace filter that filters based on the value of a property.

## Remarks

**Note**: The functionality of this filter is superseeded by [ExpressionFilter](ExpressionFilter) and it is only maintained for backwards compatibility.

The initializeData for this filter contains a single property comparison, e.g. initializeData="Id == 1".

The value must consist of a property name from one of the values supported by [TraceFormatter](TraceFormatter), the C# equality operator (double equals signs) and then a single value. Matching quotes around the value are removed and whitespace (outside of any quotes) is ignored, e.g. "Id == 1" and " Id == '1' " are equivalent.

The property is converted to a string using [TraceFormatter](TraceFormatter) and then compared with the value.
	
## Config Attributes

|| Attribute || Description ||
| initializeData | Property name and value to compare to, e.g. " EventType == 'Warning' ". |

## Example Config

{code:xml}
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <system.diagnostics>
    <sharedListeners>
      <add name="coloredconsole"
           type="Essential.Diagnostics.ColoredConsoleTraceListener, Essential.Diagnostics">
        <filter type="Essential.Diagnostics.PropertyFilter, Essential.Diagnostics"
                initializeData="Id == 1001" />
      </add>
    </sharedListeners>
    <sources>
      <source name="ExampleSource" switchValue="All">
        <listeners>
          <clear />
          <add name="coloredconsole" />
        </listeners>
      </source>
    </sources>
  </system.diagnostics>
</configuration>
{code:xml}

