using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using Monarch.ActiveSupport;

namespace Monarch.ActiveRecord
{
    public class Model<T> : ModelBase
    {
        #region Instance Methods

        public bool Save()
        {
            var dataKeys = changedColumns.ToArray();
            var dataValues = new List<object>();

            foreach (var key in dataKeys)
                dataValues.Add(data[key]);

            return !Data.ContainsKey("ID") ? Insert(dataKeys, dataValues.ToArray()) : Update(dataKeys, dataValues.ToArray(), FormatSql("ID = {0}", Data["ID"]));
        }

        #endregion

        #region Class Methods

        public static string Escape(string input)
        {
            return input.Replace("'", "''");
        }

        public static string[] EscapeCollection(IEnumerable collection, bool quote)
        {
            var toReturn = new List<string>();

            foreach (var input in collection)
            {
                if (quote)
                    toReturn.Add("'" + Escape(input.ToString()) + "'");
                else
                    toReturn.Add(Escape(input.ToString()));
            }

            return toReturn.ToArray();
        }

        public static int ExecuteNonQuery(string sql)
        {
            using (var connection = GetConnection())
            {
                connection.Open();

                var command = new SqlCommand(sql, connection);

                return command.ExecuteNonQuery();
            }
        }

        public static T Find(object primaryKey)
        {
            return FindBySql(FormatSql("SELECT * FROM {0} WHERE ID = '{1}'", GetTableName(), primaryKey));
        }

        public static T[] FindAll()
        {
            return FindAllBySql(FormatSql("SELECT * FROM {0}", GetTableName()));
        }

        public static T[] FindAll(string where)
        {
            return FindAllBySql(FormatSql("SELECT * FROM {0} WHERE {1}", GetTableName(), where));
        }

        public static T[] FindAll(string where, string orderBy)
        {
            return FindAllBySql(FormatSql("SELECT * FROM {0} WHERE {1} ORDER BY {2}", GetTableName(), where, orderBy));
        }

        public static T[] FindAllBySql(string sql)
        {
            var results = new List<T>();

            using (var connection = GetConnection())
            {
                connection.Open();

                var command = new SqlCommand(sql, connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var model = (T) Activator.CreateInstance(typeof(T));

                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            (model as ModelBase).Data.Add(reader.GetName(i), reader.GetValue(i));
                        }

                        results.Add(model);
                    }
                }
            }

            return results.ToArray();
        }

        public static T FindBySql(string sql)
        {
            var results = FindAllBySql(sql);

            if (null != results && results.Length > 0)
                return results[0];

            return default(T);
        }

        public static string FormatSql(string sql, params object[] args)
        {
            if (null != args && args.Length > 0)
            {
                return string.Format(sql, EscapeCollection(args, false));
            }

            return sql;
        }

        public static string GetTableName()
        {
            return Inflector.Pluralize(typeof (T).Name);
        }

        public static bool Insert(string[] columns, object[] values)
        {
            var joinedColumns = string.Join(", ", columns);
            var joinedValues = string.Join(", ", EscapeCollection(values, true));

            return ExecuteNonQuery(string.Format("INSERT INTO {0} ({1}) VALUES ({2})", GetTableName(), joinedColumns, joinedValues)) == 1;
        }

        public static bool Update(string[] columns, object[] values, string where)
        {
            var pairs = "";

            for (var i = 0; i < columns.Length; i++)
                if ("ID" != columns[i])
                    pairs += FormatSql("{0} = '{1}',", columns[i], values[i]);

            pairs = pairs.TrimEnd(",".ToCharArray());

            return ExecuteNonQuery(string.Format("UPDATE {0} SET {1} WHERE {2}", GetTableName(), pairs, where)) > 0;
        }

        #endregion
    }
}
 