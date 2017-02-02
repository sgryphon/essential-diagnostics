using System;
using System.Data.Common;

namespace Essential.Data
{
    /// <summary>
    /// Extensions to the DbProvideFactory class with factory methods that create
    /// database objects and set basic properties.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public static class DbProviderFactoryExtensions
    {
        /// <summary>
        /// Create a DbCommand for the specified command text, with the specified connection set.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static DbCommand CreateCommand(DbProviderFactory dbFactory, string commandText, DbConnection connection)
        //public static DbCommand CreateCommand(this DbProviderFactory dbFactory, string commandText, DbConnection connection)
        {
            if (dbFactory == null) throw new ArgumentNullException("dbFactory");

            var command = dbFactory.CreateCommand();
            command.CommandText = commandText;
            command.Connection = connection;
            return command;
        }

        /// <summary>
        /// Create a DbConnection with the specified connection string set.
        /// </summary>
        public static DbConnection CreateConnection(DbProviderFactory dbFactory, string connectionString)
        //public static DbConnection CreateConnection(this DbProviderFactory dbFactory, string connectionString)
        {
            if (dbFactory == null) throw new ArgumentNullException("dbFactory");

            var connection = dbFactory.CreateConnection();
            connection.ConnectionString = connectionString;
            return connection;
        }

        /// <summary>
        /// Create a DbParameter with the specified name and value.
        /// </summary>
        public static DbParameter CreateParameter(DbProviderFactory dbFactory, string parameterName, object value)
        //public static DbParameter CreateParameter(this DbProviderFactory dbFactory, string parameterName, object value)
        {
            if (dbFactory == null) throw new ArgumentNullException("dbFactory");

            var parameter = dbFactory.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.Value = value;
            return parameter;
        }

    }
}
