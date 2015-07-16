using NetMud.DataStructure.Base.System;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var sql = String.Format("select * from [dbo].[{0}]", typeof(T).Name);

            var ds = SqlWrapper.RunDataset(sql, CommandType.Text);

            if (ds.HasErrors)
            {
                //TODO: Error handling logging?
            }
            else if(ds.Rows != null)
            {
                foreach (DataRow dr in ds.Rows)
                {
                    try
                    {
                        var newValue = Activator.CreateInstance(typeof(T)) as IData;
                        newValue.Fill(dr);
                        returnList.Add((T)newValue);
                    }
                    catch
                    {
                        //error logging
                    }
                }
            }

            return returnList;
        }

        public IEnumerable<T> GetAllBySharedKey<T>(string sharedKeyName, string sharedKeyValue) where T : IData
        {
            var returnList = new List<T>();
            var sql = String.Format("select * from [dbo].[{0}] where {1} = '{2}'", typeof(T).Name, sharedKeyName, sharedKeyValue);

            var ds = SqlWrapper.RunDataset(sql, CommandType.Text);

            if (ds.HasErrors)
            {
                //TODO: Error handling logging?
            }
            else if (ds.Rows != null)
            {
                foreach (DataRow dr in ds.Rows)
                {
                    try
                    {
                        var newValue = Activator.CreateInstance(typeof(T)) as IData;
                        newValue.Fill(dr);
                        returnList.Add((T)newValue);
                    }
                    catch
                    {
                        //error logging
                    }
                }
            }

            return returnList;
        }

        public T GetOne<T>(long id) where T : IData
        {
            IData returnValue = default(T);
            var sql = String.Format("select * from [dbo].[{0}] where ID = {1}", typeof(T).Name, id);

            var ds = SqlWrapper.RunDataset(sql, CommandType.Text);

            if (ds.HasErrors)
            {
                //TODO: Error handling logging?
            }
            else if (ds.Rows != null)
            {
                foreach (DataRow dr in ds.Rows)
                {
                    try
                    {
                        returnValue = Activator.CreateInstance(typeof(T)) as IData;
                        returnValue.Fill(dr);
                    }
                    catch
                    {
                        //error logging
                    }
                }
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
