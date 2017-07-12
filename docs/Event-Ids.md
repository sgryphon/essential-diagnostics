[Home](../ReadMe.md) | [Index](Index.md) | [Examples](Examples.md) | [Guidance](Guidance.md) | [FAQ](FAQ.md) | [Listeners](Listeners.md) | [Filters](Filters.md) | [Extensions](Extensions.md)

# Theory of Event IDs

The "Theory of Reply Codes" was a proposition in early RFC's (particularly SMTP, FTP and HTTP) which provided some structure around reply codes. This organised codes into a hierarchy where the first digit indicated the general class of the response and so on, provide some structure to the values.

A similar theory can be applied to Windows event codes / error codes to allow generic handling: Event IDs four digits long can be used, with the first two digits (a and b) following a structured format, allowing the remaining two digits for the specific event.

As the Event ID's are structured (and not repeated), so the Category ID is not needed as a separate value (unless a very large number of event ID's were needed, in which case the system can be partitioned by category).

The last two digits (xx) are assigned by the application as needed, either sequential (01, 02, 03, etc), or as groups of related functions.

Note that this theory in primarily intended to apply for significant (infrequent) events that are logged to the Windows Event Log, and so does not cover verbose or more detailed (frequent) logging or tracing. Event IDs, and the Windows Event Log, are particularly important for Windows Services and other applications that do not have a frequently used user interface. For service applications the main feedback of system health and operation is via monitoring of the Windows Event Log, and so it is important the event IDs are easy to use.

## First Digit (Type)

The following standard values are used for the first digit:

| Event ID | Meaning | Example | Event Type |
| -------- | ------- | ------- | ---------- |
| 1bxx | Positive Occasional Preliminary | Service starting | Information |
| 2bxx | Positive Occasional Completion | Connection made, or user logged on | Information |
| 3bxx | Positive Frequent Intermediate | Command received | (Application Log) |
| 4bxx | Transient Negative | Warning | Warning |
| 5bxx | Permanent Negative | Error | Error |
| 6bxx |  |  |  |
| 7bxx |  |  |  | 					
| 8bxx | Positive Occasional Finalization | Service stopped | Information |
| 9bxx | Unknown (Error) | Unhandled exception | Error (Critical) |

Although it might seem redundant to use the first digit to indicate the event type, you are going to want to use separate Event ID's anyway (a system with an error "4801" and a separate warning "4801", with the same number but different meanings, would be very confusing).

Event IDs are also structured so that typical operation of the system should have a recognisable sequence pattern -- when the system starts you will see 1bxx events, then 2bxx events; during operation you may see the occassional 4bxx warning leading up to a 5bxx error; eventually when the system is shut down you will see 8bxx events; hopefully you never see a 9bxx critical error!

Intermediate 3bxx event types, if very frequent events (such as serving data requests or individual transactions) would not logged in Windows Event Log, however they would often appear in a structured application log (an example would be the IIS web logs).

Event types 6bxx and 7bxx are not standardised in this theory, but could be used for beginning (start or resume) and ending (stop or suspend) of activity tracing. These events would usually be too frequent to be included in the Windows Event Log, or even an application-specific log, but it may be useful to assign them ID values for tracing (especially structured tracing) purposes.

## Second Digit (Area)

The following standard values are used for the second digit:

| Event ID | Event Subtype / Category | Example |
| -------- | ------------------------ | ------- |
| a0xx | Syntax	| Error in syntax or configuration |
| a1xx | System Control | Status, also system-level statuses |
| a2xx | Connection | Low level communication channels	|
| a3xx | Authentication/Accounting | Authorisation failure |
| a4xx |  |  | 			
| a5xx | _Core function A_ |  |
| a6xx | _Core function B_ |  |
| a7xx | _Core function C_ |  |
| a8xx |  |  |
| a9xx | Unknown or suspect	| e.g. Unhandled general exception |

The system should be separated into up to 5 other major function areas, with values from the 4-8 range allocated.

Using the first two digits for type and area leaves the last two values -- 100 different event IDs -- for specific warning or errors (usually the limiting factor) logged in each major area of the application. If this is not enough, then five digit event IDs could be used if starting with 4 or 5 (the maximum value is 2 bytes, i.e. 65535).

Usual .NET trace event types also don't cover the Success and Failure events used in security logs. Success, such as user logged in, could be classified as occasional completion of an authentication event (e.g. 2300) whereas a failed security checks (of different types) could be a set of different warning authentication event IDs (e.g. 4301, 4302, etc).

## Implementation Considerations

Event IDs should not be scattered through the code base as "magic numbers", but should be defined in a central enumeration or simply as named constants on an EventId class.

The source code should contain explantory comments that document the structure of the codes and the specific ranges of values for the first two digits, e.g. defining the actual core functions assigned to values 4-8 of the second digit, or any other customisations of the codes used. 

### Note on Warning vs Error

The distinction between Warning and Error can be complex, and this categorisation also provides another way to look at it as transient vs permanent. This is particularly important for automatic retries (without any changes) versus permanent errors.

One issue that still remains is that a transaction-level error (i.e. permanent failure of that particular transaction) could also be viewed as a system-level warning (i.e. system will continue to function and process the next transaction).

In this case the second digit may help, I.e. a 51xx error means the system has crashed, whereas a 55xx error means only an individual transaction had a permanent error; the second situation could have alternatively been coded as a 41xx error (i.e. system is continuing). A 45xx code would mean a transient transaction warning where the transaction will be attempted again.

Another note on the concept of error vs warning, is that errors are things where the program needs to notify a human operator; when scanning the event log they should be the entries that quickly draw attention to themselves.
