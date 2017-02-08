using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;

namespace Essential.Diagnostics.Tests.Utility
{
    public class MockConnection : DbConnection
    {
        ConnectionState state;

        public override string ConnectionString { get; set; }

        public override void Open()
        {
            state = ConnectionState.Open;
        }

        public override void Close()
        {
            state = ConnectionState.Closed;
        }

        public override ConnectionState State { get { return state; } }

        private void SetState(ConnectionState state)
        {
            var originalState = this.state;
            this.state = state;
            var e = new StateChangeEventArgs(originalState, state);
            OnStateChange(e);
        }

        // Not implemented

        protected override DbTransaction BeginDbTransaction(System.Data.IsolationLevel isolationLevel)
        {
            throw new NotImplementedException();
        }

        public override void ChangeDatabase(string databaseName)
        {
            throw new NotImplementedException();
        }

        protected override DbCommand CreateDbCommand()
        {
            throw new NotImplementedException();
        }

        public override string DataSource
        {
            get { throw new NotImplementedException(); }
        }

        public override string Database
        {
            get { throw new NotImplementedException(); }
        }

        public override string ServerVersion
        {
            get { throw new NotImplementedException(); }
        }

    }
}
