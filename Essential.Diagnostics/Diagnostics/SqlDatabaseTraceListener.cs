using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Data.SqlClient;
using System.Configuration;

namespace Essential.Diagnostics
{
    public class SqlDatabaseTraceListener : TraceListenerBase
    {
        public const string DefaultTable = "TraceLog";
        string _connectionName;
        string _tableName;

        public SqlDatabaseTraceListener(string connectionName)
        {
            _connectionName = connectionName;
        }

        public SqlDatabaseTraceListener(string connectionName, string tableName)
        {
            _connectionName = connectionName;
            _tableName = tableName;
        }

        public string TableName
        {
            get
            {
                lock (this)
                {
                    if (_tableName == null)
                    {
                        if (base.Attributes.ContainsKey("tableName"))
                        {
                            _tableName = base.Attributes["tableName"];
                        }
                        else
                        {
                            _tableName = DefaultTable;
                        }
                    }
                }
                return _tableName;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("TableName");
                }
                if (value.Length == 0)
                {
                    throw new ArgumentOutOfRangeException("TableName");
                }
                lock (this)
                {
                    this._tableName = value;
                }
            }
        }

        protected override string[] GetSupportedAttributes()
        {
            return new string[] { "tableName" };
        }

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
            const string sql = "INSERT INTO [dbo].[TraceLog] " +
               "( [Source], [EventID], [Severity], [LogTimeUtc], " +
               "[MachineName], [AppDomainName], [ProcessID], [ThreadName], " +
               "[Message], [ActivityId], [RelatedActivityId], [LogicalOperationStack], " +
               "[Data] ) " +
               "VALUES " +
               "( @source, @eventId, @severity, @logTimeUtc, " +
               "@machineName, @appDomainName, @processID, @threadName, " +
               "@message, @activityId, @relatedActivityId, @logicalOperationStack, " + 
               "@data ) ";

            ConnectionStringSettings connectionSettings = ConfigurationManager.ConnectionStrings[_connectionName];

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

            using (SqlConnection connection = new SqlConnection(connectionSettings.ConnectionString))
            {
                SqlCommand command = new SqlCommand(sql, connection);

                command.Parameters.AddWithValue("@source", source != null ? (object)source : DBNull.Value);
                command.Parameters.AddWithValue("@eventId", id.HasValue ? (object)id.Value : DBNull.Value);
                command.Parameters.AddWithValue("@severity", eventType.ToString());
                command.Parameters.AddWithValue("@logTimeUtc", logTime);
                command.Parameters.AddWithValue("@machineName", Environment.MachineName);
                command.Parameters.AddWithValue("@appDomainName", AppDomain.CurrentDomain.FriendlyName);
                command.Parameters.AddWithValue("@processId", eventCache != null ? (object)eventCache.ProcessId : DBNull.Value);
                command.Parameters.AddWithValue("@threadName", eventCache != null ? (object)eventCache.ThreadId : DBNull.Value);
                command.Parameters.AddWithValue("@message", message != null ? (object)message : DBNull.Value);
                command.Parameters.AddWithValue("@activityId", Trace.CorrelationManager.ActivityId != Guid.Empty ? (object)Trace.CorrelationManager.ActivityId : DBNull.Value );
                command.Parameters.AddWithValue("@relatedActivityId", relatedActivityId.HasValue ? (object)relatedActivityId.Value : DBNull.Value);
                command.Parameters.AddWithValue("@logicalOperationStack", logicalOperationStack != null ? (object)logicalOperationStack : DBNull.Value);
                command.Parameters.AddWithValue("@data", dataString != null ? (object)dataString : DBNull.Value);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }

}
