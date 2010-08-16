using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

namespace Essential.Data
{
    public static class DbProviderFactoryExtensions
    {
        public static DbCommand CreateCommand(this DbProviderFactory dbFactory, string commandText, DbConnection connection)
        {
            var command = dbFactory.CreateCommand();
            command.CommandText = commandText;
            command.Connection = connection;
            return command;
        }

        public static DbConnection CreateConnection(this DbProviderFactory dbFactory, string connectionString)
        {
            var connection = dbFactory.CreateConnection();
            //connection.ConnectionString = connectionString;
            return connection;
        }

        public static DbParameter CreateParameter(this DbProviderFactory dbFactory, string parameterName, object value)
        {
            var parameter = dbFactory.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.Value = value;
            return parameter;
        }

    }
}
