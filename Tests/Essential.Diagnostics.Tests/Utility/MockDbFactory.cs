using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Collections;

namespace Essential.Diagnostics.Tests.Utility
{
    public class MockDbFactory : DbProviderFactory
    {
        // Fields
        public static readonly MockDbFactory Instance = new MockDbFactory();

        public Queue<DbCommand> CommandQueue = new Queue<DbCommand>();

        public override DbConnection CreateConnection()
        {
            var connection = new MockConnection();
            return connection;
        }

        public override DbCommand CreateCommand()
        {
            var command = CommandQueue.Dequeue();
            return command;
        }

        public override DbParameter CreateParameter()
        {
            var parameter = new MockParameter();
            return parameter;
        }
    }
}
