using NetMud.DataStructure.Base.System;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace NetMud.DataAccess
{
    public class DataWrapper
    {
        //Not even sure what to do here yet
        public DataWrapper()
        {
        }

        public IEnumerable<T> GetAll<T>() where T : IData
        {
            var returnList = new List<T>();
            var sql = string.Format("select * from [dbo].[{0}]", typeof(T).Name);

            try
            {
                var ds = SqlWrapper.RunDataset(sql, CommandType.Text);

                if (ds.Rows != null)
                {
                    foreach (DataRow dr in ds.Rows)
                    {
                        var newValue = Activator.CreateInstance(typeof(T)) as IData;
                        newValue.Fill(dr);
                        returnList.Add((T)newValue);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return returnList;
        }

        public IEnumerable<T> GetAllBySharedKey<T>(string sharedKeyName, string sharedKeyValue) where T : IData
        {
            var returnList = new List<T>();
            var parms = new Dictionary<string, object>();
            var sql = string.Format("select * from [dbo].[{0}] where {1} = @value", typeof(T).Name, sharedKeyName, sharedKeyValue);

            parms.Add("value", sharedKeyValue);

            try
            {
                var ds = SqlWrapper.RunDataset(sql, CommandType.Text, parms);

                if (ds.Rows != null)
                {
                    foreach (DataRow dr in ds.Rows)
                    {
                        var newValue = Activator.CreateInstance(typeof(T)) as IData;
                        newValue.Fill(dr);
                        returnList.Add((T)newValue);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return returnList;
        }

        public T GetOneBySharedKey<T>(string sharedKeyName, string sharedKeyValue) where T : IData
        {
            IData returnValue = default(T);
            var parms = new Dictionary<string, object>();
            var sql = string.Format("select * from [dbo].[{0}] where {1} = @value", typeof(T).Name, sharedKeyName, sharedKeyValue);

            parms.Add("value", sharedKeyValue);

            try
            {
                var ds = SqlWrapper.RunDataset(sql, CommandType.Text, parms);

                if (ds.Rows != null)
                {
                    if (ds.Rows.Count > 1)
                        throw new InvalidOperationException("More than one row returned for shared key.");

                    foreach (DataRow dr in ds.Rows)
                    {
                        returnValue = Activator.CreateInstance(typeof(T)) as IData;
                        returnValue.Fill(dr);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return (T)returnValue;
        }

        public T GetOne<T>(long id) where T : IData
        {
            IData returnValue = default(T);
            var sql = string.Format("select * from [dbo].[{0}] where ID = {1}", typeof(T).Name, id);

            try
            {
                var ds = SqlWrapper.RunDataset(sql, CommandType.Text);

                if (ds.Rows != null)
                {
                    foreach (DataRow dr in ds.Rows)
                    {
                        returnValue = Activator.CreateInstance(typeof(T)) as IData;
                        returnValue.Fill(dr);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return (T)returnValue;
        }

        private string ScrubTypeName(Type t)
        {
            if (t.IsInterface)
                return t.Name.Substring(1);

            return t.Name;
        }
    }
}
