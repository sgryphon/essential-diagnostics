using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Data.Common;
using Essential.Data;
using System.Reflection;

namespace Essential.Diagnostics
{
    /// <summary>
    /// Trace listener that writes to a database.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This listener writes to the database table created by
    /// the diagnostics_regsql tool (via the stored procedure
    /// created by the tool).
    /// </para>
    /// </remarks>
    public class SqlDatabaseTraceListener : TraceListenerBase
    {
        //public const string DefaultTable = "diagnostics_Trace";
        string _connectionName;
        const string _defaultApplicationName = "";
        private static string[] _supportedAttributes = new string[] 
            { 
                "applicationName", "ApplicationName", "applicationname", 
            };

        /// <summary>
        /// Constructor with initializeData.
        /// </summary>
        /// <param name="connectionName">connection string of the database to write to</param>
        public SqlDatabaseTraceListener(string connectionName)
        {
            _connectionName = connectionName;
        }

        /// <summary>
        /// Gets or sets the name of the application used when logging to the database.
        /// </summary>
        public string ApplicationName
        {
            get
            {
                // Default format matches System.Diagnostics.TraceListener
                if (Attributes.ContainsKey("applicationname"))
                {
                    return Attributes["applicationname"];
                }
                else
                {
                    return _defaultApplicationName;
                }
            }
            set
            {
                Attributes["applicationname"] = value;
            }
        }

        /// <summary>
        /// Allowed attributes for this trace listener.
        /// </summary>
        protected override string[] GetSupportedAttributes()
        {
            return _supportedAttributes;
        }

        /// <summary>
        /// Write trace event with data.
        /// </summary>
        protected override void WriteTrace(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message, Guid? relatedActivityId, object[] data)
        {
            string dataString = null;
            if (data != null)
            {
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    if (i != 0)
                    {
                        builder.Append(", ");
                    }
                    if (data[i] != null)
                    {
                        builder.Append(data[i]);
                    }
                }
                dataString = builder.ToString();
            }
            WriteToDatabase(eventCache, source, eventType, id, message, relatedActivityId, dataString);
        }

        private void WriteToDatabase(TraceEventCache eventCache, string source, TraceEventType eventType, int? id, string message, Guid? relatedActivityId, string dataString)
        {
            const string sql = "EXEC diagnostics_Trace_AddEntry " +
               "@ApplicationName, @TraceSource, @EventId, @Severity, @LogTimeUtc, " +
               "@MachineName, @AppDomainName, @ProcessId, @ThreadName, " +
               "@MessageText, @ActivityId, @RelatedActivityId, @LogicalOperationStack, " + 
               "@Data;";

            DateTime logTime;
            string logicalOperationStack = null;
            if (eventCache != null)
            {
                logTime = eventCache.DateTime.ToUniversalTime();
                if( eventCache.LogicalOperationStack != null && eventCache.LogicalOperationStack.Count > 0 )
                {
                    StringBuilder stackBuilder = new StringBuilder();
                    foreach (object o in eventCache.LogicalOperationStack)
                    {
                        if( stackBuilder.Length > 0 ) stackBuilder.Append(", ");
                        stackBuilder.Append(o);
                    }
                    logicalOperationStack = stackBuilder.ToString();
                }
            }
            else
            {
                logTime = DateTimeOffset.UtcNow.UtcDateTime;
            }

            // Truncate message
            if (message != null && message.Length > 1500)
            {
                message = message.Substring(0, 1497) + "...";
            }

            ConnectionStringSettings connectionSettings = ConfigurationManager.ConnectionStrings[_connectionName];
            DbProviderFactory dbFactory = DbProviderFactories.GetFactory(connectionSettings.ProviderName);
            using (var connection = dbFactory.CreateConnection(connectionSettings.ConnectionString))
            {
                // TODO: Would it be more efficient to create command & params once, then set value & reuse?
                // (But would need to synchronise threading)
                var command = dbFactory.CreateCommand(sql, connection);

                command.Parameters.Add(dbFactory.CreateParameter("@ApplicationName", source != null ? (object)source : DBNull.Value));
                command.Parameters.Add(dbFactory.CreateParameter("@TraceSource", source != null ? (object)source : DBNull.Value));
                command.Parameters.Add(dbFactory.CreateParameter("@EventId", id ?? 0));
                command.Parameters.Add(dbFactory.CreateParameter("@Severity", eventType.ToString()));
                command.Parameters.Add(dbFactory.CreateParameter("@LogTimeUtc", logTime));
                command.Parameters.Add(dbFactory.CreateParameter("@MachineName", Environment.MachineName));
                command.Parameters.Add(dbFactory.CreateParameter("@AppDomainName", AppDomain.CurrentDomain.FriendlyName));
                command.Parameters.Add(dbFactory.CreateParameter("@ProcessId", eventCache != null ? (object)eventCache.ProcessId : 0));
                command.Parameters.Add(dbFactory.CreateParameter("@ThreadName", eventCache != null ? (object)eventCache.ThreadId : DBNull.Value));
                command.Parameters.Add(dbFactory.CreateParameter("@MessageText", message != null ? (object)message : DBNull.Value));
                command.Parameters.Add(dbFactory.CreateParameter("@ActivityId", Trace.CorrelationManager.ActivityId != Guid.Empty ? (object)Trace.CorrelationManager.ActivityId : DBNull.Value ));
                command.Parameters.Add(dbFactory.CreateParameter("@RelatedActivityId", relatedActivityId.HasValue ? (object)relatedActivityId.Value : DBNull.Value));
                command.Parameters.Add(dbFactory.CreateParameter("@LogicalOperationStack", logicalOperationStack != null ? (object)logicalOperationStack : DBNull.Value));
                command.Parameters.Add(dbFactory.CreateParameter("@Data", dataString != null ? (object)dataString : DBNull.Value));

                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }

}
