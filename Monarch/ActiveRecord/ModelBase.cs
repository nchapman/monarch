using System.Collections.Generic;
using System.Data.SqlClient;
using Monarch.ActiveSupport;

namespace Monarch.ActiveRecord
{
    public class ModelBase : Boo.Lang.IQuackFu
    {
        #region Fields

        protected List<string> changedColumns = new List<string>();
        protected Dictionary<string, object> data = new Dictionary<string, object>();

        #endregion

        #region Instance Indexers

        public object this[string key]
        {
            get
            {
                return data[key];
            }
            set
            {
                if (!changedColumns.Contains(key) && data[key] != value)
                    changedColumns.Add(key);

                data[key] = value;
            }
        }

        #endregion

        #region Instance Properties

        public List<string> ChangedColumns
        {
            get { return changedColumns; }
        }

        public Dictionary<string, object> Data
        {
            get { return data; }
            set { data = value; }
        }

        #endregion

        #region Instance Methods

        protected string[] GetDataKeysArray()
        {
            var keys = new List<string>();

            foreach (var key in data.Keys)
                keys.Add(key);

            return keys.ToArray();
        }

        protected object[] GetDataValuesArray()
        {
            var values = new List<object>();

            foreach (var value in data.Values)
                values.Add(value);

            return values.ToArray();
        }

        #endregion

        #region IQuackFu Members

        public object QuackGet(string name, object[] parameters)
        {
            return this[name];
        }

        public object QuackSet(string name, object[] parameters, object value)
        {
            this[name] = value;

            return value;
        }

        public object QuackInvoke(string name, params object[] args)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region Class Methods

        protected static SqlConnection GetConnection()
        {
            return new SqlConnection(Configuration.ConnectionString);
        }

        #endregion
    }
}
