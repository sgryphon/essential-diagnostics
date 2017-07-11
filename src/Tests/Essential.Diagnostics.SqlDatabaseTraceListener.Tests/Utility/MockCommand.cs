using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Collections.ObjectModel;

namespace Essential.Diagnostics.Tests.Utility
{
    public class MockCommand : DbCommand
    {
        private MockParameterCollection parameterCollection = new MockParameterCollection();
        private List<IDictionary<string, string>> commandsExecuted = new List<IDictionary<string, string>>();

        public IList<IDictionary<string, string>> MockCommandsExecuted
        {
            get { return commandsExecuted; }
        }

        public override string CommandText { get; set; }

        protected override DbConnection DbConnection { get; set; }

        protected override DbParameterCollection DbParameterCollection
        {
            get { return parameterCollection; }
        }

        public override int ExecuteNonQuery()
        {
            var commandDetails = new Dictionary<string, string>();
            commandDetails.Add("CommandText", CommandText);
            foreach (DbParameter parameter in Parameters)
            {
                commandDetails.Add(parameter.ParameterName, parameter.Value.ToString());
            }
            commandsExecuted.Add(commandDetails);
            return 0;
        }


        // Not implemented

        public override void Cancel()
        {
            throw new NotImplementedException();
        }

        public override int CommandTimeout
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override CommandType CommandType
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        protected override DbParameter CreateDbParameter()
        {
            throw new NotImplementedException();
        }

        protected override DbTransaction DbTransaction
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override bool DesignTimeVisible
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            throw new NotImplementedException();
        }

        public override object ExecuteScalar()
        {
            throw new NotImplementedException();
        }

        public override void Prepare()
        {
            throw new NotImplementedException();
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        // Inner classes

        class MockParameterCollection : DbParameterCollection
        {
            private DbParameterKeyedCollection parameters = new DbParameterKeyedCollection();

            public override int Add(object value)
            {
                DbParameter parameter = (DbParameter)value;
                parameters.Add(parameter);
                return 0;
            }

            public override int Count
            {
                get { return parameters.Count; }
            }

            public override System.Collections.IEnumerator GetEnumerator()
            {
                return parameters.GetEnumerator();
            }

            protected override DbParameter GetParameter(string parameterName)
            {
                return parameters[parameterName];
            }

            protected override DbParameter GetParameter(int index)
            {
                return parameters[index];
            }


            // Not implemented

            public override void AddRange(Array values)
            {
                throw new NotImplementedException();
            }

            public override void Clear()
            {
                throw new NotImplementedException();
            }

            public override bool Contains(string value)
            {
                throw new NotImplementedException();
            }

            public override bool Contains(object value)
            {
                throw new NotImplementedException();
            }

            public override void CopyTo(Array array, int index)
            {
                throw new NotImplementedException();
            }

            public override int IndexOf(string parameterName)
            {
                throw new NotImplementedException();
            }

            public override int IndexOf(object value)
            {
                throw new NotImplementedException();
            }

            public override void Insert(int index, object value)
            {
                throw new NotImplementedException();
            }

            public override bool IsFixedSize
            {
                get { throw new NotImplementedException(); }
            }

            public override bool IsReadOnly
            {
                get { throw new NotImplementedException(); }
            }

            public override bool IsSynchronized
            {
                get { throw new NotImplementedException(); }
            }

            public override void Remove(object value)
            {
                throw new NotImplementedException();
            }

            public override void RemoveAt(string parameterName)
            {
                throw new NotImplementedException();
            }

            public override void RemoveAt(int index)
            {
                throw new NotImplementedException();
            }

            protected override void SetParameter(string parameterName, DbParameter value)
            {
                throw new NotImplementedException();
            }

            protected override void SetParameter(int index, DbParameter value)
            {
                throw new NotImplementedException();
            }

            public override object SyncRoot
            {
                get { throw new NotImplementedException(); }
            }
        }

        class DbParameterKeyedCollection : KeyedCollection<string, DbParameter>
        {
            protected override string GetKeyForItem(DbParameter item)
            {
                return item.ParameterName;
            }
        }

    }
}
